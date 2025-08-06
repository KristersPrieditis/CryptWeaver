using UnityEngine;

public class FlameWobble : MonoBehaviour
{
    public float swayAmount = 1f;        // degrees of sway
    public float swaySpeed = 1f;         // how fast it moves
    public float bobAmount = 0.005f;      // vertical motion
    public float bobSpeed = 3f;          // how fast it bobs 

    private Vector3 initialPos;
    private Quaternion initialRot;

    void Start()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }

    void Update()
    {
        // Wobble side to side (rotate)
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.localRotation = initialRot * Quaternion.Euler(0, 0, sway);

        // Bob up and down (position)
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.localPosition = initialPos + new Vector3(0, bob, 0);
    }
}
