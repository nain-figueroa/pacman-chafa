using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

enum State
{
    Up, Down, Left, Right
}

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rg;
    private State state;
    void Start()
    {
        rg = GetComponent<Rigidbody2D>();

        state = State.Left;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        PlayerMovement(state);
    }

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

    private void MoveToLeft()
    {
        Move(Vector2.left);

        if (Keyboard.current.dKey.isPressed)
        {
            state = State.Right;
        }
        else if (Keyboard.current.wKey.isPressed)
        {
            state = State.Up;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            state = State.Down;
        }
    }

    private void MoveToRight()
    {
        Move(Vector2.right);

        if (Keyboard.current.aKey.isPressed)
        {
            state = State.Left;
        }
        else if (Keyboard.current.wKey.isPressed)
        {
            state = State.Up;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            state = State.Down;
        }
    }

    private void MoveToUp()
    {
        Move(Vector2.up);

        if (Keyboard.current.sKey.isPressed)
        {
            state = State.Down;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            state = State.Right;
        }
        else if (Keyboard.current.aKey.isPressed)
        {
            state = State.Left;
        }
    }

    private void MoveToDown()
    {
        Move(Vector2.down);

        if (Keyboard.current.wKey.isPressed)
        {
            state = State.Up;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            state = State.Right;
        }
        else if (Keyboard.current.aKey.isPressed)
        {
            state = State.Left;
        }
    }


    private void Move(Vector2 direction)
    {
        rg.MovePosition(rg.position + direction * speed * Time.fixedDeltaTime);
    }
}
