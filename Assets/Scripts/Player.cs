using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;

enum State
{
    Up, Down, Left, Right
}

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Collider2D directionCollider;
    private Rigidbody2D rg;
    private State state;
    private Node node;
    private Dictionary<char, State> charToState = new Dictionary<char, State>
    {
      {'L', State.Left}, {'R', State.Right}, {'D', State.Down}, {'U', State.Up}
    };
    private bool isInsideNode;

    /*
        ==========================
        ||  Eventos de Unity    ||
        ==========================
    */
    void Start()
    {
        rg = GetComponent<Rigidbody2D>();

        state = State.Left;
        isInsideNode = false;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        PlayerMovement(state);

        if (node != null)
        {
            if (isCompletelyInside(node.GetComponent<Collider2D>()))
            {
                isInsideNode = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Node"))
        {
            node = collision.GetComponent<Node>();
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
            node = null;
            isInsideNode = false;
        }
    }

    /*
        ======================================================
        ||  Métodos y funciones definidas por el usuario    ||
        ======================================================
    */
    private bool isCompletelyInside(Collider2D node)
    {
        Bounds nodeBounds = node.bounds;
        Bounds playerBounds = directionCollider.bounds;

        return nodeBounds.Contains(playerBounds.min) && nodeBounds.Contains(playerBounds.max);
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

        foreach (NodeDirection node in this.node.Nodes)
        {
            if (charToState[node.direction] == stateToChange)
            {
                isDirectionAvailable = true;

                switch (stateToChange)
                {
                    case State.Down when isInsideNode:
                    case State.Up when isInsideNode:
                        transform.position = new Vector3(this.node.transform.position.x, transform.position.y, 0);
                        break;
                    case State.Left when isInsideNode:
                    case State.Right when isInsideNode:
                        transform.position = new Vector3(transform.position.x, this.node.transform.position.y, 0);
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
        Move(Vector2.left);

        if (Keyboard.current.dKey.isPressed)
        {
            state = State.Right;
        }
        else if (isInsideNode)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                state = ChangeDirection(State.Up, State.Left);
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                state = ChangeDirection(State.Down, State.Left);
            }
        }
    }

    private void MoveToRight()
    {
        Move(Vector2.right);

        if (Keyboard.current.aKey.isPressed)
        {
            state = State.Left;
        }
        else if (isInsideNode)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                state = ChangeDirection(State.Up, State.Right);
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                state = ChangeDirection(State.Down, State.Right);
            }
        }
    }

    private void MoveToUp()
    {
        Move(Vector2.up);

        if (Keyboard.current.sKey.isPressed)
        {
            state = State.Down;
        }
        else if (isInsideNode)
        {
            if (Keyboard.current.dKey.isPressed)
            {
                state = ChangeDirection(State.Right, State.Up);
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                state = ChangeDirection(State.Left, State.Up);
            }
        }
    }

    private void MoveToDown()
    {
        Move(Vector2.down);

        if (Keyboard.current.wKey.isPressed)
        {
            state = State.Up;
        }
        else if (isInsideNode)
        {
            if (Keyboard.current.dKey.isPressed)
            {
                state = ChangeDirection(State.Right, State.Down);
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                state = ChangeDirection(State.Left, State.Down);
            }
        }
    }


    private void Move(Vector2 direction)
    {
        rg.MovePosition(rg.position + direction * speed * Time.fixedDeltaTime);
    }
    #endregion PlayerMovement
}
