using System.Runtime.Serialization;

namespace TrainingSamples.Moq.Events
{
    [DataContract]
    public class PluginInitialized
    {
        [DataMember]
        public string PluginName { get; set; }
    }
}