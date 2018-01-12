using System.Runtime.Serialization;

namespace TrainingSamples.Moq.Events
{
    [DataContract]
    public class DesignContextChanged
    {
        [DataMember]
        public string DesignId { get; set; }
    }
}