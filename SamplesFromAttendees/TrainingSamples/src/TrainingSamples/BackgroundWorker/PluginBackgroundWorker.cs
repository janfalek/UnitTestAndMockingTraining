using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TrainingSamples.BackgroundWorker
{
    public class PluginBackgroundWorker
    {
        private readonly IProgress<ProgressMessage> _progress;
        private readonly BackgroundWorkerAbort _backgroundWorkerAbort;
        private Thread _backgroundThread;
        private CancellationTokenSource _cancellationTokenSource;
        private object _locker = new object();

        public PluginBackgroundWorker(IProgress<ProgressMessage> progress, BackgroundWorkerAbort backgroundWorkerAbort, IEventAggregator eventAggregator)
        {
            _progress = progress;
            _backgroundWorkerAbort = backgroundWorkerAbort;
            eventAggregator.Subscribe(this);
        }

        public Task Start(
            Action<CancellationToken, IProgress<ProgressMessage>> workAction,
            Action<Task, CancellationToken, IProgress<ProgressMessage>> continueWith = null)
        {
            lock (_locker)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            var token = _cancellationTokenSource.Token;

            var task = Task.Factory.StartNew(() =>
            {
                lock (_locker)
                {
                    _backgroundThread = Thread.CurrentThread;
                }

                token.ThrowIfCancellationRequested();
                workAction(token, _progress);
            }).ContinueWith(t =>
            {
                if (!token.IsCancellationRequested)
                {
                    continueWith?.Invoke(t, token, _progress);
                }
                _progress.Report(new ProgressMessage { MessageType = BackgroundMessageType.Finished });
                _backgroundWorkerAbort.CancelAbortion();
                ReleseResources();
            });
            return task;
        }

        public void Handle(BackgroundWorkerCancellationMessage message)
        {
            lock (_locker)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }

            Thread threadForAbortion;
            lock (_locker)
            {
                threadForAbortion = _backgroundThread;
            }

            if (threadForAbortion != null)
            {
                _backgroundWorkerAbort.StartCountingForAbortion(threadForAbortion);
            }
        }

        private void ReleseResources()
        {
            lock (_locker)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _backgroundThread = null;
            }
        }
    }
}