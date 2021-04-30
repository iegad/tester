using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Base;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public interface IShadowEvent
    {

    }
    public class Shadow
    {
        private TcpClient client_;
        private IShadowEvent event_;

        private static Shadow instance_;
        public static Shadow Instance
        {
            get
            {
                if (instance_ == null)
                    instance_ = new Shadow();
                return instance_;
            }
        }

        public int Init(string addr, int port, IShadowEvent ev)
        {
            int ret = -1;
            do
            {
                if (addr.Length == 0)
                {
                    Debug.LogError("addr is invalid");
                    break;
                }
                    

                if (port <= 0 || port > 65535)
                    break;

                if (ev == null)
                    break;

                client_ = new TcpClient(addr, port, JobQue<Package>.Instance);

                client_.Start();

                ret = 0;
            } while (false);

            return ret;
        }

        public void Do()
        {

        }

        private Shadow()
        {
        }
    }
}
