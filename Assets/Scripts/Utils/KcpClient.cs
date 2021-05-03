using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using KcpProject;
using System.Threading;

namespace Assets.Scripts.Utils
{

    public class KcpClient : INetClient
    {
        public int HEAD_SIZE { get; set; }

        private string addr_;
        private int port_;
        private JobQue<Package> que_;
        private UDPSession conn_;
        private Thread thread_;

        public KcpClient(string addr, int port, JobQue<Package> que, int headSize = 2)
        {
            addr_ = addr;
            port_ = port;
            que_ = que;
            HEAD_SIZE = headSize;
        }
        public void Start()
        {
            if (thread_ != null)
            {
                throw new Exception("Networking is already running");
            }

            if (conn_ != null)
            {
                if (conn_.IsConnected)
                    conn_.Close();
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
            if (conn_ != null)
                conn_.Close();
        }

        public void Write(byte[] data)
        {
            _writen(data);
        }

        private void _start()
        {
            if (conn_ == null)
                conn_ = new UDPSession();
            conn_.Connect(addr_, port_);

            if (conn_.IsConnected)
                Log.Debug("连接成功");

            while (conn_.IsConnected)
            {
                var pack = _readPackage();
                if (pack != null)
                    que_.Push(pack);
            }
        }

        private Package _readPackage()
        {
            byte[] data = _readn();

            if (data != null)
                return Package.Parser.ParseFrom(data);
            return null;
        }

        private byte[] _readn()
        {
            int n, nleft = HEAD_SIZE, pos = 0;
            byte[] hbuf = new byte[HEAD_SIZE];
            
            while(nleft > 0)
            {
                n = conn_.Recv(hbuf, pos, nleft);
                if (n < 0)
                    throw new Exception("接收错误");

                nleft -= n;
                pos += n;
            }

            if (nleft != 0)
                return null;

            switch (HEAD_SIZE)
            {
                case 2:
                    ushort us = Endian.ToLocalUint16(ref hbuf);
                    us = (ushort)~us;
                    nleft = us + 1;
                    break;

                case 4:
                    uint ui = Endian.ToLocalUint32(ref hbuf);
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
            while(nleft > 0)
            {
                n = conn_.Recv(data, pos, nleft);
                pos += n;
                nleft -= n;
            }

            if (nleft != 0)
                return null;

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)~data[i];

            return data;
        }

        private void _writen(byte[] data)
        {
            conn_.Update();
            if (conn_ == null || !conn_.IsConnected)
                throw new Exception("未建立连接");

            int dataLen = data.Length;
            if (dataLen == 0)
                return;

            byte[] hbuf;

            switch (HEAD_SIZE)
            {
                case 2:
                    if (dataLen > UInt16.MaxValue)
                        throw new Exception("data is too long");

                    ushort us = (ushort)(dataLen - 1);
                    us = (ushort)~us;
                    hbuf = BitConverter.GetBytes(us);
                    break;

                case 4:
                    if (dataLen > Int32.MaxValue)
                        throw new Exception("data is too long");

                    uint ui = (uint)(dataLen - 1);
                    ui = (uint)~ui;
                    hbuf = BitConverter.GetBytes(ui);
                    break;

                default:
                    throw new Exception("HeadSize is invalid");
            }

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
                n = conn_.Send(buf, pos, nleft);

                pos += n;
                nleft -= n;
            }
        }
    }
}
