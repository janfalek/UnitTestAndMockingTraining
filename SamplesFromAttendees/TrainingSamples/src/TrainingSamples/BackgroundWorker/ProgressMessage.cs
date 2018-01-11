using System.Runtime.Serialization;

namespace TrainingSamples.BackgroundWorker
{
    [DataContract]
    public class ProgressMessage
    {
        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public BackgroundMessageType MessageType { get; set; }
    }
}