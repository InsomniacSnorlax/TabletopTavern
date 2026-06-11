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
    public readonly static CardPackData GearPack = new ()
    {
        packPrice = 7,
        packName = "GearPack",
        packDescription = "GearPackDescription",
        packID = 0,
    };
    public readonly static CardPackData CardPack1 = new ()
    {
        packPrice = 10,
        packName = "CardPack1",
        packDescription = "CardPack1Description",
        packID = 1,
    };
    public readonly static CardPackData CardPack2 = new ()
    {
        packPrice = 40,
        packName = "CardPack2",
        packDescription = "CardPack2Description",
        packID = 2,
    };
    public readonly static CardPackData CardPack3 = new ()
    {
        packPrice = 80,
        packName = "CardPack3",
        packDescription = "CardPack3Description",
        packID = 3,
    };
    public readonly static CardPackData CardPack4 = new ()
    {
        packPrice = 75,
        packName = "CardPack4",
        packDescription = "CardPack4Description",
        packID = 4,
    };
}