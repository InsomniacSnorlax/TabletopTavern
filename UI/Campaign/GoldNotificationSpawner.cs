using UnityEngine;

namespace TJ
{
public class GoldNotificationSpawner : MonoBehaviour
{
    [SerializeField] GoldNotification prefab;
    [SerializeField] Transform spawnPoint;
    private int _previousGold;
    public void LoadGold(int currentGoldAmount)
    {
        _previousGold = currentGoldAmount;
    }

    public void DisplayGoldNotification(int newTotal, string localizedString)
    {
        int delta = newTotal - _previousGold;
        _previousGold = newTotal;

        GoldNotification notification = Instantiate(prefab, spawnPoint);
        notification.Initialize(delta, localizedString);
    }
}
}
