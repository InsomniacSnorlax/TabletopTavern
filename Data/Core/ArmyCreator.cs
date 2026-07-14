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

        private struct EnemyPrestigeProfile { public float chancePerSquad; public int maxPrestigedSquads; public float prestigeTwoChance; }

        // Duke+ (DifficultyMod 10) — base rates, indexed by Act (bookNumber - 1)
        private static readonly EnemyPrestigeProfile[] ENEMY_PRESTIGE_BASE = {
            new() { chancePerSquad = 0.10f, maxPrestigedSquads = 1, prestigeTwoChance = 0.00f }, // Act 1
            new() { chancePerSquad = 0.20f, maxPrestigedSquads = 2, prestigeTwoChance = 0.15f }, // Act 2
            new() { chancePerSquad = 0.25f, maxPrestigedSquads = 3, prestigeTwoChance = 0.25f }, // Act 3
        };

        // Emperor+ (DifficultyMod 14) — enhanced rates, replaces the base table when active
        private static readonly EnemyPrestigeProfile[] ENEMY_PRESTIGE_ENHANCED = {
            new() { chancePerSquad = 0.20f, maxPrestigedSquads = 2, prestigeTwoChance = 0.00f }, // Act 1
            new() { chancePerSquad = 0.35f, maxPrestigedSquads = 3, prestigeTwoChance = 0.20f }, // Act 2
            new() { chancePerSquad = 0.45f, maxPrestigedSquads = 4, prestigeTwoChance = 0.30f }, // Act 3
        };

        private const int ENEMY_PRESTIGE_SEED_OFFSET = 104729; // decorrelate from the deck-shuffle RNG in CreateArmyFromUnitsByTier

        // DifficultyMod 10 / DifficultyMod 14: gives enemy squads a chance to spawn already prestiged, scaling with Act and difficulty
        private static SquadToLoad[] ApplyEnemyPrestige(SquadToLoad[] _squads, int _actNumber, int _seed, bool _enhanced)
        {
            if (_squads.Length == 0) return _squads;

            EnemyPrestigeProfile[] profiles = _enhanced ? ENEMY_PRESTIGE_ENHANCED : ENEMY_PRESTIGE_BASE;
            int actIndex = System.Math.Clamp(_actNumber - 1, 0, profiles.Length - 1);
            EnemyPrestigeProfile profile = profiles[actIndex];

            System.Random random = new(_seed + ENEMY_PRESTIGE_SEED_OFFSET);

            // Shuffle selection order so the cap doesn't systematically favor whichever tier CreateArmyFromUnitsByTier appended first
            List<int> order = new();
            for (int i = 0; i < _squads.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (order[j], order[i]) = (order[i], order[j]);
            }

            int prestiged = 0;
            foreach (int index in order)
            {
                if (prestiged >= profile.maxPrestigedSquads) break;
                if (_squads[index].isEmptySquad) continue;
                if (random.NextDouble() >= profile.chancePerSquad) continue;

                int level = random.NextDouble() < profile.prestigeTwoChance ? 2 : 1;
                _squads[index].UnitPrestige = level;

                if (level == 2)
                {
                    SquadStats squadStats = TabletopTavernData.Instance.GetSquadStats(_squads[index].UnitName);
                    List<UnitAttribute> eligible = TabletopTavernConstants.GetEligiblePrestigeTraits(squadStats);
                    if (eligible.Count > 0)
                        _squads[index].PrestigeTrait = eligible[random.Next(eligible.Count)];
                }
                prestiged++;
            }
            return _squads;
        }

        public static SquadToLoad[] GenerateTownGarrison(TownSize _townSize, int _seed, List<UnitTier> unitsPool, bool difficultyImperator, int _bookNumber, bool enemyPrestigeEligible, bool enemyPrestigeEnhanced)
        {
            // Garrisons don't field cavalry or outriders — filter them out before picking units
            unitsPool = unitsPool.FindAll(u =>
                TabletopTavernData.Instance.GetUnitSizeFromUnitName(u.unitName) != UnitSize.Cavalry
            );

            static UnitsToGetByTier T(int tier, int count) => new() { unitTier = tier, unitsToGet = count };

            List<UnitsToGetByTier> unitsToGetByTier = (_bookNumber, _townSize, difficultyImperator) switch
            {
                (1, TownSize.Village, false) => new() { T(1, 3) },
                (1, TownSize.Castle,  false) => new() { T(1, 4), T(2, 2) },
                (1, TownSize.City,    false) => new() { T(1, 2), T(2, 2), T(3, 2) },

                (1, TownSize.Village, true)  => new() { T(1, 4) },
                (1, TownSize.Castle,  true)  => new() { T(1, 2), T(2, 4)},
                (1, TownSize.City,    true)  => new() { T(1, 3), T(2, 3), T(3, 2) },


                (2, TownSize.Village, false) => new() { T(1, 5) },
                (2, TownSize.Castle,  false) => new() { T(2, 6) },
                (2, TownSize.City,    false) => new() { T(2, 4), T(3, 2) },

                (2, TownSize.Village, true)  => new() { T(1, 6) },
                (2, TownSize.Castle,  true)  => new() { T(2, 7) },
                (2, TownSize.City,    true)  => new() { T(2, 4), T(3, 4) },


                (3, TownSize.Village, false) => new() { T(1, 8) },
                (3, TownSize.Castle,  false) => new() { T(1, 2), T(2, 5) },
                (3, TownSize.City,    false) => new() { T(2, 3), T(3, 6) },

                (3, TownSize.Village, true)  => new() { T(1, 9) },
                (3, TownSize.Castle,  true)  => new() { T(1, 2), T(2, 7) },
                (3, TownSize.City,    true)  => new() { T(2, 2), T(3, 7) },

                _ => new()
            };

            SquadToLoad[] garrison = CreateArmyFromUnitsByTier(unitsToGetByTier, unitsPool, _seed);
            if (enemyPrestigeEligible) garrison = ApplyEnemyPrestige(garrison, _bookNumber, _seed, enemyPrestigeEnhanced);
            return garrison;
        }
        private static SquadToLoad[] CreateArmyFromUnitsByTier(List<UnitsToGetByTier> _unitsToGetByTier, List<UnitTier> _unitsPool, int _seed)
        {
            System.Random random = new(_seed);
            List<SquadToLoad> squads = new();

            Dictionary<int, List<UnitName>> decksByTier = new();
            foreach (UnitsToGetByTier entry in _unitsToGetByTier)
            {
                if (decksByTier.ContainsKey(entry.unitTier)) continue;
                List<UnitName> deck = new();
                foreach (UnitTier u in _unitsPool)
                    if (u.tier == entry.unitTier) deck.Add(u.unitName);
                if (deck.Count == 0)
                {
                    Debug.LogError($"ArmyCreator: No unit found for tier {entry.unitTier} in the provided pool.");
                    continue;
                }
                for (int i = deck.Count - 1; i > 0; i--)
                {
                    int j = random.Next(0, i + 1);
                    (deck[j], deck[i]) = (deck[i], deck[j]);
                }
                decksByTier[entry.unitTier] = deck;
            }

            foreach (UnitsToGetByTier entry in _unitsToGetByTier)
            {
                if (!decksByTier.TryGetValue(entry.unitTier, out List<UnitName> deck)) continue;
                for (int i = 0; i < entry.unitsToGet; i++)
                {
                    UnitName chosen = random.NextDouble() < 0.35
                        ? deck[random.Next(deck.Count)]
                        : deck[i % deck.Count];
                    squads.Add(new SquadToLoad(chosen));
                }
            }
            return squads.ToArray();
        }
        // NOTE: CustomBattleGeneratorEditor mirrors this switch — update both when changing tier tables.
        public static SquadToLoad[] GenerateEnemyArmy(int _boardNumber, int _battlesFought, int _seed, bool _finalBattle, List<UnitTier> unitsPool, bool knightDifficulty, bool enemyPrestigeEligible, bool enemyPrestigeEnhanced)
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
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
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
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 1 });
                            }
                            else if (_battlesFought < 7)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
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
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                        }
                        else
                        {
                            if (_battlesFought < 3)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 1 });
                            }
                            else if (_battlesFought < 5)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 2 });
                            }
                            else if (_battlesFought < 7)
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 3 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 1 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 5 });
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
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 3 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 4 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 4, unitsToGet = 1 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 3 });
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
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 1, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 5 });
                            }
                            else
                            {
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 2, unitsToGet = 2 });
                                unitsToGetByTier.Add(new UnitsToGetByTier { unitTier = 3, unitsToGet = 7 });
                            }
                        }
                    break;
                }
            }

            SquadToLoad[] army = CreateArmyFromUnitsByTier(unitsToGetByTier, unitsPool, _seed);
            if (enemyPrestigeEligible) army = ApplyEnemyPrestige(army, _boardNumber, _seed, enemyPrestigeEnhanced);
            return army;
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
                    List<UnitTier> filteredUnitsPool = new();
                    int searchTier = unitTier;
                    while (filteredUnitsPool.Count == 0 && searchTier >= 1)
                    {
                        filteredUnitsPool = unitsPool.FindAll(unit => unit.tier == searchTier);
                        filteredUnitsPool = filteredUnitsPool.FindAll(unit => TabletopTavernData.Instance.GetUnitSizeFromUnitName(unit.unitName) != UnitSize.Monstrous && TabletopTavernData.Instance.GetUnitSizeFromUnitName(unit.unitName) != UnitSize.SingleUnit);
                        searchTier--;
                    }

                    if (filteredUnitsPool.Count == 0)
                    {
                        Debug.LogError($"ArmyCreator: No replacement infantry unit found for tier {unitTier} or below. Keeping original squad.");
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