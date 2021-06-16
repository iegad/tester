using Assets.Scripts.Scenes;
using pb;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Google.Protobuf;
using Assets.Scripts.comm;

public class SampleScene : BaseScene
{
    public GameObject loginForm;
    public Text time;

    // UI
    public InputField phoneNum;
    public InputField vcode;

    #region 继承实现
    #endregion

    protected override int UserLoginHandle(UserLoginRsp rsp)
    {
        if (rsp == null)
        {
            Debug.LogError("UserLoginRsp is invalid");
            return -1;
        }

        if (rsp.Code != 0)
        {
            Debug.LogError(rsp);
            if (!loginForm.activeInHierarchy)
                loginForm.SetActive(true);
            return -2;
        }

        Config.UserSession = rsp.UserSession;
        PlayerPrefs.SetString("userSession", Config.UserSession.ToString());
        LoadScene(1);
        return 0;
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
        BeginConnection(() =>
        {
            if (Hydra.Sphinx != null)
            {
                do
                {
                    string ustr = PlayerPrefs.GetString("userSession");

                    if (ustr == null || ustr.Length == 0)
                        break;

                    UserSession ss = UserSession.Parser.ParseJson(ustr);
                    if (ss == null)
                        break;

                    int n = UserLogin(ss.UserInfo.UserID, ss.UserInfo.PhoneNum, ss.UserInfo.Email, ss.Token, ss.DeviceCode);
                    if (n == 0)
                        return;
                } while (false);

                loginForm.SetActive(true);
            }
        });
    }

    void Update()
    {
        time.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF");
        DispatchPackage();
    }
}
