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
    public delegate void InitedHandler();

    public partial class BaseScene : MonoBehaviour
    {
        public NW_PROTOCOL Protocol;
        public string Host;
        public int Port;

        protected Task initAsync_;
        private long seq_;

        // -------------------------- 属性 --------------------------
        protected virtual string Error { get; set; }
        public int ReconnectedCount { get; set; }
        public int ReconnectedIntval { get; set; }
        protected long Seq { get { return ++seq_; } }

        // -------------------------- 方法 --------------------------

        // 发包
        protected void SendPackage(long seq, PackageID pid, MessageID mid, ByteString data, Node node)
        {
            Package pack = new Package
            {
                Seq = (int)seq,
                PID = pid,
                MID = mid,
                Data = data,
                ToNodeAddr = node.Addr
            };
            Cerberus.Instance.SendPackage(pack);
        }

        /// <summary>
        /// 开始连接, 异步
        /// </summary>
        protected void BeginInitConnection(InitedHandler callback)
        {
            if (ReconnectedCount <= 0)
                ReconnectedCount = 8;

            if (ReconnectedIntval <= 0)
                ReconnectedIntval = 3000;

            initAsync_ = new Task(() =>
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

                    Cerberus.Instance.SendPackage(new Package()
                    {
                        PID = PackageID.PidGetNodesReq,
                        Data = req.ToByteString()
                    });
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });
            initAsync_.Start();
            StartCoroutine(endInitConnection(callback));
        }

        /// <summary>
        /// 初始化连接完成
        /// </summary>
        /// <param name="callback">初始化连接完成后, 会调用该回调</param>
        /// <returns></returns>
        private IEnumerator endInitConnection(InitedHandler callback)
        {
            if (initAsync_ != null)
            {
                while (!initAsync_.IsCompleted)
                    yield return null;
                initAsync_.Dispose();

                if (Error != null && Error.Length > 0)
                {
                    Debug.LogError(string.Format("Err: {0}", Error));
                    yield break;
                }

                callback?.Invoke();
            }
        }

        /// <summary>
        /// 消息分发, 由各场景自行实现
        /// </summary>
        /// <param name="pack">消息包</param>
        protected virtual void DispatchMessage(Package pack)
        {
            throw new NotImplementedException();
        }

        protected void LoadScene(int sceneIndex)
        {
            _ = StartCoroutine(loadLoginScene(sceneIndex));
        }

        IEnumerator loadLoginScene(int sceneIndex)
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
