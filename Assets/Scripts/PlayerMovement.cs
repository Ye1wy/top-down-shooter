using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        moveInput = Vector2.zero;
        if (kb.wKey.isPressed) moveInput.y += 1;
        if (kb.sKey.isPressed) moveInput.y -= 1;
        if (kb.dKey.isPressed) moveInput.x += 1;
        if (kb.aKey.isPressed) moveInput.x -= 1;

        moveInput = moveInput.normalized;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void FixedUpdate()
    {

    }
}
