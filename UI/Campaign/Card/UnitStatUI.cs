using System.Collections.Generic;
using Memori.Tooltip;
using Memori.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Memori.Scenes;
using Memori.SaveData;
using Memori.Localization;

namespace TJ
{
    [RequireComponent(typeof(MemoriTooltipTrigger))]
    public class UnitStatUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statScoreText, statNameText;
        [SerializeField] private Image statImage;
        [SerializeField] private Slider baseValueBar, bonusValueBar;
        float amount;
        int totalBonus;
        UnitStat unitStat;
        MemoriTooltipTrigger memoriTooltipTrigger;
        GearManager gearManager;
        public void LoadUnitStatUI(UnitStatValue _unitStatValue, int _prestige, UnitName _unitName, bool applyGearBonuses)
        {
            amount = _unitStatValue.Value;
            unitStat = _unitStatValue.unitStat;
            
            string baseValueLocalized = LocalizationManager.Instance.GetText("Base Value");
            string PrestigeLocalised = LocalizationManager.Instance.GetText("Prestige");

            gearManager = SceneHandler.Instance.CurrentGameState switch
            {
                GameStateEnum.Battle => BattleManager.Instance.GearManager,
                GameStateEnum.Map => CampaignManager.Instance.GearManager,
                //menu
                _ => null,
            };
            memoriTooltipTrigger = GetComponent<MemoriTooltipTrigger>();

            statImage.sprite = SpriteData.GetSprite(unitStat.ToString());
            statImage.color = ColorData.GetUnitStatColor(unitStat);
            statNameText.text = LocalizationManager.Instance.GetText(unitStat.ToString());

            totalBonus = 0;
            string description = LocalizationManager.Instance.GetText(unitStat.ToString()+"Desc");
            description += $"\n\n<color {ColorData.Green}>{baseValueLocalized}: {amount}</color>";
            
            if(_prestige > 0)
            {
                UnitType unitType = TabletopTavernData.Instance.GetUnitTypeFromUnitName(_unitName);
                if(TabletopTavernConstants.UsesMeleePrestige(_unitName) || (unitType != UnitType.Ranged && unitType != UnitType.Artillery))
                {
                    if(UnitStat.MeleeAttack == unitStat || UnitStat.MeleeDefense == unitStat || UnitStat.Leadership == unitStat)
                    {
                        static string PrestigeRomanNumeral(int _prestige) {
                            return _prestige switch {
                                0 => "I",
                                1 => "II",
                                2 => "III",
                                _ => "",
                            };
                        }
                        totalBonus += TabletopTavernConstants.PRESTIGE_BONUS * _prestige;
                        description += $"\n<color {ColorData.Green}>{PrestigeLocalised} {PrestigeRomanNumeral(_prestige)}: +{TabletopTavernConstants.PRESTIGE_BONUS * _prestige}</color>";
                    }
                }
                else
                {
                    if(UnitStat.Range == unitStat || UnitStat.Accuracy == unitStat)
                    {
                        static string PrestigeRomanNumeral(int _prestige) {
                            return _prestige switch {
                                0 => "I",
                                1 => "II",
                                2 => "III",
                                _ => "",
                            };
                        }
                        totalBonus += TabletopTavernConstants.PRESTIGE_BONUS * _prestige;
                        description += $"\n<color {ColorData.Green}>{PrestigeLocalised} {PrestigeRomanNumeral(_prestige)}: +{TabletopTavernConstants.PRESTIGE_BONUS * _prestige}</color>";
                    }
                    else if(UnitStat.Ammunition == unitStat)
                    {
                        static string PrestigeRomanNumeral(int _prestige) {
                            return _prestige switch {
                                0 => "I",
                                1 => "II",
                                2 => "III",
                                _ => "",
                            };
                        }
                        int ammoBonusPerLevel = unitType == UnitType.Artillery ? TabletopTavernConstants.PRESTIGE_AMMO_BONUS_ARTILLERY : TabletopTavernConstants.PRESTIGE_AMMO_BONUS_RANGED;
                        totalBonus += ammoBonusPerLevel * _prestige;
                        description += $"\n<color {ColorData.Green}>{PrestigeLocalised} {PrestigeRomanNumeral(_prestige)}: +{ammoBonusPerLevel * _prestige}</color>";
                    }
                }
            }

            if (gearManager != null && applyGearBonuses)
            {
                //get gear bonuses
                List<UnitStatBonus> unitBonues = gearManager.GetGearStatBonus(unitStat, _unitName);
                foreach (UnitStatBonus unitBonus in unitBonues)
                {
                    totalBonus += (int)unitBonus.Value;
                    description += $"\n<color {ColorData.Green}>{unitBonus.BonusName}: +{unitBonus.Value}</color>";
                }

                //get heroes bonuses
                List<UnitStatBonus> heroBonuses = HeroBonusManager.Instance.GetHeroStatBonus(unitStat, _unitName, amount);
                foreach (UnitStatBonus unitBonus in heroBonuses)
                {
                    totalBonus += (int)unitBonus.Value;
                    description += $"\n<color {ColorData.Green}>{unitBonus.BonusName}: +{unitBonus.Value}</color>";
                }

                //get faction bonuses
                if (HeroBonusManager.Instance.ActiveHeroID == 11 || HeroBonusManager.Instance.ActiveHeroID == 12)
                {
                    //check if units only sakura dynasty
                    if (BattleManager.HasInstance && BattleManager.Instance.OnlySakuraUnits)
                    {
                        List<UnitStatBonus> factionBonuses = HeroBonusManager.GetFactionBonus(unitStat);

                        foreach (UnitStatBonus unitBonus in factionBonuses)
                        {
                            totalBonus += (int)unitBonus.Value;
                            description += $"\n<color {ColorData.Green}>{unitBonus.BonusName}: +{unitBonus.Value}</color>";
                        }
                    }
                }
            }

            //if battle, check squad for battlefield bonuses and defensive stance
            if(SceneHandler.Instance.CurrentGameState == GameStateEnum.Battle)
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                SquadEntity squadEntity = BattleManager.Instance.UIManager.SquadBattleInfo.SquadEntity;
                if(entityManager.Exists(squadEntity.SelfEntity))
                {
                    if(entityManager.HasComponent<BattlefieldBonusBufferElement>(squadEntity.SelfEntity))
                    {
                        DynamicBuffer<BattlefieldBonusBufferElement> battlefieldBonus = entityManager.GetBuffer<BattlefieldBonusBufferElement>(squadEntity.SelfEntity);
                        foreach(BattlefieldBonusBufferElement bonus in battlefieldBonus)
                        {
                            if (bonus.Value.UnitStat == unitStat)
                            {
                                if(unitStat == UnitStat.Armor)
                                {
                                    float armorPenalty = (bonus.Value.Value * 100) / (1 + bonus.Value.Value); // Convert mitigation to armor
                                    int roundedArmorPenalty = Mathf.RoundToInt(armorPenalty);
                                    totalBonus += roundedArmorPenalty;
                                    string localisedBonusName = LocalizationManager.Instance.GetText(bonus.Value.BattlefieldBonusEnum.ToString());
                                    description += $"\n<color {(bonus.Value.Value > 0 ? ColorData.Green : ColorData.Error)}>{localisedBonusName}: {(bonus.Value.Value > 0 ? "+" : "")}{roundedArmorPenalty} </color>";
                                }
                                else if (unitStat == UnitStat.Speed)
                                {
                                    if (bonus.Value.BattlefieldBonusEnum != BattlefieldBonusEnum.Rain && TabletopTavernData.Instance.IgnoresSwamp(_unitName))
                                    {
                                        continue;
                                    }
                                    //speed bonuses are in percentage such as 0.25 the total bonus should be the amount * (1 - bonus.Value.Value)
                                    totalBonus -= Mathf.RoundToInt(amount * (1f - bonus.Value.Value));
                                    string localisedBonusName = LocalizationManager.Instance.GetText(bonus.Value.BattlefieldBonusEnum.ToString());
                                    description += $"\n<color {ColorData.Error}>{localisedBonusName}: {totalBonus} </color>";

                                }
                                else if (bonus.Value.BattlefieldBonusEnum == BattlefieldBonusEnum.Fog)
                                {
                                    totalBonus -= (int)(amount * 0.5f);
                                    string localisedBonusName = LocalizationManager.Instance.GetText(bonus.Value.BattlefieldBonusEnum.ToString());
                                    description += $"\n<color {ColorData.Error}>{localisedBonusName}: -50% </color>";
                                }
                                else
                                {
                                    totalBonus += (int)bonus.Value.Value;
                                    string localisedBonusName = LocalizationManager.Instance.GetText(bonus.Value.BattlefieldBonusEnum.ToString());
                                    description += $"\n<color {(bonus.Value.Value > 0 ? ColorData.Green : ColorData.Error)}>{localisedBonusName}: {(bonus.Value.Value > 0 ? "+" : "")}{bonus.Value.Value} </color>";
                                }
                            }
                            
                            if(TabletopTavernData.Instance.IsForestDweller(_unitName))
                            {
                                // Debug.Log($"Squad is forest dweller, checking for forest bonuses");
                                if (bonus.Value.BattlefieldBonusEnum == BattlefieldBonusEnum.Forest)
                                {
                                    if (unitStat == UnitStat.MeleeAttack)
                                    {
                                        totalBonus += 5;
                                        string localisedBonusName = LocalizationManager.Instance.GetText("ForestDweller");
                                        description += $"\n<color {ColorData.Green}>{localisedBonusName}: +5 </color>";
                                    }
                                    else if (unitStat == UnitStat.MissileStrength)
                                    {
                                        totalBonus += 5;
                                        string localisedBonusName = LocalizationManager.Instance.GetText("ForestDweller");
                                        description += $"\n<color {ColorData.Green}>{localisedBonusName}: +5 </color>";
                                    }
                                }
                            }
                        }
                    }
                    if((unitStat == UnitStat.MeleeAttack || unitStat == UnitStat.MeleeDefense) && entityManager.HasComponent<ShieldedStanceSquadComponent>(squadEntity.SelfEntity))
                    {
                        ShieldedStanceSquadComponent shieldedStanceSquadComponent = entityManager.GetComponentData<ShieldedStanceSquadComponent>(squadEntity.SelfEntity);
                        if(shieldedStanceSquadComponent.Stance == ShieldedStance.Defensive)
                        {
                            if(unitStat == UnitStat.MeleeAttack)
                            {
                                string defensiveStanceLocalised = LocalizationManager.Instance.GetText("DefensiveStanceTitle");
                                totalBonus -= (int)(amount / 2); //defensive stance reduces melee attack by 50% of base
                                description += $"\n<color {ColorData.Error}>{defensiveStanceLocalised}: -50% </color>";
                            }
                            else if(unitStat == UnitStat.MeleeDefense)
                            {
                                string defensiveStanceLocalised = LocalizationManager.Instance.GetText("DefensiveStanceTitle");
                                totalBonus += (int)(amount / 2); //defensive stance increases melee defense by 50% of base
                                description += $"\n<color {ColorData.Green}>{defensiveStanceLocalised}: +50% </color>";
                            }
                        }
                    }
                    if(unitStat == UnitStat.Accuracy && entityManager.HasComponent<RangedFireModeSquadComponent>(squadEntity.SelfEntity))
                    {
                        RangedFireModeSquadComponent rangedFireModeSquadComponent = entityManager.GetComponentData<RangedFireModeSquadComponent>(squadEntity.SelfEntity);
                        if(rangedFireModeSquadComponent.FireMode == RangedFireMode.FireAtWill)
                        {
                            totalBonus -= (int)(amount * 0.2);
                            string fireAtWillLocalised = LocalizationManager.Instance.GetText("FireAtWillTitle");
                            description += $"\n<color {ColorData.Error}>{fireAtWillLocalised}: -20% </color>";
                        }
                    }
                    if(unitStat == UnitStat.Ammunition && entityManager.HasComponent<RangedSquad>(squadEntity.SelfEntity))
                    {
                        RangedSquad rangedSquadComponent = entityManager.GetComponentData<RangedSquad>(squadEntity.SelfEntity);
                        int ammunitionLost = (int)(amount + totalBonus - rangedSquadComponent.Ammunition);
                        if(ammunitionLost > 0)
                        {
                            totalBonus -= ammunitionLost;
                            string ammunitionDepletedLocalised = LocalizationManager.Instance.GetText("AmmunitionDepletedTitle");
                            description += $"\n<color {ColorData.Error}>{ammunitionDepletedLocalised}: -{ammunitionLost} </color>";
                        }
                    }
                    if (unitStat == UnitStat.Leadership && entityManager.HasComponent<DefendersResolveComponent>(squadEntity.SelfEntity))
                    {
                        int bonus = (int)TabletopTavernConstants.FORTIFIED_MORALE_BONUS;
                        totalBonus += bonus;
                        string fortifiedMoraleLocalised = LocalizationManager.Instance.GetText("DefendersResolve");
                        description += $"\n<color {ColorData.Green}>{fortifiedMoraleLocalised}: +{bonus} </color>";
                    }
                }
            }

            statScoreText.text = $"{amount + totalBonus}";
            Color textColor = Color.black;
            textColor = totalBonus switch
            {
                int n when n < 0 => (Color)ColorData.HexToRgba(ColorData.Error),
                int n when n > 0 => (Color)ColorData.HexToRgba(ColorData.Green),
                _ => (Color)ColorData.HexToRgba(ColorData.Primary),
            };
            statScoreText.color = textColor;

            memoriTooltipTrigger.SetUpToolTip(
                _description: description,
                _delay: 0.15f
            );

            SetUpBars();
        }
        private void SetUpBars()
        {
            int2 sliderRanges = GetSliderRanges();

            if(totalBonus>0)
            {
                baseValueBar.minValue = sliderRanges.x;
                baseValueBar.maxValue = sliderRanges.y;
                baseValueBar.value = amount;

                bonusValueBar.minValue = sliderRanges.x;
                bonusValueBar.maxValue = sliderRanges.y;
                bonusValueBar.value = amount + totalBonus;
            }
            else
            {
                baseValueBar.minValue = sliderRanges.x;
                baseValueBar.maxValue = sliderRanges.y;
                baseValueBar.value = amount + totalBonus;

                bonusValueBar.minValue = sliderRanges.x;
                bonusValueBar.maxValue = sliderRanges.y;
                bonusValueBar.value = amount;
            }
            bonusValueBar.fillRect.GetComponent<Image>().color = totalBonus switch
            {
                int n when n < 0 => (Color)ColorData.HexToRgba(ColorData.Error),
                int n when n > 0 => (Color)ColorData.HexToRgba(ColorData.Green),
                _ => (Color)ColorData.HexToRgba(ColorData.Primary),
            };
        }
        private int2 GetSliderRanges()
        {
            switch(unitStat)
            {
                case UnitStat.MeleeAttack:
                    return new int2(0, 100);
                case UnitStat.MeleeDefense:
                    return new int2(0, 100);
                case UnitStat.WeaponStrength:
                    return new int2(0, 50);
                case UnitStat.Speed:
                    return new int2(0, 100);
                case UnitStat.Armor:
                    return new int2(0, 150);
                case UnitStat.Range:
                    return new int2(0, 200);
                case UnitStat.Accuracy:
                    return new int2(0, 100);
                case UnitStat.MissileStrength:
                    return new int2(0, 50);
                case UnitStat.ChargeBonus:
                    return new int2(0, 100);
                case UnitStat.Leadership:
                    return new int2(0, 100);
                case UnitStat.Ammunition:
                    return new int2(0, 1000);
                case UnitStat.ChargeImpactDamage:
                    return new int2(0, 100);
                default:
                    return new int2(0, 0);
            }
        }
    }
}
