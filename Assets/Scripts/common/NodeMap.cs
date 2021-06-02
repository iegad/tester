using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alfred;
using NKraken.nw.client;
using System;

using cerberus_ = NKraken.cerberus.Cerberus;

public class NodeInfo
{
    public string Host { get; set; }

    public string NodePath { get; set; }

    public override string ToString()
    {
        return string.Format("[{0}] => {1}", Host, NodePath);
    }
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

    public NodeInfo Cerberus { get; set; }
    public NodeInfo Sphinx { get; set; }

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

                cerberus_.Instance.Init(NW_PROTOCOL.TCP, strarr[0], port);

                Cerberus = new NodeInfo
                {
                    NodePath = node.NodePath,
                    Host = node.Addr
                };

                break;

            case "sphinx":
                Sphinx = new NodeInfo
                {
                    NodePath = node.NodePath,
                    Host = node.Addr
                };
                break;

            default:
                throw new Exception(string.Format("unkown node: {0}", node.NodePath));
        }
    }
}
