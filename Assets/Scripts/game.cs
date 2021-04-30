using Base;
using Google.Protobuf;
using UnityEngine;
using Assets.Scripts.Utils;


public class game : MonoBehaviour
{
    public string Address;
    public int Port;

    private TcpClient client;

    private int n = 0;

    void Start()
    {
        client = new TcpClient(Address, Port, JobQue<Package>.Instance);
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

    void Update()
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
            Log.Debug(rsp.UserID);
            Log.Debug(rsp.AppID);
            Log.Debug(rsp.AppSecret);

            client.Write(outPack.ToByteArray());
            n++;

            if (n == 1000)
            {
                client.Stop();
            }
        }
    }
}
