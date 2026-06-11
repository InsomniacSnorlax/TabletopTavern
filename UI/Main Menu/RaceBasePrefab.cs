using UnityEngine;

public class RaceBasePrefab : MonoBehaviour
{
    [SerializeField] private GameObject[] activeParts;
    public void SetUp(bool isCollected, Material undiscoveredMaterial)
    {
        if (!isCollected)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = undiscoveredMaterial;
            }
            foreach (GameObject part in activeParts)
            {
                part.SetActive(false);
            }
        }
    }
}
