using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;

namespace Utils
{
    public class JobQue
    {
        object lk_;
        Queue<Package> que_;

        private static JobQue instance_;

        public static JobQue Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new JobQue();
                }
                return instance_;
            }
        }

        private JobQue()
        {
            lk_ = new object();
            que_ = new Queue<Package>();
        }

        public void Push(Package pack)
        {
            lock (lk_)
            {
                que_.Enqueue(pack);
            }
        }

        public Package Pop()
        {
            Package pack;
            lock (lk_)
            {
                pack = que_.Dequeue();
            }
            return pack;
        }
    }
}


