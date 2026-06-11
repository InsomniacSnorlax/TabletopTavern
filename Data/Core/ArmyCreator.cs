using Memori.SaveData;
using UnityEngine;
using System.Collections.Generic;

namespace TJ
{
    public static class ArmyCreator
    {
        [System.Serializable] private struct UnitsToGetByTier
        {
            public int unitTier;
            public int unitsToGet;
        }
        public static SquadToLoad[] GenerateTownGarrison(TownSize _townSize, Race _townRace, int _seed, List<UnitTier> unitsPool, bool difficultyImperator, int _bookNumber = 1)
        {
            // Garrisons don't field cavalry or outriders — filter them out before picking units
            unitsPool = unitsPool.FindAll(u =>
                TabletopTavernData.Instance.GetUnitSizeFromUnitName(u.unitName) != UnitSize.Cavalry
            );

            List<UnitsToGetByTier> unitsToGetByTier = new();

            switch (_bookNumber)
            {
                case 1:
                {
                    if (!difficultyImperator)
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 4 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 4 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                        }
                    }
                    else
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 3 });
                        }
                    }
                    break;
                }

                case 2:
                {
                    if (!difficultyImperator)
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 5 });
                        }
                    }
                    else
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 1 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 7 });
                        }
                    }
                    break;
                }

                case 3:
                {
                    if (!difficultyImperator)
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 1 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 6 });
                        }
                    }
                    else
                    {
                        if (_townSize == TownSize.Village) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 3 });
                        } else if (_townSize == TownSize.Castle) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 1 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 5 });
                        } else if (_townSize == TownSize.City) {
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                            unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 7 });
                        }
                    }
                    break;
                }
            }

            return CreateArmyFromUnitsByTier(unitsToGetByTier, unitsPool, _seed);
        }
        private static SquadToLoad[] CreateArmyFromUnitsByTier(List<UnitsToGetByTier> _unitsToGetByTier, List<UnitTier> _unitsPool, int _seed)
        {
            System.Random random = new(Seed: _seed);
            List<SquadToLoad> squads = new();
            foreach (UnitsToGetByTier unit in _unitsToGetByTier) {
                for (int i = 0; i < unit.unitsToGet; i++) {
                    //shoudl get a new random index for each unit
                    int randomIndex = random.Next(0, _unitsPool.Count);
                    UnitName unitName = GetUnitOfTier(unit.unitTier, randomIndex, _unitsPool);
                    squads.Add(new SquadToLoad(unitName));
                }
            }
            return squads.ToArray();
        }
        private static UnitName GetUnitOfTier(int _unitTier, int _randomIndex, List<UnitTier> unitsPool)
        {
            System.Random random = new(_randomIndex);
            UnitTier selectedUnit = null;
            // work on a local copy so the caller's list is not mutated
            List<UnitTier> shuffled = new(unitsPool);
            for (int i = 0; i < shuffled.Count; i++) {
                int randomIndex = random.Next(i, shuffled.Count);
                (shuffled[randomIndex], shuffled[i]) = (shuffled[i], shuffled[randomIndex]);
            }
            foreach (UnitTier unit in shuffled)
            {
                if (unit.tier == _unitTier)
                {
                    selectedUnit = unit;
                    break;
                }
            }
            if (selectedUnit == null)
            {
                Debug.LogError($"ArmyCreator: No unit found for tier {_unitTier} in the provided pool.");
                return default;
            }
            return selectedUnit.unitName;
        }
        // NOTE: CustomBattleGeneratorEditor mirrors this switch — update both when changing tier tables.
        public static SquadToLoad[] GenerateEnemyArmy(int _boardNumber, int _battlesFought, int _seed, bool _finalBattle, List<UnitTier> unitsPool, bool knightDifficulty)
        {
            List<UnitsToGetByTier> unitsToGetByTier = new();
            // Debug.Log($"race strength tier: {_raceStrengthTier} for act {_actNumber}, battles fought {_battlesFought}, final battle {_finalBattle}");

            switch (_boardNumber)
            {
                //Board 1
                case 1:
                    {
                        if (_finalBattle)
                        {
                            if(knightDifficulty)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 2 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                        }
                        else
                        {
                            //if battles fought is less than 4, just get 4 units from the first tier
                            if (_battlesFought < 3)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 4 });
                            }
                            else if (_battlesFought < 5)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                            }
                            else if (_battlesFought < 7)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                            }
                        }
                    break;
                }


                //Board 2
                case 2:
                    {
                        if (_finalBattle)
                        {
                            if(knightDifficulty)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 2 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                        }
                        else
                        {
                            if (_battlesFought < 3)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                            }
                            else if (_battlesFought < 5)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                            }
                            else if (_battlesFought < 7)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 5 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                            }
                        }
                    break;
                }

                //Board 3
                case 3:
                    {
                        if (_finalBattle)
                        {
                            if(knightDifficulty)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 5 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                        }
                        else
                        {
                            if (_battlesFought < 3)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                            }
                            else if (_battlesFought < 5)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                            }
                            else if (_battlesFought < 7)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 7 });
                            }
                        }
                    break;
                }
            }

            return CreateArmyFromUnitsByTier(unitsToGetByTier, unitsPool, _seed);
        }

        public static SquadToLoad[] ReplaceMonsterUnits(SquadToLoad[] _squadsToLoad, int _seed, List<UnitTier> unitsPool)
        {
            List<SquadToLoad> newSquads = new();
            System.Random random = new(_seed);
            foreach (SquadToLoad squad in _squadsToLoad)
            {
                if (TabletopTavernData.Instance.GetUnitSizeFromUnitName(squad.UnitName) != UnitSize.Infantry)
                {
                    // Race race = TabletopTavernData.Instance.GetRaceFromUnitName(squad.UnitName);
                    //filter out all units that are not the same tier
                    //filter out all large units
                    int unitTier = TabletopTavernData.Instance.GetUnitTierFromUnitName(squad.UnitName);
                    List<UnitTier> filteredUnitsPool = unitsPool.FindAll(unit => unit.tier == unitTier);
                    filteredUnitsPool = filteredUnitsPool.FindAll(unit => TabletopTavernData.Instance.GetUnitSizeFromUnitName(unit.unitName) != UnitSize.Monstrous && TabletopTavernData.Instance.GetUnitSizeFromUnitName(unit.unitName) != UnitSize.SingleUnit);

                    if (filteredUnitsPool.Count == 0)
                    {
                        Debug.LogError($"ArmyCreator: No replacement infantry unit found for tier {unitTier}. Keeping original squad.");
                        newSquads.Add(squad);
                        continue;
                    }

                    int randomIndex = random.Next(0, filteredUnitsPool.Count);
                    newSquads.Add(new SquadToLoad(filteredUnitsPool[randomIndex].unitName));
                }
                else
                {
                    newSquads.Add(squad);
                }
            }
            return newSquads.ToArray();
        }
    }
}