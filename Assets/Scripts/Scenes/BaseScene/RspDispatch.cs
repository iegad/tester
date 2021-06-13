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

                var pack = Cerberus.Instance.Update();
                if (pack != null)
                {
                    switch (pack.PID)
                    {
                        // IO错误
                        case PackageID.PidIowrite:
                        case PackageID.PidIoread:
                            OnIOError(pack.Data.ToStringUtf8());
                            break;

                        // 包重复
                        case PackageID.PidIdempotent:
                            OnPackageRepeated(pack);
                            break;

                        // 心跳
                        case PackageID.PidPong:
                            OnPong(pack);
                            break;

                        // 节点消息分发
                        case PackageID.PidNodeDelivery:
                            DispatchMessage(pack);
                            break;

                        // 踢人
                        case PackageID.PidKickUser:
                            OnKickUser(pack);
                            break;

                        // 查询节点响应
                        case PackageID.PidGetNodesRsp:
                            OnGetNodesRsp(pack);
                            break;

                        // 未知的PID
                        default:
                            OnPIDUnkown(pack);
                            break;
                    }
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
    }
}

