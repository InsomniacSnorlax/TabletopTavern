// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Mathematics;

// public abstract class FormationBase : MonoBehaviour {
//     [SerializeField] [Range(0, 1)] protected float _noise = 0;
//     [SerializeField] protected float InfantrySpread = 1.5f, CavalrySpread = 3f, MonsterSpread = 7f;
//     protected float cachedDistance = 50f;
//     public float CachedDistance => cachedDistance;
//     public abstract IEnumerable<Vector3> EvaluatePoints();
//     public abstract void CalculateUnitDepthAndWidth(float distance);
//     public abstract void CalculateUnitDepthAndWidthForSpawn(int unitCount);
//     public float3 GetNoise(float3 pos)
//     {
//         var noise = Mathf.PerlinNoise(pos.x * _noise, pos.z * _noise);
//         return new float3(noise, 0, noise);
//     }
// }