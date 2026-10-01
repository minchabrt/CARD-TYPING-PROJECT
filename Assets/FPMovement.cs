using UnityEngine;
using UnityEngine.InputSystem;

public class FPMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;


    public float health = 100;



    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private float yVelocity = 0f;
    private bool isGrounded;

    private float xRotation = 0f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Cards")]
    public CardSystem cardsScript; // prevuci ovde GameObject koji ima CardSystem skriptu

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Move()
    {
        // Provera da li je na zemlji
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -2f; // Zalepi ga za tlo
        }

        bool cardsUp = cardsScript != null && cardsScript.isLookingDown;

        // NOVO: kretanje je blokirano dok su kartice podignute
        float x = cardsUp ? 0f : Input.GetAxis("Horizontal");
        float z = cardsUp ? 0f : Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        // Skok je dozvoljen samo ako kartice NISU podignute
        if (Input.GetButtonDown("Jump") && isGrounded && !cardsUp)
        {
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        yVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * yVelocity * Time.deltaTime);
    }
}