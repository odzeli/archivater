using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace CompressBySepareting
{
    class Archiver
    {
        private int _maxThreadPoolCount = 20;
        private static int _bufferSize = 1000000;
        private readonly object _locker = new object();

        public void StartCompress(string sourceFile, string targetCompressedFile)
        {
            try
            {
                int partsCount;
                var threadPool = new List<Thread>();

                using (FileStream fs = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    partsCount = (int)(fs.Length / _bufferSize);
                    partsCount = fs.Length % _bufferSize != 0 ? ++partsCount : partsCount;
                }
                Console.WriteLine("Compressing started...");
                Compressor compress = null;
                for (var i = 0; i < partsCount;)
                {
                    var count = threadPool.Count(t => t.IsAlive);
                    if (count <= _maxThreadPoolCount)
                    {
                        var offset = i * _bufferSize;
                        compress = new Compressor(sourceFile, _bufferSize, _locker, offset, targetCompressedFile);
                        Thread myThread = new Thread(compress.Compressing) {Name = i.ToString()};
                        threadPool.Add(myThread);
                        i++;
                        myThread.Start();
                    }
                }

                Wait(threadPool);
                compress?.PutAllCompressedFilesTogether(partsCount);
                Console.WriteLine($"\nParts count: {partsCount}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void StartDecompress(string sourceCompresedFile, string targetDecompressedFile)
        {
            try
            {

                var threadPool = new List<Thread>();
                var compressedBufferHistory = Decompress.Decompressor.CountCompressedBufferHistory(sourceCompresedFile);
                for (var i = 0; i < compressedBufferHistory.Count;)
                {
                    var count = threadPool.Count(t => t.IsAlive);
                    if (count <= _maxThreadPoolCount)
                    {
                        var decompress = new Decompress.Decompressor(sourceCompresedFile, compressedBufferHistory, _locker, targetDecompressedFile, i);
                        Thread myThread = new Thread(decompress.Decompressing) {Name = i.ToString()};
                        threadPool.Add(myThread);
                        i++;
                        myThread.Start();
                    }
                }
                Wait(threadPool);
                Console.WriteLine($"\nParts count: {compressedBufferHistory.Count}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private void Wait(List<Thread> treadsPool)
        {
            try
            {

                var active = true;
                while (active)
                {
                    foreach (var thread in treadsPool)
                    {
                        active = thread.IsAlive;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        public static string FormNewFileName(string fullPath, long offset)
        {
            return $"{fullPath}-FROM-{offset}-TO-{offset + _bufferSize}.gz";
        }
    }
}
