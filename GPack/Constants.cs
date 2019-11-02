using System;

namespace gpack
{
    public static class Constants
    {
        public static int FILE_READ_PAGE_SIZE = 16 * 1024;
        public static int CHAINE_SIZE = 8;
        public static int THREADS_COUNT = Environment.ProcessorCount > 2 ? Environment.ProcessorCount : 2;
    }
}
