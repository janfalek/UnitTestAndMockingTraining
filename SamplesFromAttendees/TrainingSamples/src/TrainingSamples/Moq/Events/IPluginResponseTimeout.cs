namespace TrainingSamples.Moq.Events
{
    public interface IPluginResponseTimeout
    {
        void SetResponseTimeout(int seconds);

        void CancelTimeout();
    }
}