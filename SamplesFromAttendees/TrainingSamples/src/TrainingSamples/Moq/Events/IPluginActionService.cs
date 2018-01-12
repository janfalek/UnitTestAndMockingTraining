using System.ServiceModel;

namespace TrainingSamples.Moq.Events
{
    [ServiceContract]
    public interface IPluginActionService
    {
        [OperationContract(IsOneWay = true)]
        void SendAction(PluginActionMessage message);

        [OperationContract(IsOneWay = true)]
        void SendPopupResult(DialogResult dialogResult);

        [OperationContract]
        void TerminatePlugins();

        [OperationContract(IsOneWay = true)]
        void CancelBackgroundWorker();
    }
}