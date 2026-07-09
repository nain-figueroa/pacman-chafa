using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum State
{
    Up, Down, Left, Right, Idle
}

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Collider2D directionCollider;
    [SerializeField] private GameObject skin;
    
    private Rigidbody2D _rigidbody;
    private Transform _spriteTransform;
    private SpriteRenderer _spriteRenderer;
    private State _state;
    private Node _node, _lastNode;
    private Dictionary<char, State> _charToState = new Dictionary<char, State>
    {
      {'L', State.Left}, {'R', State.Right}, {'D', State.Down}, {'U', State.Up}
    };
    private bool _isInsideNode;

    /*
        ==========================
        ||  Eventos de Unity    ||
        ==========================
    */
    void Start()
    {
        Time.timeScale = 1f;
        _rigidbody = GetComponent<Rigidbody2D>();

        _state = State.Left;
        _isInsideNode = false;
        _spriteRenderer = skin.GetComponent<SpriteRenderer>();
        _spriteTransform = skin.GetComponent<Transform>();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        PlayerMovement(_state);

        if (_node != null)
        {
            if (isCompletelyInside(_node.GetComponent<Collider2D>()))
            {
                _isInsideNode = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Node"))
        {
            _node = collision.GetComponent<Node>();
            _lastNode = _node;
        }
        else if (collision.CompareTag("Teleport"))
        {
            Teleport teleport = collision.GetComponent<Teleport>();
            transform.position = teleport.OtherTelepor.transform.position;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Node"))
        {
            _node = null;
            _isInsideNode = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ghost"))
        {
            Time.timeScale = 0;
            Timer(2f);
            SceneManager.LoadScene("GameOver");
        }
    }

    /*
        ======================================================
        ||  Métodos y funciones definidas por el usuario    ||
        ======================================================
    */

    public Node LastNode => _lastNode;
    public State State => _state;
    private bool isCompletelyInside(Collider2D node)
    {
        Bounds nodeBounds = node.bounds;
        Bounds playerBounds = directionCollider.bounds;

        return nodeBounds.Contains(playerBounds.min) && nodeBounds.Contains(playerBounds.max);
    }

    private IEnumerator<WaitForSeconds> Timer(float time)
    {
        yield return new WaitForSeconds(time);
    }

    #region PlayerMovement
    private void PlayerMovement(State state)
    {
        switch (state)
        {
            case State.Down:
                MoveToDown();
                break;
            case State.Up:
                MoveToUp();
                break;
            case State.Left:
                MoveToLeft();
                break;
            case State.Right:
                MoveToRight();
                break;
        }
    }
    /*
        **Cuando esté avanzando en una dirección siempre podrá avanzar en la dirección contraria**
        **Para cambios drásticos (por ejemplo, de izquierda hacia abajo) se necesita validar si está en un nodo o no**
    */
    private State ChangeDirection(State stateToChange, State actualState)
    {
        bool isDirectionAvailable = false;

        foreach (NodeDirection node in this._node.Nodes)
        {
            if (_charToState[node.direction] == stateToChange)
            {
                isDirectionAvailable = true;

                switch (stateToChange)
                {
                    case State.Down when _isInsideNode:
                    case State.Up when _isInsideNode:
                        transform.position = new Vector3(this._node.transform.position.x, transform.position.y, 0);
                        break;
                    case State.Left when _isInsideNode:
                    case State.Right when _isInsideNode:
                        transform.position = new Vector3(transform.position.x, this._node.transform.position.y, 0);
                        break;
                }
                break;
            }
        }

        if (!isDirectionAvailable) return actualState;

        return stateToChange;
    }
    private void MoveToLeft()
    {
        _spriteRenderer.flipX = true;
        _spriteTransform.localRotation = Quaternion.Euler(0,0,0);
        Move(Vector2.left);

        if (Keyboard.current.dKey.isPressed)
        {
            _state = State.Right;
        }
        else if (_isInsideNode)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                _state = ChangeDirection(State.Up, State.Left);
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                _state = ChangeDirection(State.Down, State.Left);
            }
        }
    }

    private void MoveToRight()
    {
        _spriteRenderer.flipX = false;
        _spriteTransform.localRotation = Quaternion.Euler(0,0,0);
        Move(Vector2.right);

        if (Keyboard.current.aKey.isPressed)
        {
            _state = State.Left;
        }
        else if (_isInsideNode)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                _state = ChangeDirection(State.Up, State.Right);
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                _state = ChangeDirection(State.Down, State.Right);
            }
        }
    }

    private void MoveToUp()
    {
        _spriteRenderer.flipX = false;
        _spriteTransform.localRotation = Quaternion.Euler(0,0,90);
        Move(Vector2.up);

        if (Keyboard.current.sKey.isPressed)
        {
            _state = State.Down;
        }
        else if (_isInsideNode)
        {
            if (Keyboard.current.dKey.isPressed)
            {
                _state = ChangeDirection(State.Right, State.Up);
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                _state = ChangeDirection(State.Left, State.Up);
            }
        }
    }

    private void MoveToDown()
    {
        _spriteRenderer.flipX = false;
        _spriteTransform.localRotation = Quaternion.Euler(0,0,270);
        Move(Vector2.down);

        if (Keyboard.current.wKey.isPressed)
        {
            _state = State.Up;
        }
        else if (_isInsideNode)
        {
            if (Keyboard.current.dKey.isPressed)
            {
                _state = ChangeDirection(State.Right, State.Down);
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                _state = ChangeDirection(State.Left, State.Down);
            }
        }
    }


    private void Move(Vector2 direction)
    {
        _rigidbody.MovePosition(_rigidbody.position + direction * speed * Time.fixedDeltaTime);
    }
    #endregion PlayerMovement
}
