using NKraken;
using pb;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Scenes
{
    public class BasicScene : MonoBehaviour
    {
        // -------------------------- 公共事件 --------------------------
        // 错误事件
        protected virtual void OnError(string err)
        {
            Debug.LogError(err);
        }

        // IO错误事件
        protected virtual void OnIOError(string err)
        {
            Debug.LogError(err);
            Cerberus.Instance.Reconnect();
        }

        // 消息重复发送事件
        protected virtual void OnPackageRepeated(Package package)
        {

        }

        // 心跳回包事件
        protected virtual void OnPong(Package package)
        {

        }

        // 被踢事件
        protected virtual void OnKickUser(Package package)
        {

        }

        // 未知消息事件
        protected virtual void OnPIDUnkown(Package package)
        {

        }

        // -------------------------- 属性 --------------------------

        private string errstr_ = "";
        protected virtual string Error 
        { 
            get { return errstr_; } 
            set { errstr_ = value; } 
        }

        // -------------------------- 方法 --------------------------

        protected virtual void BeginInit()
        {
            throw new NotImplementedException();
        }

        protected virtual IEnumerator EndInit()
        {
            throw new NotImplementedException();
        }

        protected virtual void DispatchMessage(Package pack)
        {
            throw new NotImplementedException();
        }

        protected virtual void DispatchPackage() 
        {
            do
            {
                if (errstr_.Length > 0)
                {
                    OnError(errstr_);
                    break;
                }

                var pack = Cerberus.Instance.Update();
                if (pack != null)
                {
                    switch (pack.PID)
                    {
                        // IO错误
                        case -1:
                            OnIOError?.Invoke(pack.Data.ToStringUtf8());
                            break;

                        // 消息重复发送
                        case (int)PackageID.Idempotent:
                            OnPackageRepeated?.Invoke(pack);
                            break;

                        // 心跳
                        case (int)CerberusID.PidPong:
                            OnPong?.Invoke(pack);
                            break;

                        // 节点消息
                        case (int)CerberusID.PidNodeDelivery:
                            DispatchMessage(pack);
                            break;

                        // 被踢事件
                        case (int)CerberusID.PidKickUser:
                            OnKickUser?.Invoke(pack);
                            break;

                        // 未知PID
                        default:
                            OnPIDUnkown?.Invoke(pack);
                            break;
                    }
                }
            } while (false);
        }
    }
}
