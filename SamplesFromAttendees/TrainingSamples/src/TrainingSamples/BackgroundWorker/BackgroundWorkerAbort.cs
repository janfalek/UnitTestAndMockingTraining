using System;
using System.Threading;

namespace TrainingSamples.BackgroundWorker
{
    public class BackgroundWorkerAbort : IBackgroundWorkerAbort
    {
        private readonly ManualResetEvent waitHandle;
        private int seconds = 5;

        public BackgroundWorkerAbort()
        {
            waitHandle = new ManualResetEvent(false);
        }

        public event Action ThreadAborted;

        public void StartCountingForAbortion(Thread thread)
        {
            var timeoutThread = new Thread(() =>
            {
                waitHandle.Reset();
                int timeout = seconds * 1000;

                if (!waitHandle.WaitOne(timeout))
                {
                    thread.Abort();
                    thread.Join();
                    ThreadAborted?.Invoke();
                }
            })
            {
                IsBackground = true
            };
            timeoutThread.Start();
        }

        public void CancelAbortion()
        {
            waitHandle.Set();
        }
    }
}