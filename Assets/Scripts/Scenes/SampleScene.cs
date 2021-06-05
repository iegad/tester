using Alfred;
using Assets.Scripts.Scenes;
using Google.Protobuf;
using NKraken;
using NKraken.nw.client;
using pb;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SampleScene : BasicScene
{
    public string Address;
    public int Port;
    public NW_PROTOCOL protocol;
    public GameObject loginForm;
    public Text time;

    // UI
    public InputField phoneNum;
    public InputField vcode;

    private Task initAsync_;

    #region 继承实现
    protected override void BeginInit()
    {
        initAsync_ = new Task(() =>
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Cerberus.Instance.Init(protocol, Address, Port))
                        break;
                    Thread.Sleep(3000);
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

    protected override IEnumerator EndInit()
    {
        while (!initAsync_.IsCompleted)
            yield return null;

        initAsync_.Dispose();

        if (Error.Length > 0)
        {
            Debug.LogError(string.Format("Err: {0}", Error));
            yield break;
        }

        loginForm.SetActive(true);
    }

    protected override void DispatchMessage(Package package)
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
    #endregion

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

    

    public void btnGetVCodeClick()
    {
        System.Random random = new System.Random();
        vcode.text = random.Next(99999).ToString();
    }

    public void btnLoginClick()
    {
        // 发送登录请求
    }

    void Awake()
    {
        loginForm.SetActive(false);

        OnError += (err) =>
        {
            Debug.LogError(err);
        };

        OnIOError += (err) =>
        {
            Debug.LogError(err);

            Cerberus.Instance.Reconnect();
        };
    }

    void Start()
    {
        BeginInit();
        StartCoroutine(EndInit());
    }

    void Update()
    {
        time.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF");
        DispatchPackage();
    }
}
