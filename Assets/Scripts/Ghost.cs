using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] private SpriteRenderer spriteRenderer, eyesSpriteRender;
    [SerializeField] private List<Node> cornerNodes;
    [SerializeField] private List<Sprite> eyes;

    public Animator animator;
    public float speed;
    
    private Rigidbody2D _rigidbody;
    private Collider2D _physicCollider;
        
    private AStar _aStar = new AStar();
    private List<Node> _path = new();
    private GhostState _state = GhostState.Idle;
    private Node _actualNode;
    private Vector2 _horizontalMov;
    private float _idleTime;
    private const float NormalSpeed = 3f;
    private Dictionary<Vector2, int> directionToEyes= new Dictionary<Vector2, int>
    {
        {Vector2.right, 0}, {Vector2.left, 1}, {Vector2.down, 2}, {Vector2.up, 3}, {Vector2.zero, 0} 
    };

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
        speed = NormalSpeed;
        
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
                speed = NormalSpeed;
                spriteRenderer.gameObject.SetActive(true);
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

            spriteRenderer.gameObject.SetActive(false);
            eyesSpriteRender.gameObject.SetActive(true);
            speed *= 4;
            _physicCollider.enabled = false;
            StartCoroutine(PauseTime());
        }
    }

    #endregion

    public Node ActualNode => _actualNode;
    public GhostState ActualState => _state;
    public float Speed => NormalSpeed;

    public void ResetState()
    {
        StopAllCoroutines();
        transform.position = startNode.transform.position;
        _horizontalMov = Vector2.left;
        _state = GhostState.Idle;
        speed = NormalSpeed;
        GoToNormalSkin();
        _actualNode = startNode;
        CreatePath(startNode, destinyNode);
        StartCoroutine(IdleState());
    }

    public void SetFrightenedMode(bool value)
    {
        if (!value)
        {
            _state = GhostState.Chase;
            animator.SetBool("superPacman", false);
            GoToNormalSkin();
            StartCoroutine(ChaseState());
            eyesSpriteRender.gameObject.SetActive(true);
            return;
        }

        _state = GhostState.Frightened;
        StopAllCoroutines();

        animator.SetBool("superPacman", true);
        eyesSpriteRender.gameObject.SetActive(false);
        spriteRenderer.color = Color.white;
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
    #region Coroutines

    private IEnumerator PauseTime()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
    }
    private IEnumerator ChaseState()
    {
        yield return new WaitForSeconds(4f);
        _state = GhostState.Scatter;
        StartCoroutine(ScatterState());
    }

    private IEnumerator ScatterState()
    {
        yield return new WaitForSeconds(4f);
        _state = GhostState.Chase;
        StartCoroutine(ChaseState());
    }

    private IEnumerator IdleState()
    {
        yield return new WaitForSeconds(_idleTime);
        _state = GhostState.Chase;
        transform.position = startNode.transform.position;
        StartCoroutine(ChaseState());
    }
    #endregion

    private void GoToNormalSkin()
    {
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
        if (eyesSpriteRender.enabled)
            eyesSpriteRender.sprite = eyes[directionToEyes[GetDirection(_path.Count == 0 ? _actualNode : _path[0])]];
        
        Vector2 target = _path.Count == 0 ? _actualNode.transform.position : _path[0].transform.position;
        Vector2 newPosition = Vector2.MoveTowards(_rigidbody.position, target, speed * Time.fixedDeltaTime);
        
        _rigidbody.MovePosition(newPosition);
        
        if (Vector2.Distance(_rigidbody.position, target) < 0.05f)
        {
            transform.position = target;
            _actualNode = _path.Count == 0 ? _actualNode : _path[0];
            DefinePath();
        }
    }

    private void IdleMovement()
    {
        _rigidbody.MovePosition(_rigidbody.position + _horizontalMov * speed * Time.fixedDeltaTime);
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
            {
                int index = Random.Range(0, (cornerNodes.Count - 1));
                Node destiny = cornerNodes[index];
                CreatePath(_actualNode, destinyNode);
                break;
            }
            case GhostState.Eaten:
                CreatePath(_actualNode, startNode);
                break;
        }
    }

    private Vector2 GetDirection(Node destinyNode)
    {
        Vector2 direction = Vector2.zero;
        foreach (NodeDirection node in _actualNode.Nodes)
        {
            if (node.node.ID == destinyNode.ID)
            {
                direction = node.direction switch
                {
                    'U' => Vector2.up,
                    'D' => Vector2.down,
                    'L' => Vector2.left,
                    'R' => Vector2.right,
                    _ => Vector2.zero
                    
                };
            }
        }

        return direction;
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
