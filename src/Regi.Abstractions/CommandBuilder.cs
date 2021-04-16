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
            if (string.IsNullOrEmpty(directive))
            {
                throw new InvalidOperationException("Received null value for directive");
            }

            _directives.Add(directive);
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException("Received null value for directive key or value");
            }

            _directives.Add($"{key} {value}");
        }

        public void Add(string key, ICollection<string> values, DirectiveListType listType = DirectiveListType.Multiple)
        {
            if (string.IsNullOrEmpty(key) || !values.Any())
            {
                throw new InvalidOperationException("Received null value for directive key or value");
            }

            if (listType == DirectiveListType.Multiple)
            {
                foreach (var v in values)
                {
                    _directives.Add($"{key} {v}");
                }
            }
            else
            {
                throw new NotImplementedException(); // TODO: support other types
            }
        }
    }
}
