using System.ServiceModel;
using TrainingSamples.BackgroundWorker;

namespace TrainingSamples.Moq.Events
{
    [ServiceKnownType(typeof(InformationBoardMessage))]
    [ServiceKnownType(typeof(ErrorInformationBoardMessage))]
    [ServiceKnownType(typeof(WarningInformationBoardMessage))]
    [ServiceContract(CallbackContract = typeof(IPluginActionService))]
    public interface IHostNotificationService : IHostService
    {
        [OperationContract]
        void Connect();

        [OperationContract(IsOneWay = true)]
        void PluginContainerInitialized(PluginContainerInitialized pluginContainerInitializedMessage);

        [OperationContract(IsOneWay = true)]
        void PluginInitialized(PluginInitialized pluginInitializedMessage);

        [OperationContract(IsOneWay = true)]
        void PluginNavigatedToMenuView(PluginNavigatedToMenuView pluginNavigatedToMenuViewMessage);

        [OperationContract(IsOneWay = true)]
        void SetCurrentDesignContextId(DesignContextChanged designContexChangedMessage);

        [OperationContract]
        string GetCurrentDesignContextId();

        [OperationContract]
        void ReleaseCallbacks();

        [OperationContract(IsOneWay = true)]
        void PluginExited(PluginTermination message);

        [OperationContract(IsOneWay = true)]
        void SendPopupMessage(PopupMessage message);

        [OperationContract]
        void RequestHostClose();

        [OperationContract]
        void SetFocus();

        [OperationContract(IsOneWay = true)]
        void PluginButtonsShow(PluginButtonsShowMessage message);

        [OperationContract(IsOneWay = true)]
        void SendInformationBoardMessage(InformationBoardMessageBase informationBoardMessage);

        [OperationContract(IsOneWay = true)]
        void BackgroundWorkerProgress(ProgressMessage message);
    }
}