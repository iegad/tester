using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public class Endian
    {
        public static ushort ToBig(ref byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }

            return BitConverter.ToUInt16(data, 0);
        }

        public static ushort ToLocalUint16(ref byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }

            return BitConverter.ToUInt16(data, 0);
        }

        public static uint ToLocalUint32(ref byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }

            return BitConverter.ToUInt32(data, 0);
        }
    }
}
