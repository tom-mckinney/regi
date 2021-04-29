using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Abstractions
{
    public class CommandBuilder
    {
        public enum DirectiveListType
        {
            Multiple,
            Comma,
            SemiColon
        }

        private readonly List<string> _directives = new();

        public string Build()
        {
            return string.Join(' ', _directives);
        }

        public void Add(string directive)
        {
            if (string.IsNullOrWhiteSpace(directive))
            {
                throw new InvalidOperationException("Received null value for directive");
            }

            _directives.Add(directive);
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("Received null value for directive key or value");
            }

            _directives.Add($"{key} {value}");
        }

        public void Add(string key, ICollection<string> values, DirectiveListType listType = DirectiveListType.Multiple)
        {
            if (values?.Any() == true)
            {
                if (listType == DirectiveListType.Multiple)
                {
                    foreach (var v in values)
                    {
                        Add(key, v);
                    }
                }
                else
                {
                    throw new NotImplementedException(); // TODO: support other types
                }
            }
        }
    }
}
