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
        void Queue(bool isSerial, Action action, CancellationToken cancellationToken);
        void Queue(bool isSerial, Func<Task> action, CancellationToken cancellationToken);

        void QueueAsync(Action action, CancellationToken cancellationToken);
        void QueueAsync(Func<Task> action, CancellationToken cancellationToken);
        
        void QueueSerial(Action action);
        void QueueSerial(Func<Task> action);

        Task RunAllAsync(CancellationToken cancellationToken);

        Task ConfirmProjectsStartedAsync(IList<Project> projects, CancellationToken cancellationToken);
        Task ConfirmProjectsStartedAsync(IDictionary<int, Project> projects, CancellationToken cancellationToken);

        Task WaitOnPortAsync(Project project, CancellationToken cancellationToken);
    }

    public class QueueService : IQueueService
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(3, 3);
        private readonly IConsole _console;
        private readonly INetworkingService _networkingService;

        public QueueService(IConsole console, INetworkingService networkingService)
        {
            _console = console;
            _networkingService = networkingService;
        }

        public ConcurrentQueue<Func<Task>> AsyncActions { get; } = new ConcurrentQueue<Func<Task>>();
        public ConcurrentQueue<Func<Task>> SerialActions { get; } = new ConcurrentQueue<Func<Task>>();


        public void Queue(bool isSerial, Action action, CancellationToken cancellationToken)
        {
            Queue(isSerial, () => Task.Run(action), cancellationToken);
        }

        public void Queue(bool isSerial, Func<Task> action, CancellationToken cancellationToken)
        {
            if (isSerial)
                QueueSerial(action);
            else
                QueueAsync(action, cancellationToken);
        }

        public void QueueAsync(Action action, CancellationToken cancellationToken)
        {
            QueueAsync(() => Task.Run(action), cancellationToken);
        }

        public void QueueAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            AsyncActions.Enqueue(action);
        }

        public void QueueSerial(Action action)
        {
            QueueSerial(() => Task.Run(action));
        }

        public void QueueSerial(Func<Task> action)
        {
            SerialActions.Enqueue(action);
        }

        public async Task RunAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await RunAsyncActions(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await RunSerialActions(cancellationToken);
        }

        public async Task RunAsyncActions(CancellationToken cancellationToken)
        {
            foreach (var action in AsyncActions)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                await action();

                semaphore.Release();
            }
        }

        public async Task RunSerialActions(CancellationToken cancellationToken)
        {
            foreach (var action in SerialActions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await action();
            }
        }

        public virtual async Task WaitOnPortAsync(Project project, CancellationToken cancellationToken)
        {
            if (project.Port.HasValue)
            {
                bool isListening = false;

                while (!isListening && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(200);

                    isListening = _networkingService.IsPortListening(project.Port.Value);
                }

                cancellationToken.ThrowIfCancellationRequested();

                _console.WriteSuccessLine($"{project.Name} is now listening on port {project.Port}");
            }
        }

        public Task ConfirmProjectsStartedAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            return ConfirmProjectsStartedAsync(projectsWithPorts, cancellationToken);
        }

        public virtual async Task ConfirmProjectsStartedAsync(IDictionary<int, Project> projects, CancellationToken cancellationToken)
        {
            string projectPluralization = projects.Count == 1 ? "project" : "projects";
            string hasPluralization = projects.Count == 1 ? "has" : "have";
            _console.WriteDefaultLine($"Confirming {projectPluralization} {hasPluralization} started: {string.Join(", ", projects.Select(p => $"{p.Value.Name} ({p.Key})"))}");

            IList<int> activePorts = new List<int>();

            while (projects.Count > activePorts.Count && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(200);

                foreach (var port in projects.Keys.Where(k => !activePorts.Contains(k)))
                {
                    if (_networkingService.IsPortListening(port))
                    {
                        activePorts.Add(port);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            _console.WriteSuccessLine("All projects started", ConsoleLineStyle.LineBeforeAndAfter);
        }
    }
}
