using UnityEngine;
using Unity.Entities;

public class CameraReferenceSystem : MonoBehaviour
{
    public static Camera MainCamera;

    void Start()
    {
        MainCamera = Camera.main;
    }
}
