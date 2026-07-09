using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 *
 *  CUANDO LLEGUEN AL NODO QUE ESTÁ AL LADO DEL TELEPORT, QUE SIGAN DE LARGO, QUE SE TELETRANSPORTEN
 *  EN CASO DE QUE ASÍ SEA NECESARIO
 *  LOS FANTASMAS DEBEN DE SALIR DE ACUERDO A CUANTOS PUNTOS TENGA PACMAN
 */

public class Ghost : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private char id; //B: Blinky, P: Pinky, I: Inky, C: Clyde
    [SerializeField] private float idleTime;
    [SerializeField] private Node firstNode;
    [SerializeField] private Node destinyNode;
    [SerializeField] private Player pacman;
    [SerializeField] private Collider2D collider2D;
    [SerializeField] private GameObject nodes;
    [SerializeField] private Ghost blinky = null;
    [SerializeField] private List<Node> cornerNodes;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D _rigidbody;
    
    private List<Node> _path;
    private AStar _aStar;
    private Node _actualNode;
    private Vector2 _horizontalMov;
    private bool _idleState;

    #region UnityMethods
    void Start()
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
        _aStar = new AStar();

        _rigidbody = GetComponent<Rigidbody2D>();
        
        _actualNode = firstNode;
        CreatePath(firstNode, destinyNode);

        _horizontalMov = Vector2.left;
        _idleState = true;

        StartCoroutine(Timer(idleTime));
    }
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (_idleState)
        {
            IdleState();
            return;
        }
        GoToDestiny();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            if (_idleState) _horizontalMov *= -1;
        }
    }

    #endregion

    public Node ActualNode => _actualNode;
    
    private IEnumerator<WaitForSeconds> Timer(float time)
    {
        yield return new WaitForSeconds(time);
        _idleState = false;
        transform.position = firstNode.transform.position;
    }
    #region GhostMovement
    private void CreatePath(Node start, Node finish)
    {
        if (start == finish)
            return;
        
        _aStar.ClearData();
        _path = _aStar.FindPath(start, finish);
        
        if (_path == null || _path.Count == 0)
            return;
        
        _path.Reverse();
        _path.Remove(start);
    }

    private void GoToDestiny()
    {
        Vector2 target = _path[0].transform.position;
        
        _rigidbody.MovePosition(Vector2.MoveTowards(_rigidbody.position, target,speed * Time.fixedDeltaTime));
        
        if (Vector2.Distance(_rigidbody.position, target) < 0.05f)
        {
            transform.position = _path[0].transform.position;
            _actualNode = _path[0];
            CreatePath(_actualNode, GetDestinyNode());
        }
    }

    private void IdleState()
    {
        _rigidbody.MovePosition(_rigidbody.position + _horizontalMov * speed * Time.fixedDeltaTime);
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
