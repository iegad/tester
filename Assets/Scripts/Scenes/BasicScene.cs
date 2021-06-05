using Alfred;
using Google.Protobuf;
using NKraken;
using NKraken.nw.client;
using pb;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Scenes
{
    public delegate void InitedHandler();

    public class BasicScene : MonoBehaviour
    {
        public int RECONNECTED_COUNT = Cerberus.RECONNECT_COUNT;
        public int RECONNECTED_INTVAL = Cerberus.RECONNECT_INTVAL;

        public NW_PROTOCOL Protocol;
        public string Host;
        public int Port;

        protected Task initAsync_;

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


            initAsync_ = new Task(() =>
            {
                if (Cerberus.Instance.Connected)
                    return;

                try
                {
                    for (int i = 0; i < RECONNECTED_COUNT; i++)
                    {
                        if (Cerberus.Instance.Init(Protocol, Host, Port))
                            break;
                        Thread.Sleep(RECONNECTED_INTVAL);
                    }


                    GetNodeReq req = new GetNodeReq();
                    req.Paths.Add("sphinx");

                    Cerberus.Instance.SendPackage(new Package()
                    {
                        PID = (int)CerberusID.PidGetNodeReq,
                        Data = ByteString.CopyFrom(req.ToByteArray())
                    });
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });
            initAsync_.Start();
        }

        protected virtual IEnumerator EndInit(InitedHandler handler)
        {
            if (initAsync_ != null)
            {
                while (!initAsync_.IsCompleted)
                    yield return null;

                initAsync_.Dispose();

                if (Error.Length > 0)
                {
                    Debug.LogError(string.Format("Err: {0}", Error));
                    handler?.Invoke();
                    yield break;
                }
            }
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
                        case -1: OnIOError(pack.Data.ToStringUtf8()); break;

                        // 消息重复发送
                        case (int)PackageID.Idempotent: OnPackageRepeated(pack); break;

                        // 心跳
                        case (int)CerberusID.PidPong: OnPong(pack); break;

                        // 节点消息
                        case (int)CerberusID.PidNodeDelivery: DispatchMessage(pack); break;

                        // 被踢事件
                        case (int)CerberusID.PidKickUser: OnKickUser(pack); break;

                        // 未知PID
                        default: OnPIDUnkown(pack); break;
                    }
                }
            } while (false);
        }


        // 错误事件
        protected virtual void OnError(string err)
        {
            Debug.LogError(err);
        }

        // IO错误事件
        protected virtual void OnIOError(string err)
        {
            Debug.LogError(err);
            // TODO: 重连需要改为异步
            for (int i = 0; i < RECONNECTED_COUNT; i++)
            {
                if (Cerberus.Instance.Reconnect())
                    break;
                Thread.Sleep(RECONNECTED_INTVAL);
            }
        }

        // 消息重复发送事件
        protected virtual void OnPackageRepeated(Package package)
        {
            Debug.LogWarning(string.Format("package repeated: {0}", package));
        }

        // 心跳回包事件
        protected virtual void OnPong(Package package)
        {
            Debug.Log(string.Format("- Ping <-> Pong: {0}", package));
        }

        // 被踢事件
        protected virtual void OnKickUser(Package package)
        {
            // TODO:....
        }

        // 未知消息事件
        protected virtual void OnPIDUnkown(Package package)
        {
            Debug.LogWarning(string.Format("unkown package: {0}", package));
        }
    }
}
