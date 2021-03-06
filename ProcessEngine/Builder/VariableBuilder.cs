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
using System.Collections.Generic;

namespace KlaudWerk.ProcessEngine.Builder
{
    /// <summary>
    /// Process Variables Builder
    /// </summary>
    public class VariableBuilder
    {
        private readonly ProcessBuilder _parent;
        private string _name;
        private string _description;
        private string _accounts;
        private VariableTypeEnum _type;
        private ConstraintBuilder _constraints;
        private readonly VariableHandlerBuilder _handler;
        /// <summary>
        /// Public Properties
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string VariableName => _name;
        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        /// <value>
        /// The type of the variable.
        /// </value>
        public VariableTypeEnum VariableType => _type;
        /// <summary>
        /// Gets the variable description.
        /// </summary>
        /// <value>
        /// The variable description.
        /// </value>
        public string VariableDescription => _description;
        /// <summary>
        /// Gets the variable constraints.
        /// </summary>
        /// <value>
        /// The variable constraints.
        /// </value>
        public ConstraintBuilder VariableConstraints => _constraints;
        /// <summary>
        /// Gets the variable handler.
        /// </summary>
        /// <value>
        /// The variable handler.
        /// </value>
        public VariableHandlerBuilder VariableHandler => _handler;
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableBuilder"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public VariableBuilder(ProcessBuilder parent)
        {
            _parent = parent;
            _type=VariableTypeEnum.String;
            _handler = new VariableHandlerBuilder(this);
        }

        /// <summary>
        /// Set Name 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public VariableBuilder Name(string name)
        {
            name.NotNull("name").NotEmptyString("name");
            _name = name;
            return this;
        }
        /// <summary>
        /// Set Description
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public VariableBuilder Description(string description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        /// Set Type
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public VariableBuilder Type(VariableTypeEnum t)
        {
            _type = t;
            return this;
        }
        /// <summary>
        /// Handler 
        /// </summary>
        /// <returns></returns>
        public VariableHandlerBuilder Handler()
        {
            return _handler;
        }
         /// <summary>
        /// Finish the builder
        /// </summary>
        /// <returns></returns>
        public ProcessBuilder Done()
        {
            _name.NotNull("name").NotEmptyString("name");
            _parent.AddOrReplaceVariable(this);
            return _parent;
        }

        /// <summary>
        /// Create Constraints builder
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Constraints cannot be set without a type.</exception>
        public ConstraintBuilder Constraints()
        {
            if(_type==VariableTypeEnum.None)
                throw new ArgumentException("Constraints cannot be set without a type.");
            return new ConstraintBuilder(this);
        }

        /// <summary>
        /// Sets the constraint.
        /// </summary>
        /// <param name="constraintBuilder">The constraint builder.</param>
        /// <returns></returns>
        internal VariableBuilder SetConstraint(ConstraintBuilder constraintBuilder)
        {
            _constraints = constraintBuilder;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accounts"></param>
        internal void SetAccountValues(List<string> accounts)
        {
            _accounts = string.Join(";", accounts.ToArray());
        }
    }

    /// <summary>
    /// Variable Handler builder can be used to describe a "source" for a variable selection list.
    /// </summary>
    /// <seealso cref="KlaudWerk.ProcessEngine.Builder.HandlerBuilder{KlaudWerk.ProcessEngine.Builder.VariableBuilder}" />
    public class VariableHandlerBuilder : HandlerBuilder<VariableBuilder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableHandlerBuilder"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public VariableHandlerBuilder(VariableBuilder parent) : base(parent)
        {
        }
    }
}