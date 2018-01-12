using System;

namespace TrainingSamples.Moq.Events
{
    public interface IWindowFocusSetter
    {
        void SetWindowFocus(IntPtr windowPtr);
    }
}