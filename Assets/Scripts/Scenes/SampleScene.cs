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

public class SampleScene : MonoBehaviour
{
    public string Address;
    public int Port;
    public NW_PROTOCOL protocol;
    public GameObject loginForm;
    public Text time;

    private string errstr_ = "";
    private Task connectTask_;

    void getNodes()
    {
        connectTask_ = new Task(() =>
        {
            try
            {
                IClient client = ClientFactory.New(protocol);
                client.Connect(Address, Port);

                GetNodeReq req = new GetNodeReq();
                req.Paths.Add("cerberus/user/tcp");
                req.Paths.Add("sphinx");

                Piper piper = new Piper
                {
                    Name = "GetNode",
                    Data = ByteString.CopyFrom(req.ToByteArray())
                };

                client.Write(piper.ToByteArray());
  
                byte[] rbuf = client.Read();
                if (rbuf == null)
                {
                    return;
                }
 
                GetNodeRsp rsp = GetNodeRsp.Parser.ParseFrom(rbuf);

                if (rsp == null)
                {
                    return;
                }

                foreach (var node in rsp.Nodes)
                {
                    NodeMap.Instance.SetNode(node);
                }

                Debug.Log(NodeMap.Instance.Cerberus);
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                errstr_ = ex.Message;
            }
        });
        connectTask_.Start();
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

    IEnumerator waitConnected()
    {
        while(!connectTask_.IsCompleted) {
            connectTask_.Wait(10);
            yield return null;
        }

        if (errstr_.Length > 0)
        {
            Debug.LogError(string.Format("Err: {0}", errstr_));
            yield break;
        }

        loginForm.SetActive(true);
    }

    void Update()
    {
        time.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
