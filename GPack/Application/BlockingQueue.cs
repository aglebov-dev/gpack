using System.Threading;

namespace GPack
{
    public class BlockingQueue
    {
        private readonly int _queueSize;
        private readonly int _mask;
        private readonly ByteBlock[] _buffer;

        private long _queueHead;
        private long _queueTail;

        public bool IsDataReceptionCompleted { get; private set; }
        public long Count => _queueTail - _queueHead;
        public bool IsEmpty => IsDataReceptionCompleted && Count == 0;

        public BlockingQueue()
        {
            _queueSize = GetPowerOfTwo(Constants.DEFAULT_BUFFER_SIZE);
            _mask = _queueSize - 1;
            _buffer = new ByteBlock[_queueSize];
        }

        public bool TryEnqueue(ByteBlock block)
        {
            while (true)
            {
                var head = Interlocked.Read(ref _queueHead);
                var tail = Interlocked.Read(ref _queueTail);

                if (IsDataReceptionCompleted || tail - head >= _queueSize)
                {
                    return false;
                }
                else if (tail == Interlocked.CompareExchange(ref _queueTail, tail + 1, tail))
                {
                    var index = tail & _mask;
                    _buffer[index] = block;
                   
                    return true;
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        public bool TryDequeue(out ByteBlock block)
        {
            while (true)
            {
                var head = Interlocked.Read(ref _queueHead);
                var tail = Interlocked.Read(ref _queueTail);
                if (head >= tail)
                {
                    block = null;
                    return false;
                }

                var buffer = _buffer;
                var index = head & _mask;
                block = buffer[index];
                if (block != null && head == Interlocked.CompareExchange(ref _queueHead, head + 1, head))
                {
                    Interlocked.CompareExchange(ref _buffer[index], null, block);
                    return true;
                }

                Thread.Yield();
            }
        }

        public void Complete()
        {
            IsDataReceptionCompleted = true;
        }

        private int GetPowerOfTwo(int x)
        {
            x = x - 1;
            x = x | (x >> 1);
            x = x | (x >> 2);
            x = x | (x >> 4);
            x = x | (x >> 8);
            x = x | (x >> 16);
            return x + 1;
        }
    }
}
