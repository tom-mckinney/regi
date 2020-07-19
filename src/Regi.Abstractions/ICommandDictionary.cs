using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Abstractions
{
    public interface ICommandDictionary<TValue> : IDictionary<string, ICollection<TValue>>
    {
    }

    public interface ICommandDictionary : ICommandDictionary<string>
    {
    }
}
