using GPack.Common;
using GPack.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace GPack.Tests
{
    [TestClass]
    public class MyTestClass
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var reader = new Reader(CancellationToken.None);

            var array = new byte[Constants.FILE_READ_BUFFER_SIZE];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 9;
            }

            var queue = reader.BeginReadAsync(new MemoryStream(array));

            queue.TryDequeue(out var value);

            //Assert.AreEqual(Constants.FILE_READ_BUFFER_SIZE + Constants.INDEX_SIZE, value.Length);
        }
    }
}
