using System;

namespace GPack
{
    public class ByteBlock
    {
        public byte[] Bytes { get; }
        public int BlockLength => Bytes.Length;
        public int Index => BitConverter.ToInt32(Bytes, 4);
        public int DataLength => Bytes.Length - 8;

        public ByteBlock(byte[] bytes)
        {
            Bytes = bytes; 
        }

        public static ByteBlock Create(int index, byte[] data, int dataLength)
        {
            var bytes = new byte[8 + dataLength];
            Array.Copy(BitConverter.GetBytes(8 + dataLength), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(index), 0, bytes, 4, 4);
            Array.Copy(data, 0, bytes, 8, dataLength);

            return new ByteBlock(bytes);
        }

        public static byte[] Extractdata(ByteBlock block)
        {
            var result = new byte[block.DataLength];
            Array.Copy(block.Bytes, 8, result, 0, result.Length);

            return result;
        }
    }
}
