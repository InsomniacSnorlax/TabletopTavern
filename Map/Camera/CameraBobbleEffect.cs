using UnityEngine;

public class CameraBobbleEffect : MonoBehaviour
{
    [SerializeField] private float positionAmplitude = 0.05f; // Max position offset (in units, e.g., 0.05 for subtle)
    [SerializeField] private float positionFrequency = 1f; // Speed of position bobble (higher = faster)
    [SerializeField] private float rotationAmplitude = 0.5f; // Max rotation offset (in degrees, e.g., 0.5 for subtle)
    [SerializeField] private float rotationFrequency = 0.5f; // Speed of rotation bobble
    [SerializeField] private bool enableRotation = true; // Toggle rotation effect
    [SerializeField] private Vector3 positionAxes = Vector3.one; // Axes to apply position bobble (1 = enabled, 0 = disabled)
    [SerializeField] private Vector3 rotationAxes = Vector3.one; // Axes to apply rotation bobble

    private Vector3 initialLocalPosition; // Camera's starting local position
    private Quaternion initialRotation; // Camera's starting rotation
    private float noiseOffsetX; // Random offset for Perlin noise
    private float noiseOffsetY;
    private float noiseOffsetZ;
    bool isActive = false; // Flag to check if the effect is active

    public void TurnOn()
    {
        if (isActive) return; // Prevent re-initialization if already active
        // Store initial position and rotation
        initialLocalPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // Randomize noise offsets to avoid synchronized bobbing
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
        noiseOffsetZ = Random.Range(0f, 100f);
        isActive = true; // Set active flag to true
    }
    public void TurnOff()
    {
        // Reset position and rotation to initial values
        isActive = false; // Set active flag to false
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialRotation;
    }

    void Update()
    {
        if (!isActive) return; // Skip update if not active
        
        // Calculate Perlin noise for each axis
        float time = Time.time;
        float noiseX = Mathf.PerlinNoise(time * positionFrequency + noiseOffsetX, 0) * 2 - 1; // -1 to 1
        float noiseY = Mathf.PerlinNoise(time * positionFrequency + noiseOffsetY, 0) * 2 - 1;
        float noiseZ = Mathf.PerlinNoise(time * positionFrequency + noiseOffsetZ, 0) * 2 - 1;

        // Apply position bobble
        Vector3 positionOffset = new Vector3(
            noiseX * positionAmplitude * positionAxes.x,
            noiseY * positionAmplitude * positionAxes.y,
            noiseZ * positionAmplitude * positionAxes.z
        );
        transform.localPosition = initialLocalPosition + positionOffset;

        // Apply rotation bobble (if enabled)
        if (enableRotation)
        {
            float rotNoiseX = Mathf.PerlinNoise(time * rotationFrequency + noiseOffsetX + 100, 0) * 2 - 1;
            float rotNoiseY = Mathf.PerlinNoise(time * rotationFrequency + noiseOffsetY + 100, 0) * 2 - 1;
            float rotNoiseZ = Mathf.PerlinNoise(time * rotationFrequency + noiseOffsetZ + 100, 0) * 2 - 1;

            Vector3 rotationOffset = new Vector3(
                rotNoiseX * rotationAmplitude * rotationAxes.x,
                rotNoiseY * rotationAmplitude * rotationAxes.y,
                rotNoiseZ * rotationAmplitude * rotationAxes.z
            );
            transform.localRotation = initialRotation * Quaternion.Euler(rotationOffset);
        }
    }
}