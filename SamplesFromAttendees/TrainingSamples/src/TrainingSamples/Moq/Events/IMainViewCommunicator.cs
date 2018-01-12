using System;

namespace TrainingSamples.Moq.Events
{
    public interface IMainViewCommunicator
    {
        event Action<IntPtr?> WindowGrab;
        event Action WindowClose;
        event Action WindowFocusSet;

        IChildWindowManager ChildWindowManager { get; }

        IWindowFocusSetter WindowFocusSetter { get; }
    }
}