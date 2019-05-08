using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IParallelService
    {
        void Queue(bool isSerial, Action action);
        void QueueParallel(Action action);
        void QueueSerial(Action action);

        void RunAll();
        void RunParallel();
        void RunSerial();

        void WaitOnPorts(IList<Project> projects);

        void WaitOnPorts(IDictionary<int, Project> projects);
    }

    public class ParallelService : IParallelService
    {
        private readonly IConsole _console;
        private readonly INetworkingService _networkingService;

        public ParallelService(IConsole console, INetworkingService networkingService)
        {
            _console = console;
            _networkingService = networkingService;
        }

        public IList<Action> ParallelActions { get; } = new List<Action>();
        public IList<Action> SerialActions { get; } = new List<Action>();

        public void Queue(bool isSerial, Action action)
        {
            if (isSerial)
                QueueSerial(action);
            else
                QueueParallel(action);
        }

        public void QueueParallel(Action action)
        {
            ParallelActions.Add(action);
        }

        public void QueueSerial(Action action)
        {
            SerialActions.Add(action);
        }

        public void RunAll()
        {
            RunParallel();

            RunSerial();
        }

        public void RunParallel()
        {
            Parallel.Invoke(ParallelActions.ToArray());
        }

        public void RunSerial()
        {
            foreach (var action in SerialActions)
            {
                action();
            }
        }

        public void WaitOnPorts(IList<Project> projects)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            WaitOnPorts(projectsWithPorts);
        }

        public virtual void WaitOnPorts(IDictionary<int, Project> projects)
        {
            string projectPluralization = projects.Count > 1 ? "projects" : "project";
            _console.WriteEmphasizedLine($"Waiting for {projectPluralization} to start: {string.Join(", ", projects.Select(p => $"{p.Value.Name} ({p.Key})"))}");

            IList<int> activePorts = new List<int>();

            while (projects.Count > activePorts.Count)
            {
                foreach (var port in projects.Keys.Where(k => !activePorts.Contains(k)))
                {
                    if (_networkingService.IsPortListening(port))
                    {
                        activePorts.Add(port);

                        _console.WriteEmphasizedLine($"{projects[port].Name} is now listening on port {port}");
                    }
                }

                Thread.Sleep(100);
            }

            _console.WriteSuccessLine("All projects started", ConsoleLineStyle.LineBeforeAndAfter);
        }
    }
}
