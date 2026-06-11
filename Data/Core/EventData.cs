using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

namespace TJ
{

public enum EventRollOutcome { CriticalFailure, Failure, Success, CriticalSuccess }
public enum EventOutcomeModifierEnum { None, Gold, GearDrop, UnitHealth, PrestigeUnit, NewUnit, ConsumableDrop } //Reputation

[System.Serializable] public struct TT_Event
{
    public string EventName;
    public string EventDescription;
    public EventChoice[] EventChoices;
    public EventReward[] EventRewards;
}
[System.Serializable] public struct EventChoice
{
    public string eventChoiceTitle;
    public string eventChoiceDescription;
    public int minimumRollNeeded; // 1-20
    public int ArmySizeRequired;
    public int GoldRequired;
    public EventOutcome successOutcome;
    public EventOutcome failureOutcome;
    public EventOutcome criticalFailureOutcome;
    public EventOutcome criticalSuccessOutcome;

}
[System.Serializable] public struct EventOutcome
{
    public string OutcomeDescription;
    public List<EventOutcomeModifier> EventOutcomeModifiers;
}
[System.Serializable] public struct EventOutcomeModifier
{
    public float Value;
    public EventOutcomeModifierEnum EventOutcomeModifierEnum;
}
[System.Serializable] public struct EventReward
{
    public string EventRewardTitle;
    public EventOutcome EventOutcome;
    public int EventIndex;
}

public static class EventData
{
    public static string EventOutcomeModifierToString(EventOutcomeModifierEnum _eventOutcomeModifier)
    {
        return _eventOutcomeModifier switch {
            EventOutcomeModifierEnum.None => "None",
            // EventOutcomeModifierEnum.Reputation => "Reputation",
            EventOutcomeModifierEnum.Gold => "Gold",
            EventOutcomeModifierEnum.UnitHealth => "Unit Health",
            EventOutcomeModifierEnum.GearDrop => "Gear",
            EventOutcomeModifierEnum.ConsumableDrop => "Consumable",
            _ => "Failed to load icon"
        };
    }
    public static TT_Event ThePlaguedVillage = new ()
    {
        EventName = "The Plagued Village",
        EventDescription = "You come across a village that has been ravaged by a terrible plague. Your cleric warns that the disease is highly contagious, and intervention might spread it to your ranks.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Burn them all",
                eventChoiceDescription = "Burn the village to prevent the spread of disease",
                minimumRollNeeded = 5,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You burn the village to the ground, preventing the spread of disease and saving neighboring areas.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You burn the village to the ground, but the disease has already spread to neighboring areas.",
                    EventOutcomeModifiers = new () {
                       
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "psych, thats the wrong village",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                    
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You burn the village to the ground, preventing a catastrophic outbreak of disease. Might as well take their stuff, they won't be needing it.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Quarantine",
                eventChoiceDescription = "Enforce a blockade around the village",
                minimumRollNeeded = 10,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The Quarantine is effective, and the disease is contained.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The Quarantine fails as the villagers were obsessed with \"muh freedoms\". The disease spreads to your troops.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The Quarantine fails and the disease spreads to your troops. What was the point locking them in there with it?",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The Quarantine is effective, after saving the village, the peasants all chip in to thank you with a small purse of coins.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 10f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Provide Aid",
                eventChoiceDescription = "Attempt to treat the survivors, risking your mercenaries' health.",
                minimumRollNeeded = 16,
                successOutcome = new () {
                    OutcomeDescription = "Your efforts to treat the sick are successful, the peasants all chip in to thank you with a small purse of coins.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 75f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new () {
                    OutcomeDescription = "Your efforts to treat the sick are unsuccessful, and the disease spreads. You tried so hard and got so far, but in the end, it doesn't even matter.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new () {
                    OutcomeDescription = "Your efforts to treat the sick are unsuccessful, and the disease spreads",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new () {
                    OutcomeDescription = "Your efforts to treat the sick are successful! As a gesture of gratitude you are offered an artifact worshiped by the peasants.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 75f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Avoid",
                eventChoiceDescription = "Avoid that place like the plague",
                minimumRollNeeded = 3,
                successOutcome = new () {
                    OutcomeDescription = "Crises averted, you move on, leaving the village to its fate and sparing your men.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.4f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                failureOutcome = new () {
                    OutcomeDescription = "Crises averted, you move on, but tales of the village's plight spread, and your reputation suffers.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new () {
                    OutcomeDescription = "You attempt to avoid the village, but your troops are already infected. The disease spreads.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new () {
                    OutcomeDescription = "You march past the village and make it to the next town by nightfall. Your troops are grateful for your leadership and caution.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.4f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        }
                    }
                },
            },
        }
    };
    public static TT_Event TheWitchesBargain = new ()
    {
        EventName = "The Witches Bargain",
        EventDescription = "A witch offers you a powerful item, but only if you perform a dark deed for her. Her eyes, dark and unyielding, watch for your reaction, as the weight of your choice hangs heavy in the air of her shadowy, herb-scented lair.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Sacrifice",
                eventChoiceDescription = "Sacrifice a young maiden",
                minimumRollNeeded = 12,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The sceam of the maiden echoes through the night, but the witch is pleased, and rewards you with a powerful artifact.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops, unable to sleep with the screams of the maiden echoing through the night, waiver in their loyalty.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    },
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The maiden was a powerful sorceress, and her death has cursed your troops.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    },
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Your buring of the maiden remains a secret, and the witch rewards you with a powerful artifact.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }

                },
            },
            new () {
                eventChoiceTitle = "Turn away",
                eventChoiceDescription = "Send the witch away",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Much to the relief of your troops, you send the witch away, but you can't shake the feeling that you've missed out on something important.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The witch leaves, but you can't shake the feeling that you've missed out on something important.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The witch leaves disappointed and starts spreading rumors about your cowardice on Twitter.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You send her away without hesitation, but not before confiscating her potions.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.ConsumableDrop
                        },
                        new () {
                            Value = 0.20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Hand off",
                eventChoiceDescription = "Request your loyal squire to perform the dark deed in secret",
                ArmySizeRequired = 8,
                minimumRollNeeded = 7,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your squire handles the matter discretly, but the witch is pleased, and rewards you with a powerful artifact.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The squire tries his best, but the screams of the maiden echo through the night. The witch holds to the bargain, but your reputation suffers.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your squire balks at the task, and deserts along with some of your troops.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "After publicly denouncing the action, you secretly call on your squire. The buring of the maiden remains a secret, and the witch rewards you with a powerful artifact.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            },
        }
    };
    public static TT_Event TheHauntedChapel = new ()
    {
        EventName = "The Haunted Chapel",
        EventDescription = "Your company stumbles upon a crumbling chapel at dusk. Strange lights flicker from within, and an eerie hymn echoes across the fields.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Investigate",
                eventChoiceDescription = "Investigate the Chapel",
                minimumRollNeeded = 13,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your bravery is rewarded. The chapel was empty, except for the gold your troops recovered from beneath the altar.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "A cold wind blows through the chapel, Your troops are uneasy and demand to leave.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "A cold wind blows through the chapel, Your troops are uneasy and begin to desert",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Your bravery is rewarded. The chapel was empty, except for the loot your troops recovered from beneath the altar.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Hell no",
                eventChoiceDescription = "Attempt to exorcise the chapel by burning it to the ground",
                minimumRollNeeded = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The chapel is cleansed, and the ghost of the sorcerer is put to rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.4f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The chapel is cleansed, but the fire spreads to nearby fields which the villagers rely on for food.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops managed to set your own camp on fire. Idk how they did it, but they did.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The flames burn green and the screams of the sorcerer echo through the night. The chapel is cleansed, and the the vilagers reward you for your bravery and the fireworks show.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.4f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 30f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Avoid",
                eventChoiceDescription = "Avoid the chapel",
                minimumRollNeeded = 5,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You move on, your just a chill guy and dont want to deal with that nonsense.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.4f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You move on, but your troops grumble about lack of rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Without somewhere to sleep, your troops are tired and grumpy. There is talk about mutiny.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You avoid the chapel and make it to the next town by nightfall. Your troops are grateful for your leadership and caution.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Hire an Exocist",
                eventChoiceDescription = "Summon an exorcist to cleanse the chapel",
                minimumRollNeeded = 10,
                GoldRequired = 3,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The priest cleanses the chapel, and the ghost of the sorcerer is put to rest. Your troops can now rest easy.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The priest utters words, but nothing happens. The ghost of the sorcerer haunts your troops dreams, it's a long, sleepless night.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The priest fails to cleanse the chapel, and your dreams are haunted by the ghost of the sorcerer and your advisors saying 'Told you it wouldn't work'.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The priest exorcises the chapel, and the ghost of the sorcerer is put to rest. Confident in your leadership, your troops are ready for the next challenge.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            }
        }
    };
    public static TT_Event TheMerchantsDilemma = new ()
    {
        EventName = "The Merchant's Dilemma",
        EventDescription = "A panicked merchant flags you down, claiming bandits have stolen his goods. He offers gold if you retrieve them but cannot guarantee their exact location. Your troops are tired, and the sun is setting.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Hunt them down",
                eventChoiceDescription = "Send out a sortie to find the bandits",
                minimumRollNeeded = 13,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You find the bandits and retrieve the goods, the merchant rewards you with a small purse of coins.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You find the bandits, but they missing goods amount to little. What a waste of time.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 6f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You find the bandits, but they are well armed, and your troops are forced to retreat.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You find the bandits and retrieve the goods, the merchant rewards you with a small purse of coins and a powerful artifact.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Ignore",
                eventChoiceDescription = "Ignore the merchant",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You move on without heeding the merchant's pleas, arriving at a town by nightfall and allowing your troops time to rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You ignore him, but your advisors question your leadership, and your reputation suffers.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "As you turn your back, the merchant mutters a curse under his breath. Wah wah wee wah.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You move on without heeding the merchant's pleas, and arrive at a town by nightfall, giving your troops a much needed rest. They are greatful for your leadership.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Rob",
                eventChoiceDescription = "Rob the merchant",
                minimumRollNeeded = 7,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You rob the merchant and take the small amount still on him. Your troops look negatively on your actions.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You pointlessly rob the merchant but find nothing of value.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        },
                        new () {
                            Value = -0.05f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to rob the merchant, but your troops disobey and are astounded at your cruelty, causing some to walk away from your cause.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You rob the merchant, and find a powerful artifact in his possession.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        },
                    }
                },
            },
        }
    };
    public static TT_Event TheRogueCompany = new ()
    {
        EventName = "The Rogue Company",
        EventDescription = "While camping, your scouts report troops stalking your position. They carry no banners, and their intentions are unclear.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Sound the alarm",
                eventChoiceDescription = "Prepare for an ambush",
                minimumRollNeeded = 6,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops form a shield wall, and the unknown enemy slinks off into the night in search of easier prey. Your troops are emboldened by your leadership.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops form a shield wall and stand in it for hours, but the enemy never attacks. Their lack of sleep will be felt in the morning.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to raise the alarm, but the enemy is already upon you. Your troops are take heavy casualties.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You form a shield wall, and the ragged enemy attack is eaisly repelled. You look over the corpses of your enemies, covered in armor. \"It's free real estate\" you whisper.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Flee",
                eventChoiceDescription = "Get your troops marching and try to lose the enemy",
                minimumRollNeeded = 13,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You march through the night, and the enemy is left behind. Your troops are tired, but grateful to be alive.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to march through the night, but the enemy tails you, picking off stragglers. Your troops are forced to fight in disarray.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your attempt to flee is cut short by the sound of arrows in the night. Your troops are forced to fight in disarray.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You march through the night, and the enemy is left behind and you make it to a small glade in which to rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Parley",
                eventChoiceDescription = "Send a diplomat to parley with the enemy",
                minimumRollNeeded = 9,
                ArmySizeRequired = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The enemy is revealed to be a group of refugees from a town sacked by goblins. They offer their services in exchange for food and shelter.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The enemy is revealed to be a group of deserters, they are willing to let you go but they demand a toll for safe passage.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The unknown enemy slaughters your diplomat, and your troops are forced to fight in disarray.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The enemy is revealed to be refugees from a town sacked by goblins. The offer, no demand, thay they assist you in your quest to rid the land of goblinkind.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Ambush",
                eventChoiceDescription = "Set up an ambush for the enemy",
                minimumRollNeeded = 15,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You set up an ambush, and the enemy is caught off guard. After a brief skirmish leaving them dead, your troops take to their spoils.", 
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 18f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to set up an ambush, but the enemy moves too fast. Your troops take casulties and are forced to retreat.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to set up an ambush, but the enemy is too strong. Your troops take heavy casulties and are forced to retreat.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.35f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You set up an ambush for the ambushers, catching them off guard. Your troops gain valuable experience in the art of war.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 18f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        }
                    }
                },
            },
            // new () {
            //     eventChoiceTitle = "Offer Tribute",
            //     eventChoiceDescription = "Offer the enemy a little gold to leave you in peace",
            //     minimumRollNeeded = 6,
            //     GoldRequired = 4,
            //     successOutcome = new EventOutcome () {
            //         OutcomeDescription = "You offer the chest of gold to the enemy, and they accept it. Some of their men see fortune in their future and join you on your quest.",
            //         EventOutcomeModifiers = new () {
            //             new () {
            //                 Value = 1f,
            //                 EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
            //             },
            //         }
            //     },
            //     failureOutcome = new EventOutcome () {
            //         OutcomeDescription = "You offer the chest of gold to the enemy, and they accept it. They leave you in peace, and your troops are grateful for your leadership.",
            //         EventOutcomeModifiers = new () {
            //             new () {
            //                 EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
            //             },
            //         }
            //     },
            //     criticalFailureOutcome = new EventOutcome () {
            //         OutcomeDescription = "You offer the chest of gold to the enemy, but they demand more. Your troops are forced to fight in disarray.",
            //         EventOutcomeModifiers = new () {
            //             new () {
            //                 Value = -0.35f,
            //                 EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
            //             }
            //         }
            //     },
            //     criticalSuccessOutcome = new EventOutcome () {
            //         OutcomeDescription = "You offer the chest of gold to the enemy, and they accept it. The men seeing fortune in their future join you on your quest.",
            //         EventOutcomeModifiers = new () {
            //             new () {
            //                 Value = 1f,
            //                 EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
            //             },
            //             new () {
            //                 Value = 1f,
            //                 EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
            //             }
            //         }
            //     },
            // },
        }
    };
    public static TT_Event ATroublingDream = new ()
    {
        EventName = "A Troubling Dream",
        EventDescription = "One of your mercenaries recounts a disturbing dream of a great beast stalking the camp. The air feels heavy with tension.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Ignore",
                eventChoiceDescription = "Ignore the dream",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You ignore the dream, and your troops rest as the night passes uneventfully.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You ignore the dream, but the tension in the camp is palpable. Your troops are on edge, and little sleep is had.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You ignore the dream, and in the dead of night, the beast strikes. Your troops are caught off guard and take heavy casualties.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You ignore the dream, and the night passes uneventfully. Your troops are grateful for your leadership and their well earned rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
            },
            new () {
                eventChoiceTitle = "Double the Watch",
                eventChoiceDescription = "Double the watch for the night",
                minimumRollNeeded = 6,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops on high alert spot a large shadow creeping around the camp. They cry out and wave torches, causing the unknown beast to slink off into the night. Your paranoia may have saved countless lives.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops are on high alert, and although the night passes uneventfully, the lack of sleep reduces your men's strength.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops are on high alert, but the night passes uneventfully. The lack of sleep reduces your men's strength and they grumble about your suspicious nature.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "A howl cuts through the night, but your men stand back to back, weapons ready. The great beast meets an untimely end, and your troops are grateful for your caution.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Consult the Oracle",
                eventChoiceDescription = "Consult the Oracle for guidance",
                minimumRollNeeded = 11,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The Oracle informs you that the dream is \"Fake News\" from \"Big Monster Media\" and shouldn't be trusted. Your troops rest easy through the night.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The Oracle informs you that the dream is \"Fake News\" from \"Big Monster Media\" and shouldn't be trusted. Which was true until it wasnt. The beast strikes in the night.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The Oracle informs you that the dream is \"Fake News\" from \"Big Monster Media\" and shouldn't be trusted. Which was true until it wasn\'t. The beast strikes in the night, and your troops are caught off guard.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The Oracle informs you that the dream actually describes a roving band of mercenaries. You keep your torches lit in the hopes that they will find you, and they do. The mercenaries offer their services in exchange for food and shelter.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
            },
        }
    };
    public static TT_Event TheStrandedKnight = new ()
    {
        EventName = "The Stranded Knight",
        EventDescription = "A knight in shining armor approaches your camp, his horse panting in exhaustion. He explains that he was separated from his company and is in need of assistance.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Offer Assistance",
                eventChoiceDescription = "Offer to reunite the knight with his company",
                minimumRollNeeded = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You offer the night safe passage with your company, on the following day you find his company and reunite them. The knight is grateful for your assistance, and offers to join your company.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 8f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is grateful for your assistance, but unfortunately, you find his company stuffed with goblin arrows. He is the only survivor.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "When searching for his company you are ambushed by goblins. The knight is killed in the ensuing battle and your troops are forced to retreat.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "On the following day you find his company and reunite them. The knight is grateful for your assistance, and their offer their service. As a token of gratitude, the knight also offers you a chest of gold.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Turn Away",
                eventChoiceDescription = "Turn the knight away",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is disappointed but understands. He rides off into the night, and you hear no more of him.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is disappointed and rides off into the night. You hear no more of him, but your troops grumble about your lack of compassion.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is disappointed and rides off into the night. He returns the following morning with a band of mercenaries and attacks your camp.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is disappointed but understands. He rides off into the night, and you hear no more of him. Your troops are grateful to you for allways putting the company first.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Demand Payment Upfront",
                eventChoiceDescription = "Offer to reunite the knight with his company, for a fee",
                minimumRollNeeded = 11,
                ArmySizeRequired = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You offer the night safe passage with your experienced company, and on the following day you find his company and reunite them. The knight is grateful for your assistance, and offers to join your company.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "The knight is grateful for your assistance, but unfortunately, you find his company stuffed with goblin arrows. He is the only survivor.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "When searching for his company you are ambushed by goblins. The knight is killed in the ensuing battle and your troops are forced to retreat.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    },
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "On the following day you find his company and reunite them. The knight is grateful for your assistance, and their offer their service. As a token of gratitude, the knight also offers you a chest of gold.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 15f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
            },
        }
    };
    public static TT_Event TheGreatBeast = new ()
    {
        EventName = "The Great Beast",
        EventDescription = "Villagers plead for help slaying a rampaging monster threatening their homes.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Hunt the Beast",
                eventChoiceDescription = "Accept their plea and attempt to slay the beast",
                minimumRollNeeded = 14,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops slay the beast and gain valuable experience. The villagers reward you with a chest of gold.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to slay the beast, but the battle is hard fought. The villagers reward you a meager amount of gold, but was it worth the sacrifice?.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to slay the beast, but the battle is hard fought with many lying slain. The villagers reward you with a gold, but was it worth the sacrifice?.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops slay the beast and gain valuable experience. The villagers reward you with a chest of gold and your legend grows.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 20f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Ignore Their Plea",
                eventChoiceDescription = "Decline and continue",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the villagers' plea and allow your troops to rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "Although you decline the villagers' plea and continue on your journey, the beast attacks regardless. Your troops fend it off, but take casualties.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the villagers' plea and continue on your journey, but the beast attacks regardless. Your troops are caught off guard and take heavy casualties.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the villagers' plea and continue on your journey only to be attacked by the beast. Your troops are ready, and they slay the beast and gain valuable experience.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Demand Payment up front",
                eventChoiceDescription = "Accept their plea and attempt to slay the beast, but gold comes first",
                minimumRollNeeded = 10,
                ArmySizeRequired = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops slay the beast and gain valuable experience. The villagers reward you with a chest of gold.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to slay the beast, but the battle is hard fought. The villagers reward you with a gold, but was it worth the sacrifice?.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 8f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to slay the beast, but the battle is hard fought with many lying slain. The villagers reward you with a meager amount gold, but was it worth the sacrifice?.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                        new () {
                            Value = 2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops slay the beast and gain valuable experience. The villagers reward you with a chest of gold and your legend grows.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 25f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                    }
                },
            },
        }
    };
    public static TT_Event TheGatheringOfChieftains = new ()
    {
        EventName = "The Gathering of Chieftains",
        EventDescription = "A group of chieftains have gathered to discuss the growing threat of the goblin hordes. They request your presence.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Attend as one of them",
                eventChoiceDescription = "Attend the gathering and offer your assistance",
                minimumRollNeeded = 13,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance. The chieftains are grateful for your support and offer to assist you in your quest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance, but the chieftains are unimpressed with your insight. They ignore your offer and you leave empty handed.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        },
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance, but the chieftains are unimpressed with your insight and fee you for your outburst. They ignore your offer and you leave empty handed. Your reputation greatly suffers.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance. The chieftains are grateful for your support and offer to assist you in your quest. They also offer you a powerful artifact in the hopes that you can use it to bring an end to their plight.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Decline the Invitation",
                eventChoiceDescription = "Decline the invitation and continue on your journey",
                minimumRollNeeded = 4,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the invitation and continue on your journey.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the invitation and continue on your journey. The chieftains are insulted by your lack of respect.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the invitation and continue on your journey. The chieftains are insulted by your lack of respect and some of your troops desert to join their noble cause.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    },
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Actions speak louder than words. You decline the invitation and continue on your journey. Some of the chieftains are impressed by your resolve and offer to assist you in your quest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Flex Your Authority",
                eventChoiceDescription = "Demand their assistance in your campaign",
                minimumRollNeeded = 8,
                ArmySizeRequired = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and demand they follow you. The chieftains are grateful for your support and offer to assist you in your quest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 17f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and demand they follow you, but the chieftains are unimpressed with your insight. They ignore your offer, but some of their followers are impressed by your resolve and offer to join your company.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance, but the chieftains are unimpressed with your strategy. They ignore your offer and you leave empty handed. Your reputation greatly suffers.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -3f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance. The chieftains are grateful for your support and offer to assist you in your quest. They also offer you a powerful artifact in the hopes that you can use it to bring an end to their plight.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 17f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.GearDrop
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Bribe the Chieftains",
                eventChoiceDescription = "Attend the gathering with a chest of gold",
                minimumRollNeeded = 7,
                GoldRequired = 5,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer payment to all those willing to join your cause. The coins glint in the firelight, and the chieftains are impressed by your generosity.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance, but the chieftains are unimpressed with your gifts.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        },
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer your assistance, but the chieftains are unimpressed with your gifts. They ignore your offer and you leave empty handed. Your reputation and wallet greatly suffer.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.Gold
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You attend the gathering and offer payment to all those willing to join your cause. The coins glint in the firelight, and the chieftains are impressed by your generosity. Many men leap at the chance to join your company.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 10f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 10f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
            },
        }
    };
    public static TT_Event SchoolOfTheWolf = new ()
    {
        EventName = "School of the Wolf",
        EventDescription = "A group of monster hunters from the School of the Wolf have been spotted in the area. They offer to train your troops in the art of combat.",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Accept their Offer",
                eventChoiceDescription = "Accept the monster hunters' offer and train your troops",
                minimumRollNeeded = 3,
                GoldRequired = 2,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops train with the monster hunters and gain valuable experience. The monster hunters are impressed with your troops' progress and offer to join your company.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops train with the monster hunters, but the training is too intense. Some even die in the process.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.2f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops train with the monster hunters, but the training is too intense. Many die in the process, and they question your leadership.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "Your troops train with the monster hunters and gain valuable experience. They come out of the training stronger and more confident.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Decline their Offer",
                eventChoiceDescription = "Decline the monster hunters' offer and continue on your journey",
                minimumRollNeeded = 5,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the monster hunters' offer and continue on your journey.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the monster hunters' offer and continue on your journey. Your lack of vision is noted by your men.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the monster hunters' offer and continue on your journey. Your lack of vision is noted by your men, and some desert to find a leader who will take the fight to the enemy.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    },
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You decline the monster hunters' offer and instead let your troop rest and recover. They are grateful for the respite and come out of it stronger and more confident.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 0.5f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        }
                    }
                },
            },
            new () {
                eventChoiceTitle = "Enlist them as Mercenaries",
                eventChoiceDescription = "Enlist the monster hunters as mercenaries",
                minimumRollNeeded = 10,
                ArmySizeRequired = 8,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You enlist the monster hunters as mercenaries and they join your company. Your troops are grateful for the additional support.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to enlist the monster hunters as mercenaries, but they refuse your offer. They are not interested in your cause.",
                    EventOutcomeModifiers = new () {
                        new () {
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.None
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "You attempt to enlist the monster hunters as mercenaries, but they refuse your offer. They are not interested in your cause, and are insulted by your bravado.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = -0.1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.UnitHealth
                        },
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You enlist the monster hunters as mercenaries and they join your company. Your troops are grateful for the additional support, and your reputation grows.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        },
                    }
                },
            }
        }
    };
    public static TT_Event TestEvent = new ()
    {
        EventName = "Testy",
        EventDescription = "Testy test test",
        EventChoices = new EventChoice[] {
            new () {
                eventChoiceTitle = "Test",
                eventChoiceDescription = "Get your troops marching and try to lose the enemy",
                minimumRollNeeded = 10,
                successOutcome = new EventOutcome () {
                    OutcomeDescription = "You march through the night, and the enemy is left behind. Your troops are tired, but grateful to be alive.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                failureOutcome = new EventOutcome () {
                    OutcomeDescription = "You march through the night. Your troops are tired but the enemy is still on your tail.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                criticalFailureOutcome = new EventOutcome () {
                    OutcomeDescription = "Your attempt to flee is cut short by the sound of arrows in the night. Your troops are forced to fight in disarray.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
                criticalSuccessOutcome = new EventOutcome () {
                    OutcomeDescription = "You march through the night, and the enemy is left behind. Your troops are tired, but safe. You find a small glade in which to rest.",
                    EventOutcomeModifiers = new () {
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.NewUnit
                        },
                        new () {
                            Value = 1f,
                            EventOutcomeModifierEnum = EventOutcomeModifierEnum.PrestigeUnit
                        }
                    }
                },
            },
        }
    };
    public static TT_Event[] GetAllEvents()
    {
        return new TT_Event[] { 
            // TestEvent,
            ThePlaguedVillage, 
            TheWitchesBargain,
            TheHauntedChapel,
            TheMerchantsDilemma,
            TheRogueCompany,
            ATroublingDream,
            TheStrandedKnight,
            TheGreatBeast,
            TheGatheringOfChieftains,
            SchoolOfTheWolf,
        };
    }
    public static List<int> GetEventOrdering(System.Random _random)
    {
        int eventCount = GetAllEvents().Length;
        List<int> eventOrdering = new ();
        for (int i = 0; i < eventCount; i++) {
            eventOrdering.Add(i);
        }
        for (int i = 0; i < eventCount; i++) {
            int randomIndex = _random.Next(0, eventCount);
                (eventOrdering[i], eventOrdering[randomIndex]) = (eventOrdering[randomIndex], eventOrdering[i]);
            }
        return eventOrdering;
    }
}
}
