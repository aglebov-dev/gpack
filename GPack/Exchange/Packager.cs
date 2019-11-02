using System;
using System.IO;
using System.IO.Compression;

namespace gpack.Exchange
{
    internal class Packager
    {
        public byte[] Pack(byte[] data)
        {
            using (var stream = new MemoryStream(data.Length))
            using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
            {
                compressionStream.Write(data, 0, data.Length);
                compressionStream.Dispose();

                return stream.ToArray();
            }
        }

        public byte[] UnPack(byte[] data)
        {
            var buffer = new byte[Constants.FILE_READ_PAGE_SIZE];
            using (var stream = new MemoryStream(data))
            using (var compressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                var length = compressionStream.Read(buffer, 0, buffer.Length);
                var result = new byte[length];
                Array.Copy(buffer, 0, result, 0, length);
              
                return result;
            }
        }
    }
}
