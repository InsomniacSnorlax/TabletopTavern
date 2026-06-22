using UnityEngine;
using Memori.SaveData;
using Memori.Utilities;
using Memori.Scenes;
using Memori.Localization;
using System.Collections.Generic;

namespace TJ
{
    public class HeroBonusManager : Singleton<HeroBonusManager>
    {
        [SerializeField] private int activeHeroID = -1;
        [SerializeField] private bool isCustomBattle = false;
        [SerializeField] private static Race enemyRace;
        public int ActiveHeroID => activeHeroID;
        SceneHandler sceneHandler;

        private void Start()
        {
            sceneHandler = SceneHandler.Instance;
            sceneHandler.OnGameStateChanged += OnGameStateChanged;
        }
        public List<UnitStatBonus> GetHeroStatBonus(UnitStat _unitStat, UnitName _requestingUnit, float currentStatValue)
        {
            return GetHeroStatBonus(_unitStat, _requestingUnit, activeHeroID, currentStatValue);
        }
        public List<UnitAttributeBonus> GetHeroAttributeBonus(UnitName _requestingUnit)
        {
            return GetHeroAttributeBonus(_requestingUnit, activeHeroID);
        }
        public void OnGameStateChanged(GameStateEnum gameStateEnum)
        {
            isCustomBattle = SaveDataHandler.IsCustomBattle();
            activeHeroID = SaveDataHandler.GetActiveHeroID();
            enemyRace = SaveDataHandler.GetEnemyRace();
            // #if !UNITY_EDITOR
            //     if(activeHeroID != 1 || activeHeroID != 2 ||activeHeroID != 5 || activeHeroID != 6 ) {
            //         activeHeroID = 1;
            //     }
            // #endif
            
            if(isCustomBattle || gameStateEnum == GameStateEnum.MainMenu) {
                activeHeroID = -1;
            }
            // Debug.Log($"Active Hero ID: {activeHeroID}, custom battle: {isCustomBattle}");
        }
        public static List<UnitStatBonus> GetFactionBonus(UnitStat _unitStat)
        {
            List<UnitStatBonus> unitStatBonuses = new();

            //If army contains only Sakura Dynasty units, all units gain +20 [Leadership] and +4 [Melee Attack]
            if (_unitStat == UnitStat.MeleeAttack)
            {
                string localisedBonusName = LocalizationManager.Instance.GetText("SakuraDynastyBonusDescription");
                //parse everything to the left of the first :
                localisedBonusName = localisedBonusName.Split(':')[0];
                unitStatBonuses.Add(new UnitStatBonus(_unitStat, localisedBonusName, 4));
            }
            if (_unitStat == UnitStat.Leadership)
            {
                string localisedBonusName = LocalizationManager.Instance.GetText("SakuraDynastyBonusDescription");
                //parse everything to the left of the first :
                localisedBonusName = localisedBonusName.Split(':')[0];
                unitStatBonuses.Add(new UnitStatBonus(_unitStat, localisedBonusName, 20));
            }
        
            return unitStatBonuses;
        }
                    
        public static List<UnitStatBonus> GetHeroStatBonus(UnitStat _unitStat, UnitName _requestingUnit, int activeHeroID, float currentStatValue)
        {
            List<UnitStatBonus> unitStatBonuses = new();

            if (activeHeroID == -1) return unitStatBonuses;

            switch (activeHeroID)
            {
                case 1:
                    //Take Back our Lands: All units gain +2 [Charge Bonus]
                    if (_unitStat == UnitStat.ChargeBonus)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle2"), 2));
                    }
                    //The olde guard: [Rare] units gain +10 [Leadership] and +4 [Melee Attack]
                    if (_unitStat == UnitStat.MeleeAttack && TabletopTavernData.Instance.GetUnitTierFromUnitName(_requestingUnit) == 3)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle1"), 4));
                    }
                    if (_unitStat == UnitStat.Leadership && TabletopTavernData.Instance.GetUnitTierFromUnitName(_requestingUnit) == 3)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle1"), 10));
                    }
                    break;
                case 2:
                    //Dúnedain Captain: Deepwood Rangers gain +10 [Accuracy] and +4 [Missile Strength]
                    if (_unitStat == UnitStat.Accuracy && _requestingUnit == UnitName.DeepwoodRangers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle3"), 10));
                    }
                    if (_unitStat == UnitStat.MissileStrength && _requestingUnit == UnitName.DeepwoodRangers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle3"), 4));
                    }
                    //The Everyman: [Common] units gain +10 [Leadership] and +4 [Melee Defense]
                    if (_unitStat == UnitStat.MeleeDefense && TabletopTavernData.Instance.GetUnitTierFromUnitName(_requestingUnit) == 1)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle4"), 4));
                    }
                    if (_unitStat == UnitStat.Leadership && TabletopTavernData.Instance.GetUnitTierFromUnitName(_requestingUnit) == 1)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle4"), 10));
                    }
                    break;
                case 3:
                    //go forth my hordes: Goblins gain +4 [Melee Defense] and +4 [Melee Attack]
                    if (_unitStat == UnitStat.MeleeDefense && TabletopTavernConstants.IsAGoblinUnit(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle6"), 6));
                    }
            
                    if (_unitStat == UnitStat.MeleeAttack && TabletopTavernConstants.IsAGoblinUnit(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle6"), 6));
                    }
                    break;
                case 4:
                    //A taste for blood: Orc Ravagers cause [Terror] and gain +10 [Melee Attack] and +4 [Weapon Strength]
                    if (_unitStat == UnitStat.MeleeAttack && _requestingUnit == UnitName.OrcRavagers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle7"), 10));
                    }
                    if (_unitStat == UnitStat.WeaponStrength && _requestingUnit == UnitName.OrcRavagers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle7"), 4));
                    }
                    break;
                case 5:
                    //Ironskin: Melee Infantry gain +4 [Melee Defense], Armored Units gain +10 [Armor]
                    if (_unitStat == UnitStat.MeleeDefense && TabletopTavernData.Instance.IsMeleeInfantry(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle9"), 4));
                    }
                    if (_unitStat == UnitStat.Armor && TabletopTavernData.Instance.IsMeleeInfantry(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle9"), 10));
                    }
                    break;
                case 6:
                    //With me sisters!: Shieldmaiden units gain +10 [Leadership] and +4 [Melee Defense]
                    if (_unitStat == UnitStat.MeleeDefense && _requestingUnit == UnitName.Shieldmaidens)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle11"), 4));
                    }
                    if (_unitStat == UnitStat.Leadership && _requestingUnit == UnitName.Shieldmaidens)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle11"), 10));
                    }
                    break;
                case 7:
                    //Supernova of the West: All units gain +5 [Charge Bonus] and +4 [Melee Attack]
                    if (_unitStat == UnitStat.ChargeBonus)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle13"), 5));
                    }
                    if (_unitStat == UnitStat.MeleeAttack)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle13"), 4));
                    }
                    break;
                case 8:
                    //The Forest Walks: Forest Spirits and Treants gain +5 [Melee Defense] and +4 [Weapon Strength]
                    if (_unitStat == UnitStat.MeleeDefense && (_requestingUnit == UnitName.ForestSpirits || _requestingUnit == UnitName.Treants))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle16"), 5));
                    }
                    if (_unitStat == UnitStat.WeaponStrength && (_requestingUnit == UnitName.ForestSpirits || _requestingUnit == UnitName.Treants))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle16"), 4));
                    }
                    break;
                case 9: //Sister Morvayne
                    break;
                case 10: //Lord Draven Bloodreaver
                    //Bloodsworn Prince: Bloodsworn and Bloodsworn Knights gain +15 [Leadership] and +4 [Melee Attack]
                    if (_unitStat == UnitStat.Leadership && (_requestingUnit == UnitName.Bloodsworn || _requestingUnit == UnitName.BloodswornKnights))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle19"), 15));
                    }
                    if (_unitStat == UnitStat.MeleeAttack && (_requestingUnit == UnitName.Bloodsworn || _requestingUnit == UnitName.BloodswornKnights))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle19"), 8));
                    }
                    break;
                case 11: //Oda Nobukage
                    //Nagoya Steel: All melee units gain +4 [Weapon Strength]
                    if (_unitStat == UnitStat.MeleeAttack)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle21"), 4));
                    }
                    break;
                case 12: //Tokugawa Harunobu
                    //Innovator's Legacy: Emperors Fusiliers Gain +10 [Accuracy] and +4 [Missile Strength]
                    if (_unitStat == UnitStat.Accuracy && _requestingUnit == UnitName.EmperorsArquebusiers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle24"), 10));
                    }
                    if (_unitStat == UnitStat.MissileStrength && _requestingUnit == UnitName.EmperorsArquebusiers)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle24"), 4));
                    }
                    break;
                case 13: //Hrothgar Goblinslayer
                    //Ancestral Hatred: All units gain +10 [Melee Attack] and +4 [Weapon Strength] when fignting the Gruntkin
                    if(enemyRace == Race.Gruntkin) {
                        if (_unitStat == UnitStat.MeleeAttack)
                        {
                            unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle25"), 10));
                        }
                        if (_unitStat == UnitStat.WeaponStrength)
                        {
                            unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle25"), 4));
                        }
                    }
                    break;
                case 14: //Bertha Barrelstorm
                    //Blasting Barrels: Artillery units gain +10 [Accuracy] and +4 [Missile Strength].
                    if (_unitStat == UnitStat.Accuracy && UnitType.Artillery == TabletopTavernData.Instance.GetUnitTypeFromUnitName(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle27"), 10));
                    }
                    if (_unitStat == UnitStat.MissileStrength && UnitType.Artillery == TabletopTavernData.Instance.GetUnitTypeFromUnitName(_requestingUnit))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle27"), 4));
                    }
                    //Supply Lines: All ranged units gain 50% increased ammunition capacity.
                    if (_unitStat == UnitStat.Ammunition)
                    {
                        UnitType unitType = TabletopTavernData.Instance.GetUnitTypeFromUnitName(_requestingUnit);
                        if(unitType == UnitType.Ranged || unitType == UnitType.Artillery)
                        {
                            unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle28"), currentStatValue*0.5f)); 
                        }
                    }
                    break;
                case 15: //Skrix the Swarmcaller
                    //Kobold Kammandos: Kobold units gain +10 [Leadership] and +4 [Melee Attack]
                    if (_unitStat == UnitStat.Leadership && (_requestingUnit == UnitName.KoboldBrawlers || _requestingUnit == UnitName.ScalebowKobolds))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle29"), 10));
                    }
                    if (_unitStat == UnitStat.MeleeAttack && (_requestingUnit == UnitName.KoboldBrawlers || _requestingUnit == UnitName.ScalebowKobolds))
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle29"), 4));
                    }
                    break;
                case 16: //Valthrex Primeclaw
                    //Beastmaster: Large units gain +10 [Leadership] and +4 [Melee Defense]
                    if (_unitStat == UnitStat.Leadership && TabletopTavernData.Instance.GetUnitSizeFromUnitName(_requestingUnit) != UnitSize.Infantry)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle31"), 10));
                    }
                    if (_unitStat == UnitStat.MeleeDefense && TabletopTavernData.Instance.GetUnitSizeFromUnitName(_requestingUnit) != UnitSize.Infantry)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle31"), 4));
                    }
                    // Sacred Guard: StegoplateGuard gain +15 [Armor] and +4 [Weapon Strength]
                    if (_unitStat == UnitStat.Armor && _requestingUnit == UnitName.StegoplateGuard)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle32"), 15));
                    }
                    if (_unitStat == UnitStat.WeaponStrength && _requestingUnit == UnitName.StegoplateGuard)
                    {
                        unitStatBonuses.Add(new UnitStatBonus(_unitStat, LocalizationManager.Instance.GetText("heroBonusTitle32"), 4));
                    }
                    break;
            }

            return unitStatBonuses;
        }
        public static List<UnitAttributeBonus> GetHeroAttributeBonus(UnitName _requestingUnit, int activeHeroID)
        {
            List<UnitAttributeBonus> unitAttributeBonuses = new();

            if(activeHeroID == -1) return unitAttributeBonuses;

            SquadAttributes squadAttributes = TabletopTavernData.Instance.GetSquadStats(_requestingUnit).SquadAttributes;

            switch(activeHeroID)
            {
                case 4:
                    // Orc Ravagers cause [Terror]
                    if(_requestingUnit == UnitName.OrcRavagers && !squadAttributes.Terrifying) {
                        unitAttributeBonuses.Add(new UnitAttributeBonus(UnitAttribute.Terrifying, LocalizationManager.Instance.GetText("heroBonusTitle7"), 0));
                    }
                    break;
                case 6:
                    // All units are immune to Terror
                    if(!squadAttributes.Stalwart) {
                        unitAttributeBonuses.Add(new UnitAttributeBonus(UnitAttribute.Stalwart, LocalizationManager.Instance.GetText("heroBonusTitle12"), 0));
                    }
                    break;
                case 9: //Sister Morvayne
                    //Unbound by Chivalry: All units gain the [Outrider] ability
                    if(!squadAttributes.Outrider) {
                        unitAttributeBonuses.Add(new UnitAttributeBonus(UnitAttribute.Outrider, LocalizationManager.Instance.GetText("heroBonusTitle18"), 0));
                    }
                    break;
                case 13: //Hrothgar Goblinslayer
                    //Swarm Breaker: Cragflayers gain the [Rage] ability
                    if(_requestingUnit == UnitName.Cragflayers && !squadAttributes.Rage) {
                        unitAttributeBonuses.Add(new UnitAttributeBonus(UnitAttribute.Rage, LocalizationManager.Instance.GetText("heroBonusTitle26"), 0));
                    }
                    break;
            }

            return unitAttributeBonuses;
        }
        public static string GetLocalizedHeroUnlockDescription(Hero _hero, UnlockCondition _unlockCondition)
        {
            if(_unlockCondition != UnlockCondition.HeroCompletion) 
                return LocalizationManager.Instance.GetText(_unlockCondition.ToString());

            Hero prerequisiteHero = HeroData.GetHeroByID(_hero.HeroID - 1);
            string prerequisiteHeroName = LocalizationManager.Instance.GetText(prerequisiteHero.HeroName);
            string heroCompletionText = LocalizationManager.Instance.GetText("heroCompletion");
            return $"{heroCompletionText} {prerequisiteHeroName}";
        }
        public void OnDestroy()
        {
            sceneHandler.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}