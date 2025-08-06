using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 5f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;

    public float CurrentMoveSpeed { get; private set; }

    private void Awake()
    {
        // Singleton pattern (optional), or protect from duplicate instances
        if (FindObjectsOfType<PlayerMovement>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        characterController = GetComponent<CharacterController>();
        CurrentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(moveX, 0, moveZ).normalized;
        moveDirection = transform.TransformDirection(input) * CurrentMoveSpeed;

        characterController.SimpleMove(moveDirection);
    }

    public void ApplySpeedModifier(float multiplier)
    {
        CurrentMoveSpeed = baseMoveSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        CurrentMoveSpeed = baseMoveSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        baseMoveSpeed = newSpeed;
        CurrentMoveSpeed = newSpeed;
    }
}
