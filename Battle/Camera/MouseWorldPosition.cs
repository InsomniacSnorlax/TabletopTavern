using UnityEngine;

public class MouseWorldPosition : MonoBehaviour 
{
    public static MouseWorldPosition Instance { get; private set; }
    [SerializeField] private LayerMask layerToHit;
    // [SerializeField] private Transform debugObject;
    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetWorldPosition()
    {
        Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(mouseCameraRay, out hit, Mathf.Infinity, layerToHit)) {
            // debugObject.position = hit.point;
            return hit.point;
        } else {
            return Vector3.zero;
        }
    }
}