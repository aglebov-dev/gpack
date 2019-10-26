using System;

namespace GPack
{
    public class Progress
    {
        private long TotalRead;
        private long TotalWrite;
        public Exception Exception { get; private set; }

        public void AppendRead(long value)
        {
        }

        public void AppendWrite(long value)
        {
        }

        public void Error(Exception exception)
        {
            Exception = exception;
        }
    }
}
