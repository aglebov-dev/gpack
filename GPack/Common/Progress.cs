using System;

namespace gpack.Common
{
    internal class Progress
    {
        public Exception Exception { get; private set; }

        public void Error(Exception exception)
        {
            Exception = exception;
        }
    }
}
