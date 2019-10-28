using System;

namespace GPack
{
    public class Progress
    {
        public Exception Exception { get; private set; }

        public void Error(Exception exception)
        {
            Exception = exception;
        }
    }
}
