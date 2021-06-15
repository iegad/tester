using pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.comm
{
    public class MessageManager
    {
        private readonly object lkr_ = new object();
        private readonly SortedDictionary<long, Package> sd_ = new SortedDictionary<long, Package>();

        static private MessageManager instance_ = new MessageManager();
        public static MessageManager Instance
        {
            get
            {
                return instance_;
            }
        }

        private MessageManager() { }

        public void Set(Package pack)
        {
            if (pack == null || pack.Seq <= 0)
                return;

            lock (lkr_)
            {
                Debug.Log(string.Format("发送消息: {0}", pack));
                sd_.Add(pack.Seq, pack);
            }
        }

        public Package Get(long seq)
        {
            if (seq <= 0)
                return null;

            lock (lkr_)
            {
                return sd_[seq];
            }
        }

        public void Remove(Package pack)
        {
            if (pack == null || pack.Seq <= 0)
                return;

            lock(lkr_)
            {
                Debug.Log(string.Format("接收消息: {0}", pack));
                sd_.Remove(pack.Seq);
            }
        }
    }
}


