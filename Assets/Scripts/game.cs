using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Network;
using Base;
using Google.Protobuf;
using System.Text;
using System;

public class game : MonoBehaviour
{
    public string Address;
    public int Port;

    private TcpClient client_;

    int connectedHandler()
    {
        Debug.Log(string.Format("连接{0}:{1}成功", Address, Port));
        return 0;
    }

    void Start()
    {
        client_ = new TcpClient(Address, Port, Utils.JobQue.Instance);
        client_.Start();

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

        ushort a = 0x0102;
        var buf = BitConverter.GetBytes(a);
        a = Endian.ToBig(ref buf);

        Debug.Log(string.Format("{0}", a.ToString("X")));

        a = Endian.ToLocal(ref buf);
        Debug.Log(string.Format("{0}", a.ToString("X")));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Home) || Input.GetKey(KeyCode.Menu))
        {
            client_.Stop();
        } else
        {
            try
            {
                client_.Write(Encoding.UTF8.GetBytes("hello world"));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
