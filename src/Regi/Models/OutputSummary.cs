using System.Collections.Generic;

namespace Regi.Models
{
    public class OutputSummary
    {
        public IList<Project> Apps { get; set; } = new List<Project>();

        public IList<Project> Tests { get; set; } = new List<Project>();

        public int SuccessCount { get; set; }

        public int FailCount { get; set; }

        public int UnknownCount { get; set; }
    }
}
