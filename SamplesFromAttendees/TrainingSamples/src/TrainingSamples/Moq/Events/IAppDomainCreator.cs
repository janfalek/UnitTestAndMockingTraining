using TrainingSamples.Moq.Setup;

namespace TrainingSamples.Moq.Events
{
    public interface IAppDomainCreator
    {
        void UnloadPlugin();

        void CreateSubDomain(string endpoint, HostConfiguration hostConfiguration);
    }
}