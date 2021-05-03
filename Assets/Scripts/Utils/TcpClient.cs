//#define __HEAD_SIZE_4 // 4 字节消息头
#define __HEAD_SIZE_2 // 2 字节消息头

using System;
using System.Net.Sockets;
using System.Threading;
using Base;

namespace Assets.Scripts.Utils
{
    public class TcpClient : INetClient
    {
#if   __HEAD_SIZE_4
        const int HEAD_SIZE = 4;
#elif __HEAD_SIZE_2
        const int HEAD_SIZE = 2;
#else
#error "HEAD_SIZE IS INVALID"
#endif
        const int RECV_BUFF = 1024 * 1024;
        const int SEND_BUFF = 1024 * 512;

        private string addr_;
        private int port_;
        private JobQue<Package> que_;
        private Socket sock_;
        private Thread thread_;

        public delegate int ConnectHandler(string localEndpoint, string remoteEndpoint);
        public event ConnectHandler ConnectedEvent;


        public TcpClient(string addr, int port, JobQue<Package> que)
        {
            addr_ = addr;
            port_ = port;
            que_ = que;

        }

        public void Start()
        {
            if (sock_ != null)
            {
                if (sock_.Connected)
                    sock_.Close();
                sock_.Dispose();
            }

            try
            {
                ThreadStart targ = new ThreadStart(_start);
                thread_ = new Thread(targ);
                thread_.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public void Stop()
        {
            if (sock_ != null)
            {
                if (sock_.Connected)
                    sock_.Close();
                sock_.Dispose();
            }
        }

        private void _start()
        {
            if (sock_ == null)
            {
                sock_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, RECV_BUFF);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, SEND_BUFF);
            }

            sock_.Connect(addr_, port_);
            if (ConnectedEvent != null && ConnectedEvent(sock_.LocalEndPoint.ToString(), sock_.RemoteEndPoint.ToString()) != 0)
            {
                Stop();
                return;
            }


            while (sock_.Connected)
            {
                var pack = _readPackage();
                if (pack != null)
                    que_.Push(pack);
            }
        }

        private Package _readPackage()
        {
            byte[] data = _readn();
            Log.Debug(Hex.ToString(data));
            if (data != null)
                return Package.Parser.ParseFrom(data);
            return null;
        }


        public void Write(byte[] data)
        {
            _writen(data);
        }

        private byte[] _readn()
        {
            int n, nleft = HEAD_SIZE, pos = 0;
            byte[] hbuf = new byte[HEAD_SIZE];
            while (nleft > 0)
            {
                n = sock_.Receive(hbuf, pos, nleft, SocketFlags.None);

                nleft -= n;
                pos += n;
            }

            if (nleft != 0)
                return null;

#if   __HEAD_SIZE_4
            uint ui = Endian.ToLocalUint32(ref hbuf);
            ui = (uint)~ui;
            nleft = (int)ui + 1;
#elif __HEAD_SIZE_2
            ushort us = Endian.ToLocalUint16(ref hbuf);
            us = (ushort)~us;
            nleft = us + 1;
#else
#error "HEAD_SIZE IS INVALID"
#endif
            if (nleft == 0)
                return null;

            byte[] data = new byte[nleft];
            pos = 0;
            while (nleft > 0)
            {
                n = sock_.Receive(data, pos, nleft, SocketFlags.None);

                nleft -= n;
                pos += n;
            }

            if (nleft != 0)
                return null;

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)~data[i];

            return data;
        }

        private void _writen(byte[] data)
        {
            int dataLen = data.Length;
            if (dataLen == 0)
                return;

            byte[] hbuf;

#if   __HEAD_SIZE_4
            if (dataLen > Int32.MaxValue)
                throw new Exception("data is too long");

            uint ui = (uint)(dataLen - 1);
            ui = (uint)~ui;
            hbuf = BitConverter.GetBytes(ui);
#elif __HEAD_SIZE_2
            if (dataLen > UInt16.MaxValue)
                throw new Exception("data is too long");

            ushort us = (ushort)(dataLen - 1);
            us = (ushort)~us;
            hbuf = BitConverter.GetBytes(us);
#else
#error "HEAD_SIZE IS INVALID"
#endif

            int n = HEAD_SIZE + dataLen;
            Endian.ToBig(ref hbuf);

            var buf = new byte[n];
            hbuf.CopyTo(buf, 0);

            for (int i = 0; i < dataLen; i++)
                data[i] = (byte)~data[i];

            data.CopyTo(buf, HEAD_SIZE);

            int nleft = n, pos = 0;

            while (nleft > 0)
            {
                n = sock_.Send(buf, pos, nleft, SocketFlags.None);

                pos += n;
                nleft -= n;
            }
        }
    }
}

