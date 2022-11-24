using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrot_QA_test
{
    public static class CRC16
    {
        public static ushort ComputeChecksum(byte[] bytes, int size)
        {
            ushort crc = 0;
            for (int i = 0; i < size; ++i)
            {
                crc = checksum_crc16_block(crc, bytes[i]);
            }
            return crc;
        }

        private static ushort checksum_crc16_block(UInt16 crc, byte val)
        {
            int i;

            crc ^= (UInt16)(val << 8);

            for (i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (UInt16)((crc << 1) ^ 0x8005);
                }
                else
                {
                    crc <<= 1;
                }
            }

            return (crc);
        }

    }
}
