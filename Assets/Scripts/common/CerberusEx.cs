using Alfred;
using NKraken.nw.client;
using NKraken.security;
using System;

using cerberus_ = NKraken.cerberus.Cerberus;

public class CerberusEx
{
    public static byte[] EncodeHandler(byte[] data)
    {
        return AES128.Encode(data, cerberus_.Instance.Key);
    }

    public static byte[] DecodeHandler(byte[] data)
    {
        return AES128.Decode(data, cerberus_.Instance.Key);
    }

    //public void SetNode(Node node)
    //{
    //    string key = node.NodePath.Substring(0, node.NodePath.IndexOf('/'));
    //    switch (key)
    //    {
    //        case "sphinx":
    //            Sphinx = node;
    //            break;

    //        default:
    //            throw new Exception(string.Format("unkown node: {0}", node.NodePath));
    //    }
    //}
}
