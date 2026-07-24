using UnityEngine;
using System.Collections.Generic;

public struct CardPackData
{
    public int packPrice;
    public string packName;
    public string packDescription;
    public int packID;
}
public static class CardPackDataInfo
{
    // Keyed by packID, matching how the game already treats packs internally (CardPack's own
    // switch on cardPackData.packID, TabletopTavernData.GetSquadsToRecruitForPack).
    private static readonly Dictionary<int, int> PriceOverrides = new();

    public static void ClearEconomyOverrides() => PriceOverrides.Clear();
    public static void SetPriceOverride(int packID, int price) => PriceOverrides[packID] = price;

    private static CardPackData Build(int packID, int defaultPrice, string packName, string packDescription) => new()
    {
        packPrice = PriceOverrides.TryGetValue(packID, out int overridePrice) ? overridePrice : defaultPrice,
        packName = packName,
        packDescription = packDescription,
        packID = packID,
    };

    // Was `readonly static` fields - now computed properties so overrides can apply per access.
    // Field-access syntax (CardPackDataInfo.GearPack.packPrice) at every call site is unchanged;
    // C# doesn't distinguish field vs. property access syntax.
    public static CardPackData GearPack => Build(0, 7, "GearPack", "GearPackDescription");
    public static CardPackData CardPack1 => Build(1, 10, "CardPack1", "CardPack1Description");
    public static CardPackData CardPack2 => Build(2, 40, "CardPack2", "CardPack2Description");
    public static CardPackData CardPack3 => Build(3, 80, "CardPack3", "CardPack3Description");
    public static CardPackData CardPack4 => Build(4, 100, "CardPack4", "CardPack4Description");
}