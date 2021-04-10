using Regi.Abstractions;
using System.Collections.Generic;

namespace Regi.Models
{
    public class OutputSummary
    {
        public IList<IProject> Projects { get; set; } = new List<IProject>();

        public int SuccessCount { get; set; }

        public int FailCount { get; set; }

        public int UnknownCount { get; set; }
    }
}
