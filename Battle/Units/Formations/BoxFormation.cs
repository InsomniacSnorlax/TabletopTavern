using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;

public class BoxFormation : MonoBehaviour
{
    [SerializeField] private List<float3> _pointPositions = new();
    public List<float3> PointPositions => _pointPositions;
    private int2 spawnWidthAndDepth;
    public int2 SpawnWidthAndDepth => spawnWidthAndDepth;

    private Dictionary<int, float3> SE_WidthDepthSpreadDict = new();
    private Dictionary<int, int> selectedSquadEntityAndEntitiesCountDict = new();
    private float3 middleOffset = new(0.5f, 0, 0.5f);
    private float cachedDistance = 50f;
    private float _noise = 0;
    private const float BUFFER_BETWEEN_SQUADS = 4f;

    public float3 GetNoise(float3 pos)
    {
        var noise = Mathf.PerlinNoise(pos.x * _noise, pos.z * _noise);
        return new float3(noise, 0, noise);
    }
    public void GeneratePointPositions()
    {
        _pointPositions.Clear();
        float pointsOffset = 0;
        for (int i = 0; i < selectedSquadEntityAndEntitiesCountDict.Keys.Count; i++)
        {
            int SE_Index = selectedSquadEntityAndEntitiesCountDict.Keys.ElementAt(i);
            if(!SE_WidthDepthSpreadDict.ContainsKey(SE_Index))
            {
                Debug.LogWarning($"SE_WidthDepthSpreadDict does not contain key {SE_Index}");
                continue;
            }
            int _unitWidth = (int)SE_WidthDepthSpreadDict[SE_Index].x;
            int _unitDepth = (int)SE_WidthDepthSpreadDict[SE_Index].y;
            // Debug.Log($"Generating positions for squad {SE_Index} with width {_unitWidth} and depth {_unitDepth} for index {SE_Index}");
            float _spread = SE_WidthDepthSpreadDict[SE_Index].z;
            int _unitCount = selectedSquadEntityAndEntitiesCountDict[SE_Index];

            for (var x = 0; x < _unitDepth; x++)
            {
                // Determine if this is the last row and calculate offset for centering remaining units
                int unitsInRow = (x == _unitDepth - 1 && _unitCount % _unitWidth != 0) ? _unitCount % _unitWidth : _unitWidth;
                float rowOffset = (_unitWidth - unitsInRow) * 0.5f; // Calculate centering offset

                for (var z = 0; z < unitsInRow; z++)
                {
                    if (x * _unitWidth + z >= _unitCount) continue; // for moving on to the next squad

                    // Calculate the position with the centering offset for the last row
                    var pos = new float3(-x, 0, -z - rowOffset);
                    // var pos = new float3(-x - (z % 2 == 0 ? 0 : _nthOffset), 0, -z - rowOffset);

                    // Apply other adjustments
                    pos -= middleOffset;
                    pos += GetNoise(pos);
                    pos *= _spread;
                    pos += new float3(0, 0, pointsOffset);

                    _pointPositions.Add(pos);
                }
            }
            pointsOffset -= _unitWidth * _spread;
            pointsOffset -= BUFFER_BETWEEN_SQUADS;//buffer between squads
        }
        BattleManager.Instance.PositionDrawer.SetUnitPointsPositions();
    }
    public List<float3> GeneratePositionsForSquad(int2 widthAndDepth, int unitCount, float spread)
    {
        List<float3> positions = new();

        int width = widthAndDepth.x;   // X axis
        int depth = widthAndDepth.y;   // Z axis

        float halfWidth = (width - 1) * 0.5f;
        float halfDepth = (depth - 1) * 0.5f;

        int placed = 0;

        for (int row = 0; row < depth; row++)
        {
            int remaining = unitCount - placed;
            if (remaining <= 0)
                break;

            int unitsInThisRow = math.min(width, remaining);
            float rowOffset = (width - unitsInThisRow) * 0.5f;

            int visualRow = (depth - 1) - row;

            for (int col = 0; col < unitsInThisRow; col++)
            {
                float x = (col + rowOffset) - halfWidth;
                float z = visualRow - halfDepth;

                var pos = new float3(x, 0, z);
                pos += GetNoise(pos);

                positions.Add(pos * spread);
                placed++;
            }
        }

        return positions;
    }

    public void SetUnitCounts(Dictionary<int, int> _selectedSquadEntityAndEntitiesCountDict)
    {
        selectedSquadEntityAndEntitiesCountDict.Clear();
        for (int i = 0; i < BattleManager.Instance.SquadManager.TrueSquadOrder.Count; i++)
        {
            int squadId = BattleManager.Instance.SquadManager.TrueSquadOrder[i];
            if (_selectedSquadEntityAndEntitiesCountDict.ContainsKey(squadId))
            {
                int count = _selectedSquadEntityAndEntitiesCountDict[squadId];
                selectedSquadEntityAndEntitiesCountDict.Add(squadId, count);
            }
        }
    }
    public void CalculateUnitDepthAndWidthForSpawn(int unitCount, float _spread)
    {
        // Debug.Log($"CalculateUnitDepthAndWidthForSpawn: {unitCount}, {_spread}");
        selectedSquadEntityAndEntitiesCountDict.Clear();
        selectedSquadEntityAndEntitiesCountDict.Add(0, unitCount);

        int width = _spread switch
        {
            TabletopTavernConstants.InfantrySpread => unitCount > 48 ? 14 : 12,
            TabletopTavernConstants.CavalrySpread => 8,
            TabletopTavernConstants.MonsterSpread => unitCount > 8 ? 6 : 4,
            TabletopTavernConstants.ArtillerySpread => 3,
            TabletopTavernConstants.SingleUnitSpread => 1,
            _ => 0,
        };
        int depth = width > 0 ? Mathf.CeilToInt((float)unitCount / width) : 0;
        spawnWidthAndDepth = new int2(width, depth);

        SE_WidthDepthSpreadDict.Clear();
        SE_WidthDepthSpreadDict.Add(0, new float3(SpawnWidthAndDepth.x, SpawnWidthAndDepth.y, _spread));

        GeneratePointPositions();
    }
    public void CalculateUnitDepthAndWidth(float _distance) 
    {
        int selectedSquadCount = selectedSquadEntityAndEntitiesCountDict.Count;
        if(selectedSquadCount == 0) return;

        cachedDistance = _distance;

        cachedDistance -= BUFFER_BETWEEN_SQUADS * (selectedSquadCount - 1);
        cachedDistance /= selectedSquadCount;

        foreach (KeyValuePair<int, int> pair in selectedSquadEntityAndEntitiesCountDict)
        {
            int unitCount = pair.Value;
            //if distance less then spread, set width to 1, else set width to distance / spread
            float _spread = SE_WidthDepthSpreadDict[pair.Key].z;

            int width = 1; 
            // Debug.Log($"Calculating width for squad {pair.Key} with cached distance {cachedDistance} and spread {_spread}");
            
            if(cachedDistance < _spread)
            {
                width = 1;
            }
            else
            {
                width = Mathf.CeilToInt(cachedDistance / _spread);
            }

            if (_spread == TabletopTavernConstants.InfantrySpread || _spread == TabletopTavernConstants.CavalrySpread || _spread == TabletopTavernConstants.MonsterSpread)
            {
                if (width > unitCount / 2)
                    width = unitCount / 2;

                if (width < 4)
                    width = 4;
            }
            else if (_spread == TabletopTavernConstants.ArtillerySpread)
            {
                if (width > unitCount / 3)
                    width = unitCount / 3;

                if (width < 3)
                    width = 3;
            }
            else if (_spread == TabletopTavernConstants.SingleUnitSpread)
            {
                width = 1;
            }
            
            if (width > unitCount)
                width = unitCount;

            int depth = Mathf.CeilToInt((float)unitCount / (float)width);

            SE_WidthDepthSpreadDict[pair.Key] = new float3(width, depth, _spread);
            // SpawnWidthAndDepth = new int2(width, depth);
            // Debug.Log($"Calculated unit depth and width of {width}x{depth}x{_spread} for squad {pair.Key}");
        }

        GeneratePointPositions();
    }
    public void SetWidthAndDepthDict(Dictionary<int, float3> _SE_WidthDepthDict)
    {
        SE_WidthDepthSpreadDict = _SE_WidthDepthDict;
        foreach (KeyValuePair<int, float3> pair in SE_WidthDepthSpreadDict)
        {
            // Debug.Log($"SetWidthAndDepthDict: Squad {pair.Key} has width {pair.Value.x}, depth {pair.Value.y}, spread {pair.Value.z}");
        }
        GeneratePointPositions();
    }
    public int2 GetWidthAndDepth(int id)
    {
        return new int2((int)SE_WidthDepthSpreadDict[id].x, (int)SE_WidthDepthSpreadDict[id].y);
    }
}