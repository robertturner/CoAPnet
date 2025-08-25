using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace CoAPnet.Protocol.Options
{
    public sealed class CoapOptionBlocks
    {
        public bool More { get; set; }
        public uint SequenceNum { get; set; }
        public BlockSizeTypes BlockSize { get; set; } = BlockSizeTypes.BLOCK_SIZE_16;

        public enum BlockSizeTypes : byte
        {
            BLOCK_SIZE_16 = 0,
            BLOCK_SIZE_32 = 1,
            BLOCK_SIZE_64 = 2,
            BLOCK_SIZE_128 = 3,
            BLOCK_SIZE_256 = 4,
            BLOCK_SIZE_512 = 5,
            BLOCK_SIZE_1024 = 6,
        }

        public CoapOptionBlocks(bool more, uint sequenceNum, BlockSizeTypes blockSize)
        {
            More = more;
            SequenceNum = sequenceNum;
            BlockSize = blockSize;
        }
        public CoapOptionBlocks(bool more, uint sequenceNum, uint dataSize)
        {
            More = more;
            SequenceNum = sequenceNum;
            var blockSize = BlockSizeTypes.BLOCK_SIZE_16;
            do
            {
                if (dataSize <= (1u << ((int)blockSize + 4)))
                    break;
                blockSize = (BlockSizeTypes)(blockSize + 1);
            } while (blockSize < BlockSizeTypes.BLOCK_SIZE_1024);
            BlockSize = blockSize;
        }

        public uint AsInt() => (((uint)BlockSize) & 7U) | (More ? 8U : 0U) | (SequenceNum << 4);
    }
}
