﻿/**
The MIT License (MIT)

Copyright (c) 2016 Igor Polouektov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
  */
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KlaudWerk.ProcessEngine.Definition
{
    /// <summary>
    /// Gather "string" representation of all elements and calculate MD5
    /// </summary>
    public class Md5CalcVisitor:IProcessDefinitionVisitor
    {
        private readonly StringBuilder _sb=new StringBuilder();

        public void Visit(ActionDefinition actionDefinition)
        {
            string s = $"{actionDefinition.Name}|{actionDefinition.Description}{(actionDefinition.Skippable?'t':'f')}";
            _sb.Append(s);
        }

        public void Visit(LinkDefinition linkDefinition)
        {
            _sb.Append($"{linkDefinition.Name}|{linkDefinition.Description}|{linkDefinition.Source.StepId}|{linkDefinition.Target.StepId}");
        }

        public void Visit(StepDefinition stepDefinition)
        {
            stepDefinition.Actions?.ToList().ForEach(a=>a.Accept(this));
            stepDefinition.OnEntry?.Accept(this);
            stepDefinition.OnExit?.Accept(this);
            stepDefinition.VariablesMap?.ToList().ForEach(v=>v.Accept(this));
            stepDefinition.PotentialOwners?.ToList().ForEach(a=>a.Accept(this));
            stepDefinition.BusinessManagers?.ToList().ForEach(a=>a.Accept(this));
            stepDefinition.StepHandler?.Accept(this);
        }

        public void Visit(ScriptDefinition scriptDefinition)
        {
            string s = $"{scriptDefinition.Lang}|{scriptDefinition.Script}";
            _sb.Append(s);
            if(scriptDefinition.Imports!=null)
                _sb.Append(string.Join("|", scriptDefinition.Imports));
            if(scriptDefinition.References!=null)
                _sb.Append(string.Join("|", scriptDefinition.References));
        }

        public void Visit(SecurityDefinition securityDefinition)
        {
            _sb.Append($"{securityDefinition.Account}|{securityDefinition.AccountType}");
        }

        public void Visit(StepHandlerDefinition stepHandlerDefinition)
        {
            stepHandlerDefinition.Script?.Accept(this);
            _sb.Append(
                $"{stepHandlerDefinition.ClassFullName}|{stepHandlerDefinition.IocName}|{stepHandlerDefinition.StepHandlerType}");
        }

        public void Visit(VariableDefinition variableDefinition)
        {
            variableDefinition.HandlerDefinition?.Accept(this);
            _sb.Append($"{variableDefinition.Description}|{variableDefinition.Name}|{variableDefinition.VariableType}");
        }

        public void Visit(ConstraintDefinition constraintsDefinition)
        {
            if (constraintsDefinition == null)
                return;
            if (!string.IsNullOrEmpty(constraintsDefinition.DefaultValue))
                _sb.Append("|" + constraintsDefinition.DefaultValue);
            if(!string.IsNullOrEmpty(constraintsDefinition.MaxValue))
                _sb.Append("|" + constraintsDefinition.MaxValue);
            if(!string.IsNullOrEmpty(constraintsDefinition.MinValue))
                _sb.Append("|" + constraintsDefinition.MinValue);
            constraintsDefinition.PossibleValues?.ToList().ForEach(v=>_sb.Append("|"+v));
        }

        public void Visit(VariableMapDefinition variableMapDefinition)
        {
            _sb.Append($"{variableMapDefinition.Name}|{variableMapDefinition.Required}");
        }

        public void Visit(ActionRelationDefinition actionRelation)
        {
            _sb.Append(actionRelation.ToString());
        }

        public void Visit(ProcessDefinition processDefinition)
        {
            processDefinition.Steps?.ToList().ForEach(s=>s.Accept(this));
            processDefinition.Links?.ToList().ForEach(l=>l.Accept(this));
            processDefinition.Variables.ToList().ForEach(v=>v.Accept(this));
            _sb.Append($"{processDefinition.FlowId}|{processDefinition.Description}|{processDefinition.Name}");
        }

        public void Visit(TagDefinition tagDefinition)
        {
            _sb.Append($"{tagDefinition.Id}");
        }

        /// <summary>
        /// Calculate MD5 checksum
        /// </summary>
        /// <returns>Base64-Encoded MD5</returns>
        public string CalculateMd5()
        {
            var md5 = MD5.Create();
            byte[] hash=md5.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(_sb.ToString())));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Reset the visitor
        /// </summary>
        public void Reset()
        {
            _sb.Clear();
        }
    }
}