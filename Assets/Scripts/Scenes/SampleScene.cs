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
                    return;

                client.Close();
                GetNodeRsp rsp = GetNodeRsp.Parser.ParseFrom(rbuf);

                if (rsp == null)
                    return;

                foreach (var node in rsp.Nodes)
                    NodeInfo.Instance.SetNode(node);

                Debug.Log(NodeInfo.Instance.Cerberus);
                Debug.Log(NodeInfo.Instance.Sphinx);

                Thread.Sleep(3000);
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
        UserLoginReq req = new UserLoginReq()
        {
            PhoneNum = phoneNum.text,
            VCode = vcode.text
        };
        Package package = new Package()
        {
            PID = (int)CerberusID.PidUserDelivery,
            MID = (int)SphinxID.MidUserLoginReq,
            Idempotent = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10,
            ToNodeAddr = NodeInfo.Instance.Sphinx.Addr,
            Data = ByteString.CopyFrom(req.ToByteArray())
        };

        try
        {
            Cerberus.Instance.SendPackage(package);
        }
        catch (Exception ex)
        {
            errstr_ = ex.Message;
        }
    }

    IEnumerator waitConnected()
    {
        while (!getNodeTask_.IsCompleted)
        {
            getNodeTask_.Wait(10);
            yield return null;
        }

        getNodeTask_.Dispose();

        if (errstr_.Length > 0)
        {
            Debug.LogError(string.Format("Err: {0}", errstr_));
            yield break;
        }

        loginForm.SetActive(true);
    }

    void dispatchMessage(Package package)
    {

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
                    // IO����
                    case -1:
                        Debug.LogError(Encoding.UTF8.GetString(pack.Data.ToByteArray()));
                        break;

                    // ��Ϣ�ظ�����
                    case (int)PackageID.Idempotent:
                        Debug.LogWarning("");
                        break;

                    // ����
                    case (int)CerberusID.PidPong:
                        Debug.Log(string.Format("Pong: {0}", pack));
                        break;

                    // �ڵ���Ϣ
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
