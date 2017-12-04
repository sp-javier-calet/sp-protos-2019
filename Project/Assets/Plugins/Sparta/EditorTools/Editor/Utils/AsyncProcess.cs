using System;
using System.Threading;

namespace SpartaTools.Editor.Utils
{
    public class ProgressHandler
    {
        public float Percent { get; private set; }

        public string Message{ get; private set; }

        public bool Finished{ get; private set; }

        public bool Cancelled { get; private set; }

        public void Update(string message, float increment)
        {
            Message = message;
            Percent += increment;
        }

        public void Update(float increment)
        {
            Percent += increment;
        }

        public void Finish()
        {
            Finished = true;
        }

        public void Cancel()
        {
            Cancelled = true;
            Message = "Cancelled by user..."; 
        }
    }

    public static class AsyncProcess
    {
        public static ProgressHandler Start(Action<ProgressHandler> action)
        {
            var handler = new ProgressHandler();
            var t = new Thread(() => action(handler));
            t.Start();

            return handler;
        }
    }
}