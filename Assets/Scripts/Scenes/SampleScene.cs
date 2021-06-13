using Assets.Scripts.Scenes;
using pb;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Google.Protobuf;

public class SampleScene : BaseScene
{
    public GameObject loginForm;
    public Text time;

    // UI
    public InputField phoneNum;
    public InputField vcode;

    #region 继承实现
    protected override void DispatchMessage(Package package)
    {
        switch (package.MID)
        {
            case MessageID.MidUserLoginRsp:
                userLoginRspHandle(UserLoginRsp.Parser.ParseFrom(package.Data));
                break;

            default:
                Debug.LogError(string.Format("MessageID {0} is invalid", package.MID));
                break;
        }
    }
    #endregion



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

        LoadScene(1);
    }

    public void btnGetVCodeClick()
    {
        System.Random random = new System.Random();
        vcode.text = random.Next(99999).ToString();
    }

    public void btnLoginClick()
    {
        int res = UserLogin(phoneNum.text, vcode.text);
        if (res < 0)
            Debug.LogError(res);
    }

    void Awake()
    {
        loginForm.SetActive(false);
    }

    void Start()
    {
        BeginInitConnection(() =>
        {
            if (Hydra.Sphinx != null)
                loginForm.SetActive(true);
        });
    }

    void Update()
    {
        time.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF");
        DispatchPackage();
    }
}
