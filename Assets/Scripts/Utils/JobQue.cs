using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;

namespace Assets.Scripts.Utils
{
    public class JobQue<T> where T: class
    {// JobQue 工作队列, 用作数据缓冲
        // 锁
        private object lk_ = new object();
        // 队列
        private Queue<T> que_;
        // 单例
        private static JobQue<T> instance_; 

        public static JobQue<T> Instance
        {// 单例属性
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
        {// 私有构造函数
            que_ = new Queue<T>();
        }

        public void Push(T element)
        {// 入队
            lock (lk_)
            {
                que_.Enqueue(element);
            }
        }

        public T Pop()
        {// 出队
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


