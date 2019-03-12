using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CompressBySepareting
{
    class Archiver
    {
        private static int _bufferSize = 1000000;
        private readonly object _locker = new object();

        public void StartCompress(string sourceFile, string targetCompressedFile)
        {
            try
            {
                var threadPool = new PoolOfThread();
                var fs = new FileInfo(sourceFile);
                var partsCount = (int)(fs.Length / _bufferSize);
                partsCount = fs.Length % _bufferSize != 0 ? ++partsCount : partsCount;

                Console.WriteLine("Compressing started...");

                for (var i = 0; i < partsCount;)
                {
                    var offset = i * _bufferSize;
                    var blockCompress = new BlockCompressor(sourceFile, _bufferSize, _locker, offset, targetCompressedFile, i);
                    Thread myThread = new Thread(blockCompress.Compressing) { Name = i.ToString() };
                    threadPool.Add(myThread);
                    i++;
                }
                threadPool.Wait();
                var compressor = new Compressor(_bufferSize, targetCompressedFile);
                compressor.PutAllCompressedFilesTogether(partsCount);
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
                var threadPool = new PoolOfThread();
                var compressedBufferHistory = Decompressor.CountCompressedBufferHistory(sourceCompresedFile);
                Console.WriteLine("Decompressing started...");
                for (var i = 0; i < compressedBufferHistory.Count;)
                {
                    var blockDecompress = new BlockDecompressor(sourceCompresedFile, compressedBufferHistory, _locker, targetDecompressedFile, i);
                    Thread myThread = new Thread(blockDecompress.Decompressing) { Name = i.ToString() };
                    threadPool.Add(myThread);
                    i++;
                }
                threadPool.Wait();
                Console.WriteLine($"\nParts count: {compressedBufferHistory.Count}");
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
