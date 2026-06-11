using UnityEngine;

namespace TJ
{
public class CharacterAttachmentRandomizer : MonoBehaviour
{
    void Start()
    {
        // Get all the children of the object this script is attached to
        Transform[] children = GetComponentsInChildren<Transform>();
        int randomIndex = Random.Range(0, children.Length);
        //set a random one active   
        foreach (Transform child in children)
        {
            if (child == children[randomIndex])
            {
                child.gameObject.SetActive(true);
            }
        }   
        Debug.Log($"Randomized attachment: {children[randomIndex].name}");
    }

}
}
