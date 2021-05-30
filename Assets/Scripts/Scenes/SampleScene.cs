using Alfred;
using Google.Protobuf;
using NKraken.nw.client;
using NKraken.piper;
using System.Collections;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    public string Address;
    public int Port;


    IEnumerator GetNodes()
    {
        IClient client = ClientFactory.New(NW_PROTOCOL.TCP);
        yield return null;
        client.Connect(Address, Port);

        GetNodeReq req = new GetNodeReq();
        req.Paths.Add("cerberus/user/tcp");
        req.Paths.Add("sphinx");

        Piper piper = new Piper
        {
            Name = "GetNode",
            Data = ByteString.CopyFrom(req.ToByteArray())
        };

        int n = client.Write(piper.ToByteArray());
        yield return null;

        if (n <= 0)
        {
            Debug.LogError("·¢ËÍÊý¾ÝÊ§°Ü");
            yield break;
        }

        byte[] rbuf = client.Read();
        yield return null;
        var rsp = GetNodeRsp.Parser.ParseFrom(rbuf);
        yield return rsp;
        Debug.Log(rsp.ToString());
    }

    void Start()
    {
        StartCoroutine(GetNodes());
    }

    void Update()
    {
    }
}
