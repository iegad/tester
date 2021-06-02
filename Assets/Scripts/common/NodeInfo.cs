using Alfred;
using NKraken.nw.client;
using NKraken.security;
using System;

using cerberus_ = NKraken.cerberus.Cerberus;

public class NodeInfo
{
    private static byte[] EncodeHandler(byte[] data)
    {
        return AES128.Encode(data, cerberus_.Instance.Key);
    }

    private static byte[] DecodeHandler(byte[] data)
    {
        return AES128.Decode(data, cerberus_.Instance.Key);
    }

    private NodeInfo() { }

    private static NodeInfo instance_;
    public static NodeInfo Instance
    {
        get
        {
            if (instance_ == null)
                instance_ = new NodeInfo();
            return instance_;
        }
    }

    public Node Cerberus { get; set; }
    public Node Sphinx { get; set; }

    public void SetNode(Node node)
    {
        string key = node.NodePath.Substring(0, node.NodePath.IndexOf('/'));
        switch (key)
        {
            case "cerberus":
                string[] strarr = node.Addr.Split(':');
                if (strarr.Length != 2)
                    throw new Exception(string.Format("Addr is invalid: {0}", node.Addr));


                int port;
                if (!int.TryParse(strarr[1], out port))
                    throw new Exception(string.Format("Addr is invalid: {0}", node.Addr));

                cerberus_.Instance.Init(NW_PROTOCOL.TCP, strarr[0], port, EncodeHandler, DecodeHandler);
                cerberus_.Instance.Run();

                Cerberus = node;
                break;

            case "sphinx":
                Sphinx = node;
                break;

            default:
                throw new Exception(string.Format("unkown node: {0}", node.NodePath));
        }
    }
}
