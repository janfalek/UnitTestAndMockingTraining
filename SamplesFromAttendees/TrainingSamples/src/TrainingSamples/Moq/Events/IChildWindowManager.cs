using System;
using System.Windows.Forms;

namespace TrainingSamples.Moq.Events
{
    public interface IChildWindowManager
    {
        void SetWindowParent(IntPtr windowPtr, Control parentPanel);
        void ReleaseWindow();
    }
}