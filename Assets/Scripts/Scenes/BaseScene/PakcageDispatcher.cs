using Assets.Scripts.comm;
using NKraken;
using pb;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Scenes
{
    public partial class BaseScene : MonoBehaviour
    {
        /// <summary>
        /// 包分发
        /// </summary>
        protected virtual void DispatchPackage()
        {
            do
            {
                if (Error != null && Error.Length > 0)
                {
                    OnError(Error);
                    break;
                }

                var inPackage = Cerberus.Instance.Update();
                if (inPackage == null)
                    break;

                Package outPackage = MessageManager.Instance.Get(inPackage.Seq);
                if (outPackage == null)
                {
                    Debug.LogWarning(string.Format("未匹配到出包: {0}", inPackage.ToString()));
                    break;
                }

                MessageManager.Instance.Remove(inPackage);

                switch (inPackage.PID)
                {
                    // IO错误
                    case PackageID.PidIowrite:
                    case PackageID.PidIoread:
                        OnIOError(inPackage.Data.ToStringUtf8());
                        break;

                    // 包重复
                    case PackageID.PidIdempotent:
                        OnPackageRepeated(inPackage);
                        break;

                    // 心跳
                    case PackageID.PidPong:
                        OnPong(inPackage);
                        break;

                    // 节点消息分发
                    case PackageID.PidNodeDelivery:
                        DispatchMessage(inPackage);
                        break;

                    // 踢人
                    case PackageID.PidKickUser:
                        OnKickUser(inPackage);
                        break;

                    // 查询节点响应
                    case PackageID.PidGetNodesRsp:
                        OnGetNodesRsp(inPackage);
                        break;

                    // 未知的PID
                    default:
                        OnPIDUnkown(inPackage);
                        break;
                }
            } while (false);
        }

        // 错误句柄
        protected virtual void OnError(string err)
        {
            Debug.LogError(err);
        }

        // IO错误
        protected virtual void OnIOError(string err)
        {
            Debug.LogError(err);
            // TODO: 重连需要改为异步
            for (int i = 0; i < ReconnectedCount; i++)
            {
                if (Cerberus.Instance.Reconnect())
                    break;
                Thread.Sleep(ReconnectedIntval);
            }
        }

        // 包重复发送
        protected virtual void OnPackageRepeated(Package package)
        {
            Debug.LogWarning(string.Format("package repeated: {0}", package));
        }

        // 心跳回包
        protected virtual void OnPong(Package package)
        {
            Debug.Log(string.Format("- Ping <-> Pong: {0}", package));
        }

        // 被踢除
        protected virtual void OnKickUser(Package package)
        {
            // TODO:....
        }

        protected virtual void OnGetNodesRsp(Package package)
        {
            GetNodesRsp rsp = GetNodesRsp.Parser.ParseFrom(package.Data);
            if (rsp.Code != 0)
            {
                Debug.LogError(rsp.Error);
                return;
            }

            foreach (var node in rsp.Nodes)
            {
                Hydra.SetNode(node);
            }
        }

        // 未知PID
        protected virtual void OnPIDUnkown(Package package)
        {
            Debug.LogWarning(string.Format("unkown package: {0}", package));
        }

        /// <summary>
        /// 消息分发, 由各场景自行实现
        /// </summary>
        /// <param name="pack">应答包</param>
        protected virtual void DispatchMessage(Package pack)
        {
            switch (pack.MID)
            {
                // 用户登录应答
                case MessageID.MidUserLoginRsp:
                    UserLoginHandle(pb.UserLoginRsp.Parser.ParseFrom(pack.Data));
                    break;

                default:
                    Debug.LogError(string.Format("MessageID {0} is invalid", pack.MID));
                    break;
            }
        }
    }
}

