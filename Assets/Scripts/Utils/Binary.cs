using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public class Binary 
    {
        private Binary() { }

        public class BigEndian
        {
            private BigEndian() { }

            public static UInt16 Uint16(byte[] data)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);

                return BitConverter.ToUInt16(data, 0);
            }

            public static UInt32 Uint32(byte[] data)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }

            public static UInt64 Uint64(byte[] data)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToUInt64(data, 0);
            }

            public static void PutUint16(ref byte[] data, UInt16 v)
            {
                data = BitConverter.GetBytes(v);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
            }

            public static void PutUint32(ref byte[] data, UInt32 v)
            {
                data = BitConverter.GetBytes(v);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);

            }

            public static void PutUint64(ref byte[] data, UInt64 v)
            {
                data = BitConverter.GetBytes(v);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
            }
        }
    }
}
