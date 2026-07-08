using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

/*
 *
 *  Recuerda cambiar que node queda como actual node al iniciar ya con los fantasmas bien
 * 
 */

public class Ghost : MonoBehaviour
{
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private Node startNode;
    [SerializeField] private Node destinyNode;
    
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    
    private List<Node> _path;
    private AStar _aStar;
    private Node _actualNode;
    private bool _pathComplete;
    private Vector2 _lastDirection;
    void Start()
    {
        _aStar = new AStar();

        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _actualNode = startNode;
        _pathComplete = false;
        _lastDirection = Vector2.zero;
        
        CreatePath();
    }
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (!_pathComplete) GoToDestiny();
        if (transform.position == destinyNode.transform.position) _pathComplete = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Node"))
        {
            Node node =  other.GetComponent<Node>();
            
            _actualNode = node;
            
            // Debug.Log(_actualNode);
        }
    }

    private void CreatePath()
    {
        _path = _aStar.FindPath(startNode, destinyNode);
        _path.Reverse();
        _path.Remove(startNode);
    }

    private void GoToDestiny()
    {
        // float distance = Vector2.Distance(_rigidbody.position, (Vector2)_path[0].transform.position);
        if (_actualNode.ID != _path[0].ID)
        {
            Vector2 direction = ObtainDirection(_path[0]);
            _rigidbody.MovePosition(_rigidbody.position + direction * speed * Time.fixedDeltaTime);
            _lastDirection = new Vector2(direction.x, direction.y);
        }
        else
        {
            // _rigidbody.MovePosition(_path[0].transform.position * Time.fixedDeltaTime);
            if (IsCompletelyInside(_path[0].GetComponent<Collider2D>()))
            {
                transform.position = _path[0].transform.position;
                _path.RemoveAt(0);
            }
            else
            {
                _rigidbody.MovePosition(_rigidbody.position + _lastDirection * speed * Time.fixedDeltaTime);
            }
        }
    }

    private Vector2 ObtainDirection(Node destinyNode)
    {
        char direction = ' ';
        foreach (NodeDirection nodeDir in _actualNode.Nodes)
        {
            if (nodeDir.node.ID == destinyNode.ID)
            {
                direction = nodeDir.direction;
                break;
            }
        }

        return direction switch
        {
            'U' => Vector2.up,
            'D' => Vector2.down,
            'L' => Vector2.left,
            'R' => Vector2.right,
            _ => Vector2.zero
        };
    }
    
    private bool IsCompletelyInside(Collider2D node)
    {
        Bounds nodeBounds = node.bounds;
        Bounds ghostBounds = _collider.bounds;

        return nodeBounds.Contains(ghostBounds.min) && nodeBounds.Contains(ghostBounds.max);
    }
}
