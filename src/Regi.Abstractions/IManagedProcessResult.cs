using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IManagedProcessResult
    {
        int ExitCode { get; }
        string StandardOutput { get; }
        string StandardError { get; }
    }
}
