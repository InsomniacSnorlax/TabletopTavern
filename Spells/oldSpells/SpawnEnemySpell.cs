// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Threading.Tasks;

// namespace TJ.Spells
// {
// public class SpawnEnemySpell : SpellName
// {
//     [SerializeField] private int enemyToSpawnId;
//     [SerializeField] private int numberOfEnemiesToSpawn = 4;
//     public override void LoadSpell(Vector3 _spellCastPoint, int _spellId)
//     {
//         base.LoadSpell(_spellCastPoint, _spellId);
//         PauseForSpawn();
//     }
//     public async void PauseForSpawn()
//     {
//         await Task.Delay(1000);
//         // EnemySpawnData enemySpawnData = GameManager.Instance.DataManager.GetEnemySpawnData(GameManager.Instance.SeasonManager.SeasonPathIndex);
        
//         //create new transform
//         Transform transformPoint = new GameObject().transform;

//         if (this == null) {
//             // Debug.LogError("Failed to cast spell");
//             return;
//         }
//         transformPoint.position = transform.position;

//         //I want to spawn 8 enemies at a time in a circle around the transform all 1 unit away in all directions
//         // for(int i = 0; i < numberOfEnemiesToSpawn; i++){
//         //     transformPoint.position += new Vector3(Mathf.Cos(i * 45 * Mathf.Deg2Rad), 0, Mathf.Sin(i * 45 * Mathf.Deg2Rad))*0.1f;
//         //     enemySpawnData.spawnPoint = transformPoint;

//         //     //this is the issue right here
//         //     Enemy spawnedEnemy = GameManager.Instance.EnemyManager.SpawnEnemy(EnemyData.GetEnemyStats(enemyToSpawnId), enemySpawnData, 999, 999, true);
//         //     spawnedEnemy.OverrideEnemyProgress(EnemyCasting);

//         //     await Task.Delay(100);
//         // }
//         Destroy(transformPoint.gameObject);
//         // Invoke(nameof(RemoveSpell), 1);
//     }
//     public override void RemoveSpell()
//     {
//         Destroy(gameObject);
//     }
// }
// }
