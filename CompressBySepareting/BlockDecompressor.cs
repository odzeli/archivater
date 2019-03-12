using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressBySepareting
{
    class BlockDecompressor
    {

        private readonly int _part;
        private readonly object _locker;
        private static string _sourceCompressedFile;
        private readonly string _targetDecompressedFile;
        private readonly Dictionary<int, BlockDetails> _decompressedBufferHistory;

        public BlockDecompressor(string sourceCompressedFile, Dictionary<int, BlockDetails> decompressedBufferHistory, object locker, string targetDecompressedFile, int part)
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
                Console.Write(".");
                var compressedFileOffset = CountAllPreviouslyOffset(_decompressedBufferHistory, _part, true);
                var decompressedFileOffset = CountAllPreviouslyOffset(_decompressedBufferHistory, _part, false);

                using (FileStream fileToDecompress = new FileStream(_sourceCompressedFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (FileStream targetStream = new FileStream(_targetDecompressedFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        targetStream.Seek(decompressedFileOffset, SeekOrigin.Begin);
                        fileToDecompress.Seek(compressedFileOffset, SeekOrigin.Begin);
                        byte[] temporaryBufferWithOffset = new byte[_decompressedBufferHistory[_part].IndexOffsetForDecompressFileBlock];
                        using (GZipStream gzip = new GZipStream(fileToDecompress, CompressionMode.Decompress))
                        {
                            gzip.Read(temporaryBufferWithOffset, 0, temporaryBufferWithOffset.Length);
                            targetStream.Write(temporaryBufferWithOffset, 0, temporaryBufferWithOffset.Length);
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

        private static long CountAllPreviouslyOffset(Dictionary<int, BlockDetails> previousBuffer, int i, bool isCompressed)
        {
            long offset = 0;
            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    if (isCompressed)
                    {
                        offset = offset + previousBuffer[j].IndexOffsetOfCompressedFileBlock;
                    }
                    else
                    {
                        offset = offset + previousBuffer[j].IndexOffsetForDecompressFileBlock;
                    }
                }
                return offset;
            }
            return 0;
        }
    }
}
