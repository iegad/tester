using NKraken.cerberus;
using NKraken.nw.client;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Alfred;
using Google.Protobuf;
using NKraken.piper;
using UnityEngine.UI;
using System.Threading;
using pb;
using System.Text;
using UnityEngine.SceneManagement;

public class SampleScene : MonoBehaviour
{
    public string Address;
    public int Port;
    public NW_PROTOCOL protocol;
    public GameObject loginForm;
    public Text time;

    // UI
    public InputField phoneNum;
    public InputField vcode;

    private string errstr_ = "";
    private Task getNodeTask_;

    void getNodes()
    {
        getNodeTask_ = new Task(() =>
        {
            try
            {
                Cerberus.Instance.Init(protocol, Address, Port, CerberusEx.EncodeHandler, CerberusEx.DecodeHandler);

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
                errstr_ = ex.Message;
            }
        });
        getNodeTask_.Start();
    }

    void connectCerberus()
    {
        //UserLoginReq req = new UserLoginReq()
        //{
        //    PhoneNum = phoneNum.text,
        //    VCode = vcode.text
        //};

        //Package package = new Package()
        //{
        //    PID = (int)CerberusID.PidUserDelivery,
        //    MID = (int)SphinxID.MidUserLoginReq,
        //    Idempotent = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10,
        //    ToNodeAddr = NodeInfo.Instance.Sphinx.Addr,
        //    Data = ByteString.CopyFrom(req.ToByteArray())
        //};

        //try
        //{
        //    Cerberus.Instance.SendPackage(package);
        //}
        //catch (Exception ex)
        //{
        //    errstr_ = ex.Message;
        //}
    }

    IEnumerator waitConnected()
    {
        while (!getNodeTask_.IsCompleted)
            yield return null;

        getNodeTask_.Dispose();

        if (errstr_.Length > 0)
        {
            Debug.LogError(string.Format("Err: {0}", errstr_));
            yield break;
        }

        loginForm.SetActive(true);
    }

    IEnumerator loadLoginScene()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        ao.allowSceneActivation = false; // 设置该属性后, 场景加载完毕后, 不会马上加载, 而是需要通过代码来控制.
        yield return null;

        while (!ao.isDone)
        {
            float prograss = Mathf.Clamp01(ao.progress / 0.9f);
            if (Mathf.Approximately(ao.progress, 0.9f))
                ao.allowSceneActivation = true;
            yield return null;
        }
    }

    void userLoginRspHandle(UserLoginRsp rsp)
    {
        if (rsp == null)
        {
            Debug.LogError("UserLoginRsp is invalid");
            return;
        }
            
        if (rsp.Code != 0)
        {
            Debug.LogError(rsp.Error);
            return;
        }

        StartCoroutine(loadLoginScene());
    }

    void dispatchMessage(Package package)
    {
        switch (package.MID)
        {
            case (int)SphinxID.MidUserLoginRsp:
                userLoginRspHandle(UserLoginRsp.Parser.ParseFrom(package.Data));
                break;

            default:
                Debug.LogError(string.Format("MessageID {0} is invalid", package.MID));
                break;
        }
    }

    void dispatchPackage()
    {
        do
        {
            if (errstr_.Length > 0)
            {
                Debug.LogError(errstr_);
                break;
            }

            var pack = Cerberus.Instance.Update();
            if (pack != null)
            {
                switch (pack.PID)
                {
                    // IO错误
                    case -1:
                        Debug.LogError(Encoding.UTF8.GetString(pack.Data.ToByteArray()));
                        break;

                    // 消息重复发送
                    case (int)PackageID.Idempotent:
                        Debug.LogWarning("");
                        break;

                    // 心跳
                    case (int)CerberusID.PidPong:
                        Debug.Log(string.Format("Pong: {0}", pack));
                        break;

                    // 节点消息
                    case (int)CerberusID.PidNodeDelivery:
                        Debug.Log(string.Format("Node: {0}", pack));
                        dispatchMessage(pack);
                        break;

                    default:
                        Debug.LogError(string.Format("unkown: {0}", pack));
                        break;
                }
            }
        } while (false);
    }

    public void btnGetVCodeClick()
    {
        System.Random random = new System.Random();
        vcode.text = random.Next(99999).ToString();
    }

    public void btnLoginClick()
    {
        connectCerberus();
    }

    void Awake()
    {
        loginForm.SetActive(false);
    }

    void Start()
    {
        getNodes();
        StartCoroutine(waitConnected());
    }

    void Update()
    {
        time.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dispatchPackage();
    }
}
