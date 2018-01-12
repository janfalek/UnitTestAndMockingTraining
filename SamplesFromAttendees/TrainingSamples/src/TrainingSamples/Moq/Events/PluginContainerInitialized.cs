using System;
using System.Runtime.Serialization;

namespace TrainingSamples.Moq.Events
{
    [DataContract]
    public class PluginContainerInitialized
    {
        [DataMember]
        public IntPtr PluginWindowPointer { get; set; }
    }
}