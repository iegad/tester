using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Assets.Scripts.Utils
{
    public class ProtocEx
    {
        private static JsonFormatter formatter_;
        public static string ToJson(IMessage message)
        {
            if (message == null)
                return string.Empty;

            if (formatter_ == null)
                formatter_ = new JsonFormatter(new JsonFormatter.Settings(false));

            return formatter_.Format(message);
        }
    }
}
