using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public class NodeDirection
{
    public Node node;
    public char direction;
    public float weight;
}

public class NodeLabel
{
    public float Gn { get; set; }
    public float Hn { get; set; }
    public float Fn { get; set; }
    public Node Father { get; set; }
}

public class Node : MonoBehaviour
{
    [SerializeField] private List<NodeDirection> nodes;
    [SerializeField] private string id;
    
    private NodeLabel _label = new();

    void Start()
    {
        foreach (NodeDirection node in nodes)
        {
            node.weight = node.direction switch
            {
                'U' => Math.Abs(node.node.transform.position.y - transform.position.y),
                'D' => Math.Abs(transform.position.y - node.node.transform.position.y),
                'L' => Math.Abs(transform.position.x - node.node.transform.position.x),
                'R' => Math.Abs(node.node.transform.position.x - transform.position.x)
            };
        }
    }

    public string ID => id;
    public List<NodeDirection> Nodes => nodes;

    public NodeLabel Label => _label;

    public void ModifyLabel(float gn, float hn, Node father)
    {
        _label.Gn = gn;
        _label.Hn = hn;
        _label.Fn = gn + hn;
        _label.Father = father;
    }
}
