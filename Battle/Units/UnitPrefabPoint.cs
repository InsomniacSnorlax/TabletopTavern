using UnityEngine;
using Shapes;
public class UnitPrefabPoint : MonoBehaviour
{
    [SerializeField] private Triangle selectedShape;//, spawningShape;
    public Triangle SelectedShape => selectedShape;
    [SerializeField] private int _squadID;
    public int SquadID => _squadID;
    private void FixedUpdate()
    {
        if (Physics.Raycast(new Vector3(transform.position.x, 20, transform.position.z), Vector3.down, out RaycastHit hit, 30, LayerMask.GetMask("Tile")))
        {
            transform.position = hit.point;
            // Debug.Log($"UnitPrefabPoint: Raycast hit on Tile layer.");
        }
        else
        {
            // Debug.LogError("UnitPrefabPoint: Raycast did not hit the Tile layer. Please check the layer settings.");
        }
    }
    public void SetSquadID(int squadID)
    {
        _squadID = squadID;
    }
}
