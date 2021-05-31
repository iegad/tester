using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string NodePath { get; set; }
    public string Addr { get; set; }
}

public class NodeInfo
{
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
}
