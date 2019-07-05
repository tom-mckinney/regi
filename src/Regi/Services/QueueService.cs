using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IQueueService
    {
        ParallelOptions ParallelOptions { get; }

        void Queue(bool isSerial, Action action);
        void QueueParallel(Action action);
        void QueueSerial(Action action);

        void RunAll();
        void RunParallel();
        void RunSerial();

        void ConfirmProjectsStarted(IList<Project> projects);
        void ConfirmProjectsStarted(IDictionary<int, Project> projects);

        void WaitOnPort(Project project);
    }

    public class QueueService : IQueueService
    {
        private readonly IConsole _console;
        private readonly INetworkingService _networkingService;

        public QueueService(IConsole console, INetworkingService networkingService)
        {
            _console = console;
            _networkingService = networkingService;
        }

        public ConcurrentQueue<Action> ParallelActions { get; } = new ConcurrentQueue<Action>();
        public ConcurrentQueue<Action> SerialActions { get; } = new ConcurrentQueue<Action>();

        public ParallelOptions ParallelOptions => new ParallelOptions
        {
            MaxDegreeOfParallelism = 3
        };

        public void Queue(bool isSerial, Action action)
        {
            if (isSerial)
                QueueSerial(action);
            else
                QueueParallel(action);
        }

        public void QueueParallel(Action action)
        {
            ParallelActions.Enqueue(action);
        }

        public void QueueSerial(Action action)
        {
            SerialActions.Enqueue(action);
        }

        public void RunAll()
        {
            RunParallel();

            RunSerial();
        }

        public void RunParallel()
        {
            Parallel.Invoke(ParallelOptions, ParallelActions.ToArray());
        }

        public void RunSerial()
        {
            foreach (var action in SerialActions)
            {
                action();
            }
        }

        public virtual void WaitOnPort(Project project)
        {
            if (project.Port.HasValue)
            {
                bool isListening = false;

                while (!isListening)
                {
                    Thread.Sleep(200);

                    isListening = _networkingService.IsPortListening(project.Port.Value);
                }

                _console.WriteSuccessLine($"{project.Name} is now listening on port {project.Port}");
            }
        }

        public void ConfirmProjectsStarted(IList<Project> projects)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            ConfirmProjectsStarted(projectsWithPorts);
        }

        public virtual void ConfirmProjectsStarted(IDictionary<int, Project> projects)
        {
            string projectPluralization = projects.Count == 1 ? "project" : "projects";
            string hasPluralization = projects.Count == 1 ? "has" : "have";
            _console.WriteDefaultLine($"Confirming {projectPluralization} {hasPluralization} started: {string.Join(", ", projects.Select(p => $"{p.Value.Name} ({p.Key})"))}");

            IList<int> activePorts = new List<int>();

            while (projects.Count > activePorts.Count)
            {
                Thread.Sleep(200);

                foreach (var port in projects.Keys.Where(k => !activePorts.Contains(k)))
                {
                    if (_networkingService.IsPortListening(port))
                    {
                        activePorts.Add(port);
                    }
                }
            }

            _console.WriteSuccessLine("All projects started", ConsoleLineStyle.LineBeforeAndAfter);
        }
    }
}
