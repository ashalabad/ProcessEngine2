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
using System.Linq;

namespace KlaudWerk.ProcessEngine.Builder
{
    /// <summary>
    /// Security Account Typeq
    /// </summary>
    public enum AccountTypeEnum
    {
        Role=1,
        Group,
        User
    }
    /// <summary>
    /// Helps to build security attributes of the class
    /// </summary>
    public class SecurityBuilder<T>
    {
        private readonly List<Tuple<string,AccountTypeEnum>>
            _potentialOwners=new List<Tuple<string, AccountTypeEnum>>();
        private readonly List<Tuple<string,AccountTypeEnum>>
            _businessAdministrators=new List<Tuple<string, AccountTypeEnum>>();

        /// <summary>
        /// Gets the potential owners.
        /// </summary>
        /// <value>
        /// The potential owners.
        /// </value>
        public IReadOnlyList<Tuple<string, AccountTypeEnum>> PotentialOwners => _potentialOwners;

        /// <summary>
        /// Gets the business administrators.
        /// </summary>
        /// <value>
        /// The business administrators.
        /// </value>
        public IReadOnlyList<Tuple<string, AccountTypeEnum>> BusinessAdministrators => _businessAdministrators;

        private readonly T _parent;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent Builder</param>
        public SecurityBuilder(T parent)
        {
            _parent = parent;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public SecurityBuilder<T> AddPotentialOwners(params Tuple<string,AccountTypeEnum>[] owners)
        {
            owners.NotNull("owners");
            if(owners.Length==0)
                throw new ArgumentException("owners: Length cannot be 0.");
            foreach (Tuple<string,AccountTypeEnum> owner in owners)
            {
                if(!_potentialOwners.Contains(owner))
                    _potentialOwners.Add(owner);
            }
            return this;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public SecurityBuilder<T> AddBusinessAdministrators(params Tuple<string,AccountTypeEnum>[] admins)
        {
            admins.NotNull("admins");
            if(admins.Length==0)
                throw new ArgumentException("admins: Length cannot be 0.");
            foreach (Tuple<string,AccountTypeEnum> admin in admins)
            {
                if(!_businessAdministrators.Contains(admin))
                    _businessAdministrators.Add(admin);
            }
            return this;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public SecurityBuilder<T> RemovePotentialOwners(params string[] owners)
        {
            _potentialOwners.RemoveAll(p => owners.Contains(p.Item1));
            return this;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public SecurityBuilder<T> RemoveBusinessAdministrators(params string[] owners)
        {
            _businessAdministrators.RemoveAll(p => owners.Contains(p.Item1));
            return this;
        }

        public SecurityBuilder<T> Clear()
        {
            _potentialOwners.Clear();
            _businessAdministrators.Clear();
            return this;
        }
        /// <summary>
        /// Done building; Return the Parent Builder
        /// </summary>
        /// <returns></returns>
        public T Done()
        {
            return _parent;
        }
    }
}