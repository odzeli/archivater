using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CompressBySepareting
{
    public class Decompressor
    {
        private readonly int _part;
        private readonly object _locker;
        private static string _sourceCompressedFile;
        private readonly string _targetDecompressedFile;
        private readonly Dictionary<int, BlockDetails> _decompressedBufferHistory;
        public Decompressor(string sourceCompressedFile, Dictionary<int, BlockDetails> decompressedBufferHistory, object locker, string targetDecompressedFile, int part)
        {
            _part = part;
            _locker = locker;
            _sourceCompressedFile = sourceCompressedFile;
            _targetDecompressedFile = targetDecompressedFile;
            _decompressedBufferHistory = decompressedBufferHistory;
        }

        public void Decompressing()
        {
            try
            {
                lock (_locker)
                {
                    Console.Write(".");
                    var compressedFileOffset = CountAllPreviouslyOffset(_decompressedBufferHistory, _part);
                    var decompressedFileOffset = _part * _decompressedBufferHistory[_part].DecompressBufferBlock;

                    using (FileStream fileToDecompress = new FileStream(_sourceCompressedFile, FileMode.Open))
                    {
                        using (FileStream targetStream = new FileStream(_targetDecompressedFile, FileMode.OpenOrCreate))
                        {
                            targetStream.Seek(decompressedFileOffset, SeekOrigin.Begin);
                            fileToDecompress.Seek(compressedFileOffset, SeekOrigin.Begin);
                            byte[] temporaryBufferWithOffset = new byte[_decompressedBufferHistory[_part].DecompressBufferBlock];
                            using (GZipStream gzip = new GZipStream(fileToDecompress, CompressionMode.Decompress))
                            {
                                gzip.Read(temporaryBufferWithOffset, 0, temporaryBufferWithOffset.Length);
                                targetStream.Write(temporaryBufferWithOffset, 0, temporaryBufferWithOffset.Length);
                            }
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

        public static Dictionary<int, BlockDetails> CountCompressedBufferHistory(string sourceCompressedFile)
        {
            try
            {
                int count = 0;
                Dictionary<int, BlockDetails> compressedBufferHistory = new Dictionary<int, BlockDetails>();
                using (FileStream streamToDecompress = new FileStream(sourceCompressedFile, FileMode.Open))
                {
                    while (streamToDecompress.Position < streamToDecompress.Length)
                    {
                        var bufferForMetaData = new byte[8];
                        streamToDecompress.Read(bufferForMetaData, 0, bufferForMetaData.Length);
                        var bufferInfo = new BlockDetails { BufferBlock = BitConverter.ToInt32(bufferForMetaData, 4) };
                        var compressedBlock = new byte[bufferInfo.BufferBlock];
                        bufferForMetaData.CopyTo(compressedBlock, 0);
                        streamToDecompress.Read(compressedBlock, bufferForMetaData.Length, compressedBlock.Length - 8);
                        bufferInfo.DecompressBufferBlock = BitConverter.ToInt32(compressedBlock, bufferInfo.BufferBlock - 4);
                        compressedBufferHistory.Add(count, bufferInfo);
                        count++;
                    }
                }

                return compressedBufferHistory;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                throw;
            }
        }

        private static long CountAllPreviouslyOffset(Dictionary<int, BlockDetails> previousBuffer, int i)
        {
            long offset = 0;
            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    offset = offset + previousBuffer[j].BufferBlock;
                }
                return offset;
            }
            return 0;
        }
    }
}
