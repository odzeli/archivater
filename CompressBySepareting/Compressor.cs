using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace CompressBySepareting
{
    public class Compressor
    {
        private readonly long _offset;
        private readonly object _locker;
        private readonly string _sourceFile;
        private readonly int _compressBufferSize;
        private readonly string _targetCompressedFile;

        public Compressor(string sourceFile, int compressBufferSize, object locker, long offset, string targetCompressedFile)
        {
            _locker = locker;
            _offset = offset;
            _sourceFile = sourceFile;
            _compressBufferSize = compressBufferSize;
            _targetCompressedFile = targetCompressedFile;
        }

        public void Compressing()
        {
            Console.Write(".");
            try
            {
                lock (_locker)
                    using (var sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
                    {
                        var bufferMessage = new byte[_compressBufferSize];
                        var newFile = Archiver.FormNewFileName(_targetCompressedFile, _offset);
                        using (var targetStream = new FileStream(newFile, FileMode.OpenOrCreate))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
                                {
                                    sourceStream.Seek(_offset, SeekOrigin.Begin);
                                    sourceStream.Read(bufferMessage, 0, _compressBufferSize);
                                    compressionStream.Write(bufferMessage, 0, _compressBufferSize);
                                }

                                var compressedBlock = memoryStream.ToArray();
                                var blocksLong = BitConverter.GetBytes(compressedBlock.Length);
                                Array.Resize(ref blocksLong, 4);
                                compressedBlock = AddMetaDataToBlock(compressedBlock, blocksLong);
                                targetStream.Write(compressedBlock, 0, compressedBlock.Length);
                            }
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        public void PutAllCompressedFilesTogether(int partsCount)
        {
            try
            {
                var previousBuffer = new Dictionary<int, int>();
                for (int i = 0; i < partsCount; i++)
                {
                    Console.Write(".");
                    long offset = i * _compressBufferSize;
                    var file = Archiver.FormNewFileName(_targetCompressedFile, offset);
                    using (FileStream nextFile = new FileStream(file, FileMode.Open))
                    {
                        var bufferMessage = new byte[nextFile.Length];
                        nextFile.Read(bufferMessage, 0, bufferMessage.Length);
                        previousBuffer.Add(i, bufferMessage.Length);
                        using (FileStream targetStream = new FileStream(_targetCompressedFile, FileMode.OpenOrCreate))
                        {
                            var localOffset = CountAllPreviouslyOffset(previousBuffer, i);
                            targetStream.Seek(localOffset, SeekOrigin.Begin);
                            targetStream.Write(bufferMessage, 0, bufferMessage.Length);
                        }
                    }
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static long CountAllPreviouslyOffset(Dictionary<int, int> previousBuffer, int i)
        {
            try
            {

                long offset = 0;
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        offset = offset + previousBuffer[j];
                    }
                    return offset;
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                throw;
            }
        }

        private static byte[] AddMetaDataToBlock(byte[] compressedBlock, byte[] blocksLong)
        {
            var i = 4;
            foreach (var item in blocksLong)
            {
                compressedBlock[i] = item;
                i++;
            }

            return compressedBlock;
        }
    }
}
