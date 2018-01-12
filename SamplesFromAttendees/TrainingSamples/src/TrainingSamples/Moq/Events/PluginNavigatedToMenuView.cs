using System.Runtime.Serialization;

namespace TrainingSamples.Moq.Events
{
    [DataContract]
    public class PluginNavigatedToMenuView
    {
        [DataMember]
        public string MenuActionId { get; set; }
    }
}