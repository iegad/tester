using Assets.Scripts.comm;
using Google.Protobuf;
using NKraken;
using NKraken.nw.client;
using pb;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Scenes
{
    // 初始化完成完句柄
    public delegate void ConnectHandler();

    public partial class BaseScene : MonoBehaviour
    {
        public NW_PROTOCOL Protocol;
        public string Host;
        public int Port;

        

        protected Task connectAsync_;
        

        // -------------------------- 属性 --------------------------
        protected virtual string Error { get; set; }
        public int ReconnectedCount { get; set; }
        public int ReconnectedIntval { get; set; }

        // -------------------------- 方法 --------------------------

        private readonly object seqlk_ = new object();
        private long seq_;
        // 发包
        protected void SendPackage(PackageID pid, MessageID mid, ByteString data, Node node)
        {
            long seq = -1;
            lock (seqlk_)
            {
                seq = ++seq_;
            }

            Package pack = new Package
            {
                Seq = seq,
                PID = pid,
                MID = mid,
                Data = data,
                ToNodeAddr = node != null ? node.Addr : ""
            };

            Cerberus.Instance.SendPackage(pack);
            MessageManager.Instance.Set(pack);
        }

        /// <summary>
        /// 开始连接, 异步
        /// </summary>
        protected void BeginConnection(ConnectHandler callback)
        {
            if (ReconnectedCount <= 0)
                ReconnectedCount = 8;

            if (ReconnectedIntval <= 0)
                ReconnectedIntval = 3000;

            connectAsync_ = new Task(() =>
            {
                if (Cerberus.Instance.Connected)
                    return;

                try
                {
                    for (int i = 0; i < ReconnectedCount; i++)
                    {
                        if (Cerberus.Instance.Init(Protocol, Host, Port))
                            break;
                        Thread.Sleep(ReconnectedIntval);
                    }


                    GetNodesReq req = new GetNodesReq();
                    req.Paths.Add("sphinx");

                    SendPackage(PackageID.PidGetNodesReq, 0, req.ToByteString(), null);
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });
            connectAsync_.Start();
            StartCoroutine(endConnection(callback));
        }

        /// <summary>
        /// 初始化连接完成
        /// </summary>
        /// <param name="callback">初始化连接完成后, 会调用该回调</param>
        /// <returns></returns>
        private IEnumerator endConnection(ConnectHandler callback)
        {
            if (connectAsync_ != null)
            {
                while (!connectAsync_.IsCompleted)
                    yield return null;
                connectAsync_.Dispose();

                if (Error != null && Error.Length > 0)
                {
                    Debug.LogError(string.Format("Err: {0}", Error));
                    yield break;
                }

                callback?.Invoke();
            }
        }

        protected void LoadScene(int sceneIndex)
        {
            _ = StartCoroutine(loadLoginScene(sceneIndex));
        }

        protected IEnumerator loadLoginScene(int sceneIndex)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
            ao.allowSceneActivation = false; // 设置该属性后, 场景加载完毕后, 不会马上加载, 而是需要通过代码来控制.
            yield return null;

            while (!ao.isDone)
            {
                if (Mathf.Approximately(ao.progress, 0.9f))
                    ao.allowSceneActivation = true;
                yield return null;
            }
        }
    }
}
