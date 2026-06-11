using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private Transform _target;
    void Update()
    {
        _target.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }
}
