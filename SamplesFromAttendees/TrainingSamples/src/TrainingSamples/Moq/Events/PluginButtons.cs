using System.Collections.Generic;

namespace TrainingSamples.Moq.Events
{
    public class PluginButtons
    {
        public string PluginName { get; set; }

        public IEnumerable<string> SupportedButtonsIds { get; set; }
    }
}