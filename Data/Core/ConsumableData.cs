using UnityEngine;

namespace TJ
{
    public enum ConsumableEnum { MinorHealth, MajorHealth, Prestige, Duplicate, NewUnit, Alchemist, Rewind, TrialofGrasses, FateshineElixir, RunewellNectar, LambSauce }
    public enum ConsumableRarity { Common, Uncommon, Rare, Legendary }
    [System.Serializable] public struct Consumable
    {
        public ConsumableEnum ConsumableEnum;
        public ConsumableRarity ConsumableRarity;
    }
    public static class ConsumableData
    {
        public static Consumable GetConsumable(ConsumableEnum _consumableType)
        {
            return _consumableType switch
            {
                ConsumableEnum.MinorHealth => MinorHealthPotion,
                ConsumableEnum.MajorHealth => MajorHealthPotion,
                ConsumableEnum.Prestige => PrestigePotion,
                ConsumableEnum.Duplicate => DuplicateUnitPotion,
                ConsumableEnum.NewUnit => UnitInABottle,
                ConsumableEnum.Alchemist => AlchemistPotion,
                ConsumableEnum.Rewind => RewindPotion,
                ConsumableEnum.TrialofGrasses => TrialofGrasses,
                ConsumableEnum.FateshineElixir => FateshineElixir,
                ConsumableEnum.RunewellNectar => RunewellNectar,
                ConsumableEnum.LambSauce => LambSauce,

                _ => new Consumable(),
            };
        }
        public static Consumable MinorHealthPotion = new ()
        {
            ConsumableEnum = ConsumableEnum.MinorHealth,
            // ConsumableDescription = "Heals a unit for 50% of its max health",
            ConsumableRarity = ConsumableRarity.Common,
        };
        public static Consumable AlchemistPotion = new () // cannot appear in shop
        {
            ConsumableEnum = ConsumableEnum.Alchemist,
            // ConsumableDescription = "Instantly gain 5 gold",
            ConsumableRarity = ConsumableRarity.Common,
        };
        public static Consumable RewindPotion = new()
        {
            ConsumableEnum = ConsumableEnum.Rewind,
            // ConsumableDescription = "Rerolls the last dice roll",
            ConsumableRarity = ConsumableRarity.Common,
        };
        public static Consumable MajorHealthPotion = new ()
        {
            ConsumableEnum = ConsumableEnum.MajorHealth,
            // ConsumableDescription = "Heals a unit for 100% of its max health",
            ConsumableRarity = ConsumableRarity.Uncommon,
        };
        public static Consumable FateshineElixir = new ()
        {
            ConsumableEnum = ConsumableEnum.FateshineElixir,
            // ConsumableDescription = "Guarantees the next dice roll to be a 20"
            ConsumableRarity = ConsumableRarity.Uncommon,
        };
        public static Consumable PrestigePotion = new ()
        {
            ConsumableEnum = ConsumableEnum.Prestige,
            // ConsumableDescription = "Increases prestige of a unit by 1",
            ConsumableRarity = ConsumableRarity.Rare,
        };
        public static Consumable UnitInABottle = new ()
        {
            ConsumableEnum = ConsumableEnum.NewUnit,
            // ConsumableDescription = "Summons a random unit",
            ConsumableRarity = ConsumableRarity.Rare,
        };
        public static Consumable RunewellNectar = new()
        {
            ConsumableEnum = ConsumableEnum.RunewellNectar,
            // ConsumableDescription = "Fills all your empty potion slots with random potions",
            ConsumableRarity = ConsumableRarity.Rare
        };
        public static Consumable TrialofGrasses = new ()
        {
            ConsumableEnum = ConsumableEnum.TrialofGrasses,
            // ConsumableDescription = "Presitges a max health unit to level 3 with 10% of its max health",
            ConsumableRarity = ConsumableRarity.Legendary,
        };
        public static Consumable DuplicateUnitPotion = new ()
        {
            ConsumableEnum = ConsumableEnum.Duplicate,
            // ConsumableDescription = "Creates a prestige 1 copy of a unit",
            ConsumableRarity = ConsumableRarity.Legendary,
        };
        public static Consumable LambSauce = new ()
        {
            ConsumableEnum = ConsumableEnum.LambSauce,
            // ConsumableDescription = "Creates a signature unit for your hero",
            ConsumableRarity = ConsumableRarity.Legendary
        };

