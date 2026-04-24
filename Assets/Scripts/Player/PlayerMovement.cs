using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    //private Camera camera;

    private InputAction moveAction;

    void Awake()
    {
        // Bind WASD, Arrow Keys, and Gamepad Left Stick automatically
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //camera = GetComponent<Camera>();
    }

    // Only enable inputs for the player actually controlling this object
    public override void OnNetworkSpawn()
    {
        if (IsOwner) moveAction.Enable();
        //if (IsOwner) camera.enabled = true;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) moveAction.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Read the Input System Vector2
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 move = new Vector3(input.x, 0, input.y).normalized * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}