using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public delegate int ConnectHandler(string localEndpoint, string remoteEndpoint);
    public enum Protocol
    {
        TCP = 1,
        KCP = 2,
        WS = 3
    }

    public interface INetClient
    {
        public void Start();
        public void Stop();
        public void Write(byte[] data);
    }

    public class NetClientFactory
    {
        public static INetClient New(Protocol protocol, string addr, int port, JobQue<Package> que)
        {
            switch (protocol)
            {
                case Protocol.TCP:
                    return new TcpClient(addr, port, que);

                case Protocol.KCP:
                    return new KcpClient(addr, port, que);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
