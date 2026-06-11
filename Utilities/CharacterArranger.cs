using UnityEngine;

public class CharacterArranger : MonoBehaviour
{
    [ContextMenu("Arrange Characters")]
    public void ArrangeCharacters()
    {
        //for each child of the parent object offset the postiion by 2 unit in the x, loop around the z every 10 units
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.position = new Vector3((i % 10)*2, 0, i / 10);
        }
    }
}
