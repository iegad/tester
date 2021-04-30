using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public class Hex
    {
        public static byte[] ToBytes(string str)
        {
            byte[] ret = new byte[str.Length / 2];

            for (int i = 0, n = ret.Length; i < n; i++)
                ret[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

            return ret;
        }

        public static string FromBytes(byte[] data, bool captital = false)
        {
            string fmt = "x2";
            if (captital)
                fmt = "X2";

            StringBuilder sb = new StringBuilder();
            if (data != null && data.Length > 0)
            {
                for (int i = 0, n = data.Length; i < n; i++)
                    sb.Append(data[i].ToString(fmt));
            }

            return sb.ToString();
        }
    }
}
