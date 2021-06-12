using NKraken;
using pb;
using System;

public static class Hydra
{
    public static Node Sphinx { get; private set; }

    public static void SetNode(Node node)
    {
        string key = node.NodePath.Substring(0, node.NodePath.IndexOf('/'));
        switch (key)
        {
            case "sphinx":
                Sphinx = node;
                break;

            default:
                throw new Exception(string.Format("unkown node: {0}", node.NodePath));
        }
    }
}
