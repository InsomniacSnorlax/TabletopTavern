using System.Collections.Generic;

// Lean, localization-free rule evaluation safe to call from the Components/Systems assemblies
// (Burst-free ECS systems), which cannot reference HeroBonusManager or TabletopTavernData - both
// live in the main TabletopTavern.Core assembly, which Systems does not reference. Takes SquadStats
// and Race as parameters instead of looking them up itself, since callers already have them
// on hand via the DOTS blob / CampaignSaveDataHolder singleton.
//
// HeroBonusManager (main assembly) owns loading rules from Resources + mod overrides and shares
// the same List<T> instances here via SetRules, so both the display layer and this mechanical
// layer always read identical data - see HeroBonusManager.LoadRulesFromResourcesAndOverrides.
public static class HeroBonusRuleEvaluator
{
    private static List<HeroStatBonusRule> _statRules;
    private static List<FactionBonusRule> _factionRules;

    public static void SetRules(List<HeroStatBonusRule> statRules, List<FactionBonusRule> factionRules)
    {
        _statRules = statRules;
        _factionRules = factionRules;
    }

    public static float SumHeroStatBonus(UnitStat stat, UnitName requestingUnit, int activeHeroID, SquadStats stats, Race enemyRace, float currentValue)
    {
        if (activeHeroID == -1 || _statRules == null) return 0f;

        float total = 0f;
        foreach (var rule in _statRules)
        {
            if (rule.HeroID != activeHeroID || rule.Stat != stat) continue;
            if (!rule.Condition.Matches(requestingUnit, stats, enemyRace)) continue;
            total += rule.MagnitudeKind == BonusMagnitudeKind.PercentOfCurrentValue ? currentValue * rule.Value : rule.Value;
        }
        return total;
    }

    public static float SumFactionStatBonus(UnitStat stat, Race race, float currentValue)
    {
        if (_factionRules == null) return 0f;

        float total = 0f;
        foreach (var rule in _factionRules)
        {
            if (rule.Race != race || rule.Stat != stat) continue;
            total += rule.MagnitudeKind == BonusMagnitudeKind.PercentOfCurrentValue ? currentValue * rule.Value : rule.Value;
        }
        return total;
    }
}
