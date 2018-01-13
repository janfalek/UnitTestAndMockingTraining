using System;
using System.Threading;

namespace TrainingSamples.BackgroundWorker
{
    public interface IBackgroundWorkerAbort
    {
        event Action ThreadAborted;

        void CancelAbortion();
        void StartCountingForAbortion(Thread thread);
    }
}