using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    public Light fireLight;
    public float baseIntensity = 2f;
    public float flickerAmount = 0.5f;
    public float flickerSpeed = 2f;

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        fireLight.intensity = baseIntensity + noise * flickerAmount;
    }
}
