using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace kraken
{
    public class TcpClient
    {
        private string addr_;
        private int port_;

        public delegate int ConnectHandler();
        public event ConnectHandler Connected;

        private System.Net.Sockets.TcpClient client_;
        public TcpClient(string addr, int port)
        {
            addr_ = addr;
            port_ = port;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
            if (client_ != null)
            {
                if (client_.Connected)
                {
                    client_.Close();
                }
            }

            client_ = new System.Net.Sockets.TcpClient();
            client_.BeginConnect(addr_, port_, _connectHandle, null);
        }

        private void _connectHandle(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    client_.EndConnect(ar);
                    if (Connected() != 0)
                    {
                        client_.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void Write(byte[] data)
        {

        }
    }
}

