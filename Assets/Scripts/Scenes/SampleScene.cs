using NKraken.cerberus;
using NKraken.nw.client;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    public string Address;
    public int Port;
    public NW_PROTOCOL protocol;
    public GameObject loginForm;

    private string errstr_ = "";
    private Task connectTask_;
    

    void Awake()
    {
        loginForm.SetActive(false);

        connectTask_ = new Task(() => 
        { 
            try
            {
                Cerberus.Instance.Init(protocol, Address, Port);
            }
            catch (Exception ex)
            {
                errstr_ = ex.Message;
            }
        });
        connectTask_.Start();
    }

    //void GetNodes()
    //{
    //    Cerberus.Instance.

    //    GetNodeReq req = new GetNodeReq();
    //    req.Paths.Add("cerberus/user/tcp");
    //    req.Paths.Add("sphinx");

    //    Piper piper = new Piper
    //    {
    //        Name = "GetNode",
    //        Data = ByteString.CopyFrom(req.ToByteArray())
    //    };

    //    int n = client.Write(piper.ToByteArray());
    //    yield return null;

    //    if (n <= 0)
    //    {
    //        Debug.LogError("发送数据失败");
    //        yield break;
    //    }

    //    byte[] rbuf = client.Read();
    //    yield return null;
    //    var rsp = GetNodeRsp.Parser.ParseFrom(rbuf);
    //    yield return rsp;
    //    Debug.Log(rsp.ToString());
    //}

    void Start()
    {
        StartCoroutine(waitConnected());
    }

    IEnumerator waitConnected()
    {
        while(!connectTask_.IsCompleted) {
            connectTask_.Wait(10);
            yield return null;
        }

        if (errstr_.Length > 0)
        {
            Debug.LogError(string.Format("连接节点失败: {0}", errstr_));
            yield break;
        }

        loginForm.SetActive(true);
    }

    void Update()
    {
        
    }
}
