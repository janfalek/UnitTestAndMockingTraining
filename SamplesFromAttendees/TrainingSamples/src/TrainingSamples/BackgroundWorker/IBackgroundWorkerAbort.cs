using System.Threading;

namespace TrainingSamples.BackgroundWorker
{
    public interface IBackgroundWorkerAbort
    {
        void StartCountingForAbortion(Thread thread);
        void CancelAbortion();
    }
}