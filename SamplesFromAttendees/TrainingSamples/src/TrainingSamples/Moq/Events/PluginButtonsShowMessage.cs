using System.Collections.Generic;

namespace TrainingSamples.Moq.Events
{
    public class PluginButtonsShowMessage
    {
        public List<PluginButtons> PluginButtons { get; set; } = new List<PluginButtons>();
    }
}