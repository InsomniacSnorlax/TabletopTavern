// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace TJ.Spells
// {
// public class ReinforcementsSpell : SpellName
// {
//     [SerializeField] private int maxTroops;
//     // [SerializeField] private List<Troop> troops = new();
//     // private Dictionary<Troop, Transform> troopPointsDict = new();
//     // [SerializeField] private Transform[] troopPoints;
//     // [SerializeField] private TowerSO reinforcementTower;
//     [SerializeField] private float disengageTroopsAfterTime = 5f;

//     public void Update()
//     {
//         // foreach(Troop troop in troops) {
//         //     troop.HandleTroopStateMachine();
//         // }
//     }
//     public override void LoadSpell(Vector3 _spellCastPoint,int spellId)
//     {
//         base.LoadSpell(_spellCastPoint, spellId);

//         // GameManager.Instance.ObjectPoolingManager.CreateTroopPool(reinforcementTower, 0);

//         void CreateTroopPoints(){

//             troopPoints = new Transform[maxTroops];
//             for (int i = 0; i < maxTroops; i++) {
//                 GameObject child = new ($"TroopPoint{i}");
//                 child.transform.SetParent(transform);
//                 child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//                 troopPoints[i] = child.transform;
//             }

//             if(maxTroops == 1) {
//                 troopPoints[0].localPosition = Vector3.zero;
//             } else if(maxTroops == 2) {
//                 //get offsets for troop points, should be 0.25f away from rally point to the left and right
//                 Vector3[] offsets = new Vector3[2];
//                 offsets[0] = new Vector3(-0.25f, 0, 0);
//                 offsets[1] = new Vector3(0.25f, 0, 0);

//                 //set troop points to offsets
//                 for (int i = 0; i < troopPoints.Length; i++) {
//                     troopPoints[i].localPosition = offsets[i];
//                 }
//             } else if(maxTroops == 3){
//                 //get offsets for troop points, should be 0.25f away from rally point in a triangle
//                 Vector3[] offsets = new Vector3[3];
//                 offsets[0] = new Vector3(0, 0, 0.15f);
//                 offsets[1] = new Vector3(-0.15f, 0, -0.075f);
//                 offsets[2] = new Vector3(0.15f, 0, -0.075f);

//                 //set troop points to offsets
//                 for (int i = 0; i < troopPoints.Length; i++) {
//                     troopPoints[i].localPosition = offsets[i];
//                 }
//             }
//         }

//         void DeployAllTroops() {

//             void SetTroopPoint(Troop troop) {
//                 foreach(Transform troopPoint in troopPoints) {
//                     if(!troopPointsDict.ContainsValue(troopPoint)) {
//                         troopPointsDict.Add(troop, troopPoint);
//                         return;
//                     }
//                 }
//                 Debug.Log($"No troop points available");
//             }

//             for (int i = 0; i < maxTroops; i++) {
//                 Troop troop = GameManager.Instance.ObjectPoolingManager.GetTroop(reinforcementTower, 0);
//                 troop.transform.position = new (transform.position.x, 0, transform.position.z);
//                 troop.gameObject.SetActive(true);
//                 troops.Add(troop);
//                 SetTroopPoint(troop);
//                 troop.SpawnAtPoint(troopPointsDict[troop]);
//                 GameManager.Instance.ObjectPoolingManager.SpawnDustCloud(troopPointsDict[troop].position);
//             }
//         }

//         CreateTroopPoints();
//         DeployAllTroops();
//         Invoke(nameof(DustAllTroops), disengageTroopsAfterTime);
//         Invoke(nameof(RemoveSpell), spellLifetime);

//         Runtime.Instance.IAudioRequester.PlaySFX("spawn");
//     }
//     public override void RemoveSpell()
//     {
//         base.RemoveSpell();
//     }
//     private void DustAllTroops()
//     {
//         foreach(Troop troop in troops) {
//             troop.HandleDisengage();
//         }
//     }
// }
// }
