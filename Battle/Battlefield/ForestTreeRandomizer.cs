using UnityEngine;
public class ForestTreeRandomizer : MonoBehaviour
{
    public void RandomizeTree()
    {
        float scaleModifier = Random.Range(0.8f, 1.2f);
        transform.localScale = new Vector3(
            transform.localScale.x * scaleModifier,
            transform.localScale.y * scaleModifier,
            transform.localScale.z * scaleModifier
        );
        float rotationY = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            rotationY,
            transform.rotation.eulerAngles.z
        );
    }
}
