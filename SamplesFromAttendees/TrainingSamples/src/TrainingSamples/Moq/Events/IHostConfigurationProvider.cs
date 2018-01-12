using TrainingSamples.Moq.Setup;

namespace TrainingSamples.Moq.Events
{
    public interface IHostConfigurationProvider
    {
        HostConfiguration GetConfiguration();
    }
}