using System.Collections.Generic;

namespace Regi.Abstractions
{
    public interface ICommandDictionary<TValue> : IDictionary<string, ICollection<TValue>>
    {
    }

    public interface ICommandDictionary : ICommandDictionary<string>
    {
    }
}
