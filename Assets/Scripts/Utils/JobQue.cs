using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;

namespace Assets.Scripts.Utils
{
    public class JobQue<T> where T: class
    {// JobQue ��������, �������ݻ���
        // ��
        private object lk_ = new object();
        // ����
        private Queue<T> que_;
        // ����
        private static JobQue<T> instance_; 

        public static JobQue<T> Instance
        {// ��������
            get
            {
                if (instance_ == null)
                {
                    instance_ = new JobQue<T>();
                }
                return instance_;
            }
        }

        private JobQue()
        {// ˽�й��캯��
            que_ = new Queue<T>();
        }

        public void Push(T element)
        {// ���
            lock (lk_)
            {
                que_.Enqueue(element);
            }
        }

        public T Pop()
        {// ����
            T pack = null;
            lock (lk_)
            {
                if (que_.Count > 0)
                    pack = que_.Dequeue();
            }
            return pack;
        }
    }
}


