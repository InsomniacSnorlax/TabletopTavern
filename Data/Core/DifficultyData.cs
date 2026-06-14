using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

namespace TJ
{
//DifficultyMod use this to find them all in the project
public enum TT_Difficulty { 
    Peasant = 1, 
    Squire = 2, // 1,2
    Knight = 3, // 3,4
    Baron = 4, // 5,6
    Duke = 5, // 7,8
    King = 6, // 9,10
    Emperor = 7, // 11,12
    Imperator = 8, // 13,14
    Overlord = 9, // 15,16
    Godking = 10, // 17,18
}
public struct DifficultyLevel 
{
    public TT_Difficulty difficulty;
    public string difficultyName;
    public string[] difficultyModifiers;
}
public static class DifficultyData
{
    public static DifficultyLevel GetDifficultyLevelData(TT_Difficulty difficulty)
    {
        switch (difficulty)
        {
            case TT_Difficulty.Peasant://1,2
                return Peasant;
            case TT_Difficulty.Squire://3,4
                return Squire;
            case TT_Difficulty.Knight://5,6
                return Knight;
            case TT_Difficulty.Baron://7,8
                return Baron;
            case TT_Difficulty.Duke://9,10
                return Duke;
            case TT_Difficulty.King://11,12
                return King;
            case TT_Difficulty.Emperor://13,14
                return Emperor;
            case TT_Difficulty.Imperator://15,16
                return Imperator;
            case TT_Difficulty.Overlord://17,18
                return Overlord;
            case TT_Difficulty.Godking://19,20
                return Godking;
            default:
                Debug.LogError($"Difficulty level {difficulty} not found. Defaulting to Peasant.");
                return Peasant; // Default to Peasant if none is found
        }
    }
    public static List<string> GetAllDifficultyModifiersBeforeLevel(TT_Difficulty difficulty)
    {
        List<string> modifiers = new List<string>();
        for (int i = 2; i < (int)difficulty; i++)
        {
            DifficultyLevel levelData = GetDifficultyLevelData((TT_Difficulty)i);
            modifiers.AddRange(levelData.difficultyModifiers);
        }
        return modifiers;
    }

    //1 ""//DifficultyMod 1
    //2 ""//DifficultyMod 2

    //3 "Auto-resolve health lost preview disabled"
    //4 "All shop prices increased by 2 <sprite name=GoldSprite>."

    //5 "Gear Chests in the shop are more expensive."
    //6 "The Final Battle of each Act is more difficult."

    //7 "Stronger enemy armies"
    //8 "Removes the ability to modify rolls in events."

    //9 "Increases the cost to recruit from towns"
    //10 "Increases the rarity of signature units in recruitment."

    //11 "Reduced heal on entering cities"
    //12 "Lose 1 gold <sprite name=GoldSprite> per turn."

    //13 "Recieve no gold for selling gear or consumables."
    //14 "Reduced gold from sacking cities"

    //15 "Reduces gold rewards from ransoming captives."
    //16 "Cities have stronger garrisons."

    //17 "Reserves healing reduced by half."
    //18 "Start each run with a weakened army."

    //19 "Enemy armies scale in strength faster."
    //20 "Auto-resolve disabled"

    public static DifficultyLevel Peasant = new ()
    {
        difficulty = TT_Difficulty.Peasant,
        difficultyName = "difficultyName1",
        difficultyModifiers = new string[] { "difficultyModifier1", "difficultyModifier2" },
    };
    public static DifficultyLevel Squire = new ()
    {
        difficulty = TT_Difficulty.Squire,
        difficultyName = "difficultyName2",
        difficultyModifiers = new string[] { "difficultyModifier3", "difficultyModifier4" },
    };
    public static DifficultyLevel Knight = new ()
    {
        difficulty = TT_Difficulty.Knight,
        difficultyName = "difficultyName3",
        difficultyModifiers = new string[] { "difficultyModifier5", "difficultyModifier6" },
    };
    public static DifficultyLevel Baron = new ()
    {
        difficulty = TT_Difficulty.Baron,
        difficultyName = "difficultyName4",
        difficultyModifiers = new string[] { "difficultyModifier7", "difficultyModifier8" },
    };
    public static DifficultyLevel Duke = new ()
    {
        difficulty = TT_Difficulty.Duke,
        difficultyName = "difficultyName5",
        difficultyModifiers = new string[] { "difficultyModifier9", "difficultyModifier10" },
        
    };
    public static DifficultyLevel King = new ()
    {
        difficulty = TT_Difficulty.King,
        difficultyName = "difficultyName6",
        difficultyModifiers = new string[] { "difficultyModifier11", "difficultyModifier12" },
    };
    public static DifficultyLevel Emperor = new ()
    {
        difficulty = TT_Difficulty.Emperor,
        difficultyName = "difficultyName7",
        difficultyModifiers = new string[] { "difficultyModifier13", "difficultyModifier14" },
    };
    public static DifficultyLevel Imperator = new ()
    {
        difficulty = TT_Difficulty.Imperator,
        difficultyName = "difficultyName8",
        difficultyModifiers = new string[] { "difficultyModifier15", "difficultyModifier16" },
    };
    public static DifficultyLevel Overlord = new ()
    {
        difficulty = TT_Difficulty.Overlord,
        difficultyName = "difficultyName9",
        difficultyModifiers = new string[] { "difficultyModifier17", "difficultyModifier18" },
    };
    public static DifficultyLevel Godking = new ()
    {
        difficulty = TT_Difficulty.Godking,
        difficultyName = "difficultyName10",
        difficultyModifiers = new string[] { "difficultyModifier19", "difficultyModifier20" },
    };
}
}