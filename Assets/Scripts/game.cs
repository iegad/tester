using Base;
using Google.Protobuf;
using UnityEngine;
using Assets.Scripts.Utils;


public class game : MonoBehaviour
{
    public string Address;
    public int Port;
    public Protocol protocol;

    private INetClient client;

    void Start()
    {
        client = NetClientFactory.New(protocol, Address, Port, JobQue<Package>.Instance);
        client.Start();
        

        #region
        var req = new Cerberus.UserAuthReq();
        req.AppID = "Hello";
        req.UserID = 88888;
        req.AppSecret = "World";

        var inPack = new Package();
        inPack.PID = 100001;
        inPack.MID = 200000;
        inPack.Data = ByteString.CopyFrom(req.ToByteArray());
        var data = inPack.ToByteArray();

        client.Write(data);
        #endregion
    }

    private void FixedUpdate()
    {
        var outPack = JobQue<Package>.Instance.Pop();
        if (outPack != null)
        {
            if (outPack.PID != 100001 || outPack.MID != 200000)
            {
                Log.Error("###########");
                return;
            }

            var rsp = Cerberus.UserAuthReq.Parser.ParseFrom(outPack.Data.ToByteArray());

            if (rsp.AppID != "Hello" || rsp.UserID != 88888 || rsp.AppSecret != "World")
            {
                Log.Error("@@@@@@@@@@@@");
                return;
            }

            Log.Debug(ProtocEx.ToJson(outPack));
        }
    }
}
