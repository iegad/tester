using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kraken;
using Base;
using Google.Protobuf;
using System.Text;

public class game : MonoBehaviour
{
    public string Address;
    public int Port;
    int connectedHandler()
    {
        Debug.Log(string.Format("连接{0}:{1}成功", Address, Port));
        return 0;
    }

    void Start()
    {
        TcpClient client = new TcpClient(Address, Port);
        client.Connected += connectedHandler;

        client.Connect();

        var req = new Cerberus.UserAuthReq();
        req.AppID = "11112222";
        req.UserID = 88888;
        req.AppSecret = "213423413241234";

        var inPack = new Package();
        inPack.PID = 100001;
        inPack.MID = 200000;
        inPack.Data = ByteString.CopyFrom(req.ToByteArray());
        var data = inPack.ToByteArray();

        var outPack = Package.Parser.ParseFrom(data);
        Debug.Log(outPack.PID);
        Debug.Log(outPack.MID);

        var rsp = Cerberus.UserAuthReq.Parser.ParseFrom(outPack.Data.ToByteArray());
        Debug.Log(rsp.AppID);
        Debug.Log(rsp.UserID);
        Debug.Log(rsp.AppSecret);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
