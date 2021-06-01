using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alfred;
using NKraken.cerberus;
using NKraken.nw.client;
using System;

public enum NodeType
{
    Cerberus = 1,
    Sphinx
}

public class NodeMap
{
    private NodeMap() { }

    private static NodeMap instance_;
    public static NodeMap Instance
    {
        get
        {
            if (instance_ == null)
                instance_ = new NodeMap();
            return instance_;
        }
    }

    private Dictionary<NodeType, string> map_;

    public string this[NodeType nodeType]
    {
        get
        {
            return map_[nodeType];
        }

        set
        {
            map_[nodeType] = value;
        }
    }

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

                Cerberus.Instance.Init(NW_PROTOCOL.TCP, strarr[0], port);
                break;

            case "sphinx":
                map_[NodeType.Sphinx] = node.Addr;
                break;

            default:
                throw new Exception(string.Format("unkown node: {0}", node.NodePath));
        }
    }
}
