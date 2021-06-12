using NKraken;
using pb;
using System;

public static class CerberusEx
{
    private static Node sphinx_;
    private static void setSphinx(this Cerberus cerberus, Node node)
    {
        sphinx_ = node;
    }

    public static Node Sphinx(this Cerberus cerberus)
    {
        return sphinx_;
    }

    public static void SetNode(this Cerberus cerberus, Node node)
    {
        string key = node.NodePath.Substring(0, node.NodePath.IndexOf('/'));
        switch (key)
        {
            case "sphinx":
                cerberus.setSphinx(node);
                break;

            default:
                throw new Exception(string.Format("unkown node: {0}", node.NodePath));
        }
    }
}
