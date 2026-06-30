using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeDirection
{
    public Node node;
    public char direction;
}

public class Node : MonoBehaviour
{
    [SerializeField] private List<NodeDirection> nodes;
    [SerializeField] private string id;

    public string ID => id;
    public List<NodeDirection> Nodes => nodes;
}
