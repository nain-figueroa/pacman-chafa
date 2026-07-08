using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AStar
{
    private List<Node> _open = new();
    private List<Node> _close = new();

    public List<Node> FindPath(Node startNode, Node finishNode)
    {
        startNode.ModifyLabel(0.0f, (Math.Abs(startNode.transform.position.x - finishNode.transform.position.x) + Math.Abs(startNode.transform.position.y - finishNode.transform.position.y)), null);
        _open.Add(startNode);

        Node node = startNode;
        while (_close.Find(n => n.ID == finishNode.ID) == null)
        {
            float minFn = _open[0].Label.Fn;
            foreach (Node oNode in _open)
            {
                if (oNode.Label.Fn < minFn) minFn = oNode.Label.Fn;
            }

            node = _open.Find(n => n.Label.Fn == minFn);

            foreach (NodeDirection nodeDir in node.Nodes)
            {
                if (!_close.Contains(nodeDir.node))
                {
                    nodeDir.node.ModifyLabel(
                        (node.Label.Fn + nodeDir.weight),
                        (Math.Abs(nodeDir.node.transform.position.x - finishNode.transform.position.x) + Math.Abs(nodeDir.node.transform.position.y - finishNode.transform.position.y)),
                        node);

                    _open.Add(nodeDir.node);
                }
            }

            _close.Add(node);
            _open.RemoveAll(n => n.ID == node.ID);
        }

        List<Node> bestPath = new();

        node = finishNode;
        while (node != null)
        {
            bestPath.Add(node);
            node = node.Label.Father;
        }

        return bestPath;
    }
}
