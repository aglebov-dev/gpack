using System;
using System.IO;
using System.IO.Compression;

namespace GPack
{
    public class Packager
    {
        public ByteBlock Pack(ByteBlock block)
        {
            var data = ByteBlock.Extractdata(block);

            using (var stream = new MemoryStream(block.DataLength))
            using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
            {
                compressionStream.Write(data, 0, data.Length);
                compressionStream.Dispose();
                var compressedBytes = stream.ToArray();

                return ByteBlock.Create(block.Index, compressedBytes, compressedBytes.Length);
            }
        }

        public ByteBlock UnPack(ByteBlock block)
        {
            var data = ByteBlock.Extractdata(block);
            
            using (var stream = new MemoryStream(data))
            using (var compressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                var bytes = new byte[Constants.FILE_READ_BUFFER_SIZE];
                var length = compressionStream.Read(bytes, 0, bytes.Length);

                return ByteBlock.Create(block.Index, bytes, length);
            }
        }
    }
}
