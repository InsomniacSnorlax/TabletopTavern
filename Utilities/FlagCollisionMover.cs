using UnityEngine;

public class MaintainDistance : MonoBehaviour
{
    public float minDistance = 1f;  // Set to 1m
    public float repulsionForce = 10f;  // Strength of push-away; adjust as needed
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.enabled = false;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider.enabled = true;
        
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("SquadFlag"))  // Or check if other has a specific component
        {
            Debug.Log($"MaintainDistance: Repelling from {other.name}");
            Vector3 direction = transform.position - other.transform.position;
            float currentDistance = direction.magnitude;

            if (currentDistance < minDistance && currentDistance > 0.001f)  // Avoid divide-by-zero
            {
                Vector3 repelDirection = direction.normalized;
                rb.AddForce(repelDirection * repulsionForce);  // Push away
                // Optionally: other.GetComponent<Rigidbody>().AddForce(-repelDirection * repulsionForce);  // Mutual push
            }
        }
    }
}