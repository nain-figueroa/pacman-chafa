using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 *  Ahora queda ver el tema de los cambios de sprites entre modos
 */
public enum GhostState
{
    Idle, Chase, Scatter, Frightened, Eaten
}

public class Ghost : MonoBehaviour
{
    [SerializeField] private Node startNode, destinyNode;
    [SerializeField] private char id;
    [SerializeField] private Player pacman;
    [SerializeField] private GameObject nodes;
    [SerializeField] private Ghost blinky = null;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Node> cornerNodes;

    private Rigidbody2D _rigidbody;
    private Collider2D _physicCollider;
        
    private AStar _aStar = new AStar();
    private List<Node> _path = new();
    private GhostState _state = GhostState.Idle;
    private Node _actualNode;
    private Vector2 _horizontalMov;
    private float _idleTime, _speed;
    private const float NORMAL_SPEED = 3f;

    #region UnityMethods
    void Start()
    {
        _idleTime = id switch
        {
            'B' => 0f,
            'P' => 1f,
            'I' => 3f,
            'C' => 6f
        };
        _speed = NORMAL_SPEED;
        
        GoToNormalSkin();
        _state = GhostState.Idle;
        StartCoroutine(IdleState());
        
        _rigidbody = GetComponent<Rigidbody2D>();
        _physicCollider = GetComponent<Collider2D>();

        _horizontalMov = Vector2.left;
        
        _actualNode = startNode;
        CreatePath(startNode, destinyNode);
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        if (_state == GhostState.Idle)
        {
            IdleMovement();
            return;
        }

        if (_state == GhostState.Eaten)
        {
            if (_actualNode == startNode)
            {
                _physicCollider.enabled = true;
                _state = GhostState.Idle;
                GoToNormalSkin();
                _speed = NORMAL_SPEED;
                StartCoroutine(IdleState());
                return;
            }
        }
        
        GoToDestiny();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall") && _state == GhostState.Idle)
        {
            _horizontalMov *= -1;
        }

        if (other.gameObject.CompareTag("Player") && _state == GhostState.Frightened)
        {
            _state = GhostState.Eaten;
            //Cambiarlo por un cambio de sprite
            spriteRenderer.color = Color.white;
            _speed *= 2;
            _physicCollider.enabled = false;
        }
    }

    #endregion

    public Node ActualNode => _actualNode;
    public GhostState ActualState => _state;

    public void SetFrightenedMode(bool value)
    {
        if (!value)
        {
            _state = GhostState.Chase;
            GoToNormalSkin();
            return;
        }

        _state = GhostState.Frightened;
        //Esto lo tengo que cambiar por un cambio de sprite
        spriteRenderer.color = Color.blue;
    }
    private void CreatePath(Node start, Node destiny)
    {
        _aStar.ClearData();
        List<Node> path = _aStar.FindPath(start, destiny);
        _path = path != null || path.Count >= 1 ? path : _path;
        
        if (_path.Count == 0 || _path == null) return;
        
        _path.Reverse();
        _path.RemoveAt(0);

    }

    private IEnumerator IdleState()
    {
        yield return new WaitForSeconds(_idleTime);
        _state = GhostState.Chase;
        transform.position = startNode.transform.position;
    }

    private void GoToNormalSkin()
    {
        /*
         * Esto debo de cambiarlo para que cambie entre sprites y no tanto entre colores!!
         */
        switch (id)
        {
            case 'B':
                spriteRenderer.color = Color.red;
                break;
            case 'P':
                spriteRenderer.color = Color.magenta;
                break;
            case 'I':
                spriteRenderer.color = Color.cyan;
                break;
            case 'C':
                spriteRenderer.color = Color.orange;
                break;
        }
    }

    #region GhostMovement

    private void GoToDestiny()
    {
        Vector2 target = _path.Count == 0 ? _actualNode.transform.position : _path[0].transform.position;
        
        _rigidbody.MovePosition(Vector2.MoveTowards(_rigidbody.position, target,_speed * Time.fixedDeltaTime));
        
        if (Vector2.Distance(_rigidbody.position, target) < 0.05f)
        {
            transform.position = target;
            _actualNode = _path.Count == 0 ? _actualNode : _path[0];
            DefinePath();
        }
    }

    private void IdleMovement()
    {
        _rigidbody.MovePosition(_rigidbody.position + _horizontalMov * _speed * Time.fixedDeltaTime);
    }
    private void DefinePath()
    {
        switch (_state)
        {
            case GhostState.Chase:
            {
                Node destinyNode = GetDestinyNode();
                CreatePath(_actualNode, destinyNode);
                break;
            }
            case GhostState.Frightened:
            {
                int index = Random.Range(0, (cornerNodes.Count - 1));
                Node destinyNode = cornerNodes[index];
                CreatePath(_actualNode, destinyNode);
                break;
            }
            case GhostState.Scatter:
                break;
            case GhostState.Eaten:
                CreatePath(_actualNode, startNode);
                break;
        }
    }
    
    private Node GetDestinyNode()
        {
            switch (id)
            {
                case 'B':
                    return pacman.LastNode;
                case 'P':
                {
                    char pacmanDirection = pacman.State switch
                    {
                        State.Down => 'D',
                        State.Up => 'U',
                        State.Left => 'L',
                        State.Right => 'R',
                        _ => ' '
                    };
        
                    Node destiny = pacman.LastNode;
                    Node oldDestiny = destiny;
                    for (int i = 1; i <= 4; i++)
                    {
                        foreach (NodeDirection nodeDirs in destiny.Nodes)
                        {
                            if (nodeDirs.direction == pacmanDirection)
                            {
                                destiny = nodeDirs.node;
                                break;
                            }
                        }
        
                        if (destiny == oldDestiny) break;
                        oldDestiny = destiny;
                    }
                    return destiny;
                }
                case 'I':
                {
                    Vector2 pacmanPosition = pacman.LastNode.transform.position;
                    Vector2 blinkyPosition = blinky.ActualNode.transform.position;
                    
                    Vector2 pacmanDirection = pacman.State switch
                    {
                        State.Down => Vector2.down,
                        State.Up => Vector2.up,
                        State.Left => Vector2.left,
                        State.Right => Vector2.right,
                        _ => Vector2.zero
                    };
        
                    Vector2 temp = pacmanPosition + pacmanDirection * 2;
                    Vector2 destinyPosition = blinkyPosition + (temp - blinkyPosition) * 2;
        
                    Node[] nodes = this.nodes.GetComponentsInChildren<Node>();
        
                    Node bestNode = null;
                    float bestDistance = float.MaxValue;
        
                    foreach (Node node in nodes)
                    {
                        float distance = Vector2.Distance(node.transform.position, destinyPosition);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestNode = node;
                        }
                    }
                    return bestNode;
                }
                case 'C':
                {
                    float distance = Vector2.Distance(pacman.transform.position, transform.position);
        
                    if (distance > 8f)
                    {
                        return pacman.LastNode;
                    }
                    return cornerNodes[Random.Range(0,3)];
                }
            }
        
            return null;
        }
    #endregion
}
