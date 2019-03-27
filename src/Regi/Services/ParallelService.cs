using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IParallelService
    {
        void Queue(Action action);

        void RunInParallel();

        void WaitOnPorts(IList<Project> projects);

        void WaitOnPorts(IDictionary<int, Project> projects);
    }

    public class ParallelService : IParallelService
    {
        private readonly IList<Action> actions = new List<Action>();
        private readonly IConsole _console;
        private readonly INetworkingService _networkingService;

        public ParallelService(IConsole console, INetworkingService networkingService)
        {
            _console = console;
            _networkingService = networkingService;
        }

        public void Queue(Action action)
        {
            actions.Add(action);
        }

        public void RunInParallel()
        {
            Parallel.Invoke(actions.ToArray());
        }

        public void WaitOnPorts(IList<Project> projects)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            WaitOnPorts(projectsWithPorts);
        }

        public void WaitOnPorts(IDictionary<int, Project> projects)
        {
            string projectPluralization = projects.Count > 1 ? "projects" : "project";
            _console.WriteEmphasizedLine($"Waiting for {projectPluralization} to start: {string.Join(", ", projects.Select(p => $"{p.Value.Name} ({p.Key})"))}");

            while (projects.Count > 0)
            {
                IPEndPoint[] listeningConnections = _networkingService.GetListeningPorts();

                foreach (var connection in listeningConnections)
                {
                    if (projects.TryGetValue(connection.Port, out Project p))
                    {
                        _console.WriteEmphasizedLine($"{p.Name} is now listening on port {connection.Port}");

                        projects.Remove(connection.Port);
                    }
                }
            }

            _console.WriteSuccessLine("All projects started", ConsoleLineStyle.LineBeforeAndAfter);
        }
    }
}
