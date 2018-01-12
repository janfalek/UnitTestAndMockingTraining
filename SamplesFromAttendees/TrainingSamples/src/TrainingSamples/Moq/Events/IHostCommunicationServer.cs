using System;

namespace TrainingSamples.Moq.Events
{
    public interface IHostCommunicationServer : IDisposable
    {
        string Initialize();
    }
}