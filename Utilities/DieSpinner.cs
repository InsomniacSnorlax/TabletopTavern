using System.Collections;
using UnityEngine;

public class DieSpinner : MonoBehaviour
{
    [SerializeField] private float _spinSpeed = 200f, idleSpinSpeed = 20f, currentSpeed;
    [SerializeField] private Vector3 _spinAxis = Vector3.up, _spinDirection = Vector3.up;
    [SerializeField] private Transform _throwPoint;

    public void PreSpinDie()
    {
        currentSpeed = _spinSpeed;
        transform.position = _throwPoint.position;
    }
    public void ResetDie()
    {
        currentSpeed = idleSpinSpeed;
        transform.position = _throwPoint.position;
    }
    void Update()
    {
        transform.Rotate(_spinAxis, currentSpeed * Time.deltaTime);
        transform.Rotate(_spinDirection, currentSpeed * Time.deltaTime);
    }
}
