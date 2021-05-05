using System;
using System.Net.Sockets;
using System.Threading;
using Base;

namespace Assets.Scripts.Utils
{
    public class TcpClient : INetClient
    {// TCP 客户端
        public int HEAD_SIZE { get; set; } // 消息头条度

        // 读缓冲区大小(1M)
        const int MAX_SIZE_RBUF = 1024 * 1024;
        // 写缓冲区大小(512K)
        const int MAX_SIZE_WBUF = 1024 * 512;

        private readonly string addr_;
        private readonly int port_;
        // 任务队列
        private readonly JobQue<Package> que_; 

        private Socket sock_;
        private Thread thread_;

        // 连接成功事件
        public event ConnectHandler ConnectedEvent;

        public TcpClient(string addr, int port, JobQue<Package> que, int headSize = 2)
        {
            if (headSize != 2 && headSize != 4)
                throw new Exception("HeadSize is invalid");

            HEAD_SIZE = headSize;
            addr_ = addr;
            port_ = port;
            que_ = que;
        }

        public void Start()
        {// 启动后台读工作线程
            if (thread_ != null)
                throw new Exception("Networking is already running");

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

        public void Write(byte[] data)
        {
            _writen(data);
        }

        private void _start()
        {
            if (sock_ == null)
            {
                sock_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MAX_SIZE_RBUF);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, MAX_SIZE_WBUF);
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

        private byte[] _readn()
        {// 读消息, 仅限框架协议使用

            // Step 1: 读消息头
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

            switch (HEAD_SIZE)
            {
                case 2:
                    ushort us = Binary.BigEndian.Uint16(hbuf);
                    us = (ushort)~us;
                    nleft = us + 1;
                    break;

                case 4:
                    uint ui = Binary.BigEndian.Uint32(hbuf);
                    ui = (uint)~ui;
                    nleft = (int)ui + 1;
                    break;

                default:
                    throw new Exception("HeadSize is invalid");
            }

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

            byte[] hbuf = new byte[HEAD_SIZE];

            switch (HEAD_SIZE)
            {
                case 2:
                    if (dataLen > UInt16.MaxValue)
                        throw new Exception("data is too long");

                    ushort us = (ushort)(dataLen - 1);
                    us = (ushort)~us;
                    Binary.BigEndian.PutUint16(ref hbuf, us);
                    break;

                case 4:
                    if (dataLen > Int32.MaxValue)
                        throw new Exception("data is too long");

                    uint ui = (uint)(dataLen - 1);
                    ui = (uint)~ui;
                    Binary.BigEndian.PutUint32(ref hbuf, ui);
                    break;

                default:
                    throw new Exception("HeadSize is invalid");
            }

            int n = HEAD_SIZE + dataLen;
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

