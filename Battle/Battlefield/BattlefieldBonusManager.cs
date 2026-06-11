using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BattlefieldBonusManager : MonoBehaviour
{
    [SerializeField] private List<AssetReferenceGameObject> possibleBonuses;
    [SerializeField] private int customBattleCount = 1;
    public int CustomBattleCount => customBattleCount;
    private List<GameObject> bonusObjects = new();

    public async Task SetUp(int _seed, SpawnBox _battlefieldSize, int? forcedCount = null)
    {
        for (int i = 0; i < bonusObjects.Count; i++)
        {
            Addressables.ReleaseInstance(bonusObjects[i]);
        }
        bonusObjects.Clear();

        System.Random random = new(_seed);
        int bonusCount = forcedCount ?? random.Next(TabletopTavernConstants.BATTLEFIELD_BONUSES_RANGE.x, TabletopTavernConstants.BATTLEFIELD_BONUSES_RANGE.y);
        for (int i = 0; i < bonusCount; i++)
        {
            AssetReferenceGameObject bonusRef = possibleBonuses[random.Next(0, possibleBonuses.Count)];
            Vector3 position = new Vector3(
                random.Next((int)_battlefieldSize.min.x, (int)_battlefieldSize.max.x) / 2,
                0,
                random.Next((int)_battlefieldSize.min.z, (int)_battlefieldSize.max.z) / 2
            );
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(bonusRef, position, Quaternion.identity);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                bonusObjects.Add(handle.Result);
            }
        }
    }

    public void CleanUp()
    {
        for (int i = 0; i < bonusObjects.Count; i++)
            Addressables.ReleaseInstance(bonusObjects[i]);
        bonusObjects.Clear();
    }
}
