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
using System.Collections.Generic;

namespace KlaudWerk.ProcessEngine.Builder
{

    /// <summary>
    /// Script Builder Class
    /// The container that aggregates information about a script
    /// </summary>
    public class ScriptBuilder<T>
    {
        private readonly T _parent;
        private ScriptLanguage _language = ScriptLanguage.None;
        private string _body;
        private readonly List<string> _references = new List<string>();
        private readonly List<string> _imports = new List<string>();

        /// <summary>
        /// Gets the script language.
        /// </summary>
        /// <value>
        /// The script language.
        /// </value>
        public ScriptLanguage ScriptLanguage => _language;

        /// <summary>
        /// Gets the script body.
        /// </summary>
        /// <value>
        /// The script body.
        /// </value>
        public string ScriptBody => _body;

        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public IReadOnlyList<string> References => _references;

        /// <summary>
        /// Gets the imports.
        /// </summary>
        /// <value>
        /// The imports.
        /// </value>
        public IReadOnlyList<string> Imports => _imports;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptBuilder[T]>"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public ScriptBuilder(T parent)
        {
            parent.NotNull("StepBuilder");
            _parent = parent;
        }

        /// <summary>
        /// Set the specified language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        public ScriptBuilder<T> Language(ScriptLanguage lang)
        {
            _language = lang;
            return this;
        }
        /// <summary>
        /// Set the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public ScriptBuilder<T> Body(string body)
        {
            _body = body;
            return this;
        }
        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <returns></returns>
        public ScriptBuilder<T> Reset()
        {
            _language = ScriptLanguage.None;
            _body = string.Empty;
            _references.Clear();
            _imports.Clear();
            return this;
        }
        /// <summary>
        /// Adds the references.
        /// </summary>
        /// <param name="references">The references.</param>
        /// <returns></returns>
        public ScriptBuilder<T> AddReferences(params string[] references)
        {
            references.NotNull("references");
            foreach (var r in references)
                if (!_references.Contains(r)) _references.Add(r);
            return this;
        }
        /// <summary>
        /// Adds the importds.
        /// </summary>
        /// <param name="imports">The imports.</param>
        /// <returns></returns>
        public ScriptBuilder<T> AddImportds(params string[] imports)
        {
            imports.NotNull("references");
            foreach (var r in imports)
                if (!_imports.Contains(r)) _imports.Add(r);
            return this;
        }
        /// <summary>
        /// Done constructing the script
        /// </summary>
        /// <returns></returns>
        public T Done()
        {
            return _parent;
        }

    }
}