        public static ConsumableEnum[] GetAllConsumableEnums()
        {
            return new[] {
                ConsumableEnum.MinorHealth,//Common
                ConsumableEnum.Rewind,//Common
                ConsumableEnum.Alchemist,//Common
                ConsumableEnum.MajorHealth,//Uncommon
                ConsumableEnum.FateshineElixir,//Uncommon
                ConsumableEnum.Prestige,//Rare
                ConsumableEnum.NewUnit,//Rare
                ConsumableEnum.RunewellNectar,//Rare
                ConsumableEnum.Duplicate,//Legendary
                ConsumableEnum.TrialofGrasses,//Legendary
                ConsumableEnum.LambSauce //Legendary
            };
        }
        public static ConsumableEnum GetRandomConsumable()
        {
            return GetAllConsumableEnums()[Random.Range(0, GetAllConsumableEnums().Length)];
        }
        public static ConsumableEnum GetWeightedConsumable(bool hasLuckyHorseshoe = false)
        {
            ConsumableEnum GetRandomWeightedItem(ConsumableEnum[] items, float[] weights)
            {
                float totalWeight = 0f;
                foreach (float weight in weights)
                {
                    totalWeight += weight;
                }

                float randomValue = Random.Range(0, totalWeight);
                float cumulativeWeight = 0f;

                for (int i = 0; i < items.Length; i++)
                {
                    cumulativeWeight += weights[i];
                    if (randomValue <= cumulativeWeight)
                    {
                        return items[i];
                    }
                }

                return items[items.Length - 1]; // Fallback in case of rounding errors
            }
            ConsumableEnum[] allConsumables = GetAllConsumableEnums();
            if (hasLuckyHorseshoe)
                allConsumables = System.Array.FindAll(allConsumables, c => GetConsumable(c).ConsumableRarity != ConsumableRarity.Common);
            float[] weights = new float[allConsumables.Length];
            for (int i = 0; i < allConsumables.Length; i++)
            {
                weights[i] = ConsumableDropChance(GetConsumable(allConsumables[i]).ConsumableRarity);
            }

            return GetRandomWeightedItem(allConsumables, weights);
        }
        public static ConsumableEnum GetWeightedConsumable(int seed)
        {
            ConsumableEnum GetRandomWeightedItem(ConsumableEnum[] items, float[] weights, System.Random random)
            {
                float totalWeight = 0f;
                foreach (float weight in weights)
                    totalWeight += weight;

                float randomValue = (float)(random.NextDouble() * totalWeight);
                float cumulativeWeight = 0f;

                for (int i = 0; i < items.Length; i++)
                {
                    cumulativeWeight += weights[i];
                    if (randomValue <= cumulativeWeight)
                        return items[i];
                }

                return items[items.Length - 1];
            }

            System.Random random = new(seed);
            ConsumableEnum[] allConsumables = GetAllConsumableEnums();
            float[] weights = new float[allConsumables.Length];
            for (int i = 0; i < allConsumables.Length; i++)
                weights[i] = ConsumableDropChance(GetConsumable(allConsumables[i]).ConsumableRarity);

            return GetRandomWeightedItem(allConsumables, weights, random);
        }
        public static float ConsumableDropChance(ConsumableRarity _consumableRarity) => _consumableRarity switch
        {
            ConsumableRarity.Common => 0.5f,
            ConsumableRarity.Uncommon => 0.25f,
            ConsumableRarity.Rare => 0.15f,
            ConsumableRarity.Legendary => 0.1f,
            _ => 0.2f,
        };
        public static int ConsumableCost(ConsumableRarity _consumableRarity) => _consumableRarity switch
        {
            ConsumableRarity.Common => 4,
            ConsumableRarity.Uncommon => 8,
            ConsumableRarity.Rare => 25,
            ConsumableRarity.Legendary => 60,
            _ => 69,
        };
        public static int SellValue(ConsumableRarity _consumableRarity)
        {
            return ConsumableCost(_consumableRarity) / 2;
        }
        public static bool ConsumableRequiresTarget(ConsumableEnum _consumableEnum)
        {
            return _consumableEnum switch
            {
                ConsumableEnum.MinorHealth => true,
                ConsumableEnum.MajorHealth => true,
                ConsumableEnum.Prestige => true,
                ConsumableEnum.Duplicate => true,
                ConsumableEnum.NewUnit => false,
                ConsumableEnum.Alchemist => false,
                ConsumableEnum.Rewind => false,
                ConsumableEnum.TrialofGrasses => true,
                ConsumableEnum.FateshineElixir => false,
                ConsumableEnum.RunewellNectar => false,
                ConsumableEnum.LambSauce => false,
                _ => false,
            };
        }
    }
}
