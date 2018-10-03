using System.Collections.Generic;

namespace Regi.Models
{
    public class OutputSummary
    {
        public IList<Project> Apps { get; set; } = new List<Project>();

        public IList<Project> Tests { get; set; } = new List<Project>();
    }
}
