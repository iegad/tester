using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Utils;
using Base;
using System.Text;

namespace Assets.Scripts.Network
{
    public class TcpClient
    {
        const int HEAD_SIZE = 2;
        const int RECV_BUFF = 1024 * 1024;
        const int SEND_BUFF = 1024 * 512;

        private string addr_;
        private int port_;
        private JobQue que_;
        private Socket sock_;
        private Thread thread_;

        public delegate int ConnectHandler(string localEndpoint, string remoteEndpoint);
        public event ConnectHandler Connected;


        public TcpClient(string addr, int port, JobQue que)
        {
            addr_ = addr;
            port_ = port;
            que_ = que;
            
        }

        /// <summary>
        /// 
        /// </summary>
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
                sock_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, RECV_BUFF);
                sock_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, SEND_BUFF);
                ThreadStart targ = new ThreadStart(_start);
                thread_ = new Thread(targ);
                thread_.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
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
                throw new Exception("socket is null");
            }

            try
            {
                sock_.Connect(addr_, port_);

                Debug.Log("连接成功");

                while (sock_.Connected)
                {
                    var pack = _readPackage();
                    if (pack != null)
                        que_.Push(pack);
                }

                Debug.Log("线程退出");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private Package _readPackage()
        {
            ushort nlen = _readHead();
            byte[] data = _readBody(nlen);
            Debug.Log(Encoding.UTF8.GetString(data));

            return null;
        }

        private ushort _readHead()
        {
            byte[] buf = new byte[HEAD_SIZE];
            _readn(ref buf);
            var n = Endian.ToLocal(ref buf);
            n = (ushort)~n;
            n += 1;

            return n;
        }

        private byte[] _readBody(ushort nlen)
        {
            byte[] buf = new byte[nlen];
            _readn(ref buf);
            return buf;
        }


        public void Write(byte[] data)
        {
            _writen(data);
        }

        private void _readn(ref byte[] data)
        {
            int n = 0, nleft = data.Length, pos = 0;

            while (nleft > 0)
            {
                n = sock_.Receive(data, pos, nleft, SocketFlags.None);
                nleft -= n;
                pos += n;
            }
        }

        private void _writen(byte[] data)
        {
            int n = HEAD_SIZE + data.Length;
            ushort nlen = (ushort)data.Length;
            nlen -= 1;
            nlen = (ushort)~nlen;
            var hbuf = BitConverter.GetBytes(nlen);
            Endian.ToBig(ref hbuf);

            var buf = new byte[n];
            hbuf.CopyTo(buf, 0);
            data.CopyTo(buf, HEAD_SIZE);

            int nleft = n, pos = 0;

            while(nleft > 0)
            {
                n = sock_.Send(buf, pos, nleft, SocketFlags.None);
                pos += n;
                nleft -= n;
            }
        }
    }
}

