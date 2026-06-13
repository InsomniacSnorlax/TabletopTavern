using System.Collections.Generic;

namespace TJ
{
    [System.Serializable]
    public struct Hero
    {
        public string HeroName;
        public string HeroDescription;
        public string[] HeroBonusDescription;
        public string HeroPrefabName;
        public UnlockCondition UnlockCondition;
        public UnlockCondition DemoUnlockCondition;
        public int StartingGold;
        public int HeroID;
        public Race Race;
        public UnitName SignatureUnit;
        public UnitName[] StartingArmyUnits;
    }
    public static class HeroData
    {
        public static Hero EdricValeward = new()
        {
            HeroName = "heroName1",
            HeroDescription = "heroDescription1",
            HeroBonusDescription = new string[] { "heroBonusDescription1", "heroBonusDescription2" }, //The Olde Guard: [Rare] units gain +10 [Leadership] and +4 [Melee Attack], //Take Back our Lands: All units gain +2 [Charge Bonus]
            HeroPrefabName = "EdricValeward",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.None,
            StartingGold = 10,
            HeroID = 1,
            Race = Race.IronLegion,
            SignatureUnit = UnitName.Ashguard,
            StartingArmyUnits = new UnitName[] { UnitName.LevySwordsmen, UnitName.LevySwordsmen, UnitName.PeasantBowmen, UnitName.FieldPikemen}
        };
        public static Hero RhydanGreythorne = new()
        {
            HeroName = "heroName2",
            HeroDescription = "heroDescription2",
            HeroBonusDescription = new string[] { "heroBonusDescription3", "heroBonusDescription4" },//Dúnedain Captain: Deepwood Rangers gain +10 [Accuracy] and +4 [Missile Strength], The Everyman: [Common] units gain +10 [Leadership] and +4 [Melee Defense]
            HeroPrefabName = "RhydanGreythorne",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.HeroCompletion,
            StartingGold = 10,
            HeroID = 2,
            Race = Race.IronLegion,
            SignatureUnit = UnitName.DeepwoodRangers,
            StartingArmyUnits = new UnitName[] { UnitName.LevySwordsmen, UnitName.LevySwordsmen, UnitName.PeasantBowmen, UnitName.PeasantBowmen }
        };
        public static Hero BoblinTheGoblinKing = new()
        {
            HeroName = "heroName3",
            HeroDescription = "heroDescription3",
            HeroBonusDescription = new string[] { "heroBonusDescription5", "heroBonusDescription6" },//Drums in the Deep: [Common] card packs cost reduced to 1 gold, Go forth my hordes: Goblins gain +20 [Leadership] and +4 [Melee Attack]
            HeroPrefabName = "BoblinTheGoblinKing",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 14,
            HeroID = 3,
            Race = Race.Gruntkin,
            SignatureUnit = UnitName.Siegeclaws,
            StartingArmyUnits = new UnitName[] { UnitName.GoblinRabble, UnitName.GoblinRabble, UnitName.GoblinRabble, UnitName.BogmawTroll }
        };
        public static Hero KragmukGorethirster = new()
        {
            HeroName = "heroName4",
            HeroDescription = "heroDescription4",
            HeroBonusDescription = new string[] { "heroBonusDescription7", "heroBonusDescription8" },//A taste for man-flesh: Orc Ravagers cause [Terror] and gain +10 [Melee Attack] and +4 [Weapon Strength], Burn them all: 2x gold from sacking cities
            HeroPrefabName = "KragmukGorethirster",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 15,
            HeroID = 4,
            Race = Race.Gruntkin,
            SignatureUnit = UnitName.Meatgrinders,
            StartingArmyUnits = new UnitName[] { UnitName.OrcRavagers, UnitName.OrcStalkers, UnitName.GoblinRabble }
        };
        public static Hero BjornIronskull = new()
        {
            HeroName = "heroName5",
            HeroDescription = "heroDescription5",
            HeroBonusDescription = new string[] { "heroBonusDescription9", "heroBonusDescription10" }, //Ironskin: Melee Infantry gain +4 [Melee Defense] and +10 [Armor], //The Skull Harvest: +2 Gold from battle rewards
            HeroPrefabName = "BjornIronskull",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.DiscordExclusive,
            StartingGold = 17,
            HeroID = 5,
            Race = Race.RavenHost,
            SignatureUnit = UnitName.Jomsvikings,
            StartingArmyUnits = new UnitName[] { UnitName.SeawindSpears, UnitName.Huskarls, UnitName.DriftwoodSkirmishers, UnitName.DriftwoodSkirmishers }
        };
        public static Hero FreyjaStormweaver = new()
        {
            HeroName = "heroName6",
            HeroDescription = "heroDescription6",
            HeroBonusDescription = new string[] { "heroBonusDescription11", "heroBonusDescription12" },//With me sisters!: Shieldmaiden units gain +10 [Leadership] and +4 [Melee Defense], The Rock of Trondheim: All units are immune to Terror
            HeroPrefabName = "FreyjaStormweaver",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NewsletterExclusive,
            StartingGold = 12,
            HeroID = 6,
            Race = Race.RavenHost,
            SignatureUnit = UnitName.AngelsOfDeath,
            StartingArmyUnits = new UnitName[] { UnitName.Shieldmaidens, UnitName.DriftwoodSkirmishers, UnitName.ThrallLevy }
        };
        public static Hero IltharionStarpire = new()
        {
            HeroName = "heroName7",
            HeroDescription = "heroDescription7",
            HeroBonusDescription = new string[] { "heroBonusDescription13", "heroBonusDescription14" },//Supernova of the West: All units gain +5 [Charge Bonus] and +4 [Melee Attack], //Purge the Blight: Optional Post Battle Choice - Exectue captives to prestige a random unit
            HeroPrefabName = "IltharionStarpire",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 12,
            HeroID = 7,
            Race = Race.TaelindorForest,
            SignatureUnit = UnitName.VeilkinDrakes,
            StartingArmyUnits = new UnitName[] { UnitName.AshwoodMilitia, UnitName.AshwoodMilitia, UnitName.AerindelGuard }
        };
        public static Hero SerendaelOfNytherial = new()
        {
            HeroName = "heroName8",
            HeroDescription = "heroDescription8",
            HeroBonusDescription = new string[] { "heroBonusDescription15", "heroBonusDescription16" },//The Light of Nytherial: Units recieve 2x Healing from all sources, //The Forest Walks: Forest Spirits and Treants gain +5 [Melee Defense] and +4 [Weapon Strength]
            HeroPrefabName = "SerendaelOfNytherial",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 13,
            HeroID = 8,
            Race = Race.TaelindorForest,
            SignatureUnit = UnitName.EmeraldAncient,
            StartingArmyUnits = new UnitName[] { UnitName.ForestSpirits, UnitName.ForestSpirits, UnitName.ForestSpirits, UnitName.Treants }
        };
        public static Hero SisterMorvayne = new()
        {
            HeroName = "heroName9",
            HeroDescription = "heroDescription9",
            HeroBonusDescription = new string[] { "heroBonusDescription17", "heroBonusDescription18" },//Forbidden Rituals - Optional Post Battle Choice: Gain a consumable of any rarity, //Unbound by Chivalry: All units gain the [Outrider] ability
            HeroPrefabName = "SisterMorvayne",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 15,
            HeroID = 9,
            Race = Race.SanguineCourt,
            SignatureUnit = UnitName.NecroticChimerae,
            StartingArmyUnits = new UnitName[] { UnitName.UndeadLevies, UnitName.MistWraiths, UnitName.BoneshardArchers }
        };
        public static Hero LordDravenBloodreaver = new()
        {
            HeroName = "heroName10",
            HeroDescription = "heroDescription10",
            HeroBonusDescription = new string[] { "heroBonusDescription19", "heroBonusDescription20" },//Bloodsworn Prince: Bloodsworn and Bloodsworn Knights gain +15 [Leadership] and +4 [Melee Attack], //Thirst for Blood: Sacking a city heals all units to full health
            HeroPrefabName = "LordDravenBloodreaver",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 14,
            HeroID = 10,
            Race = Race.SanguineCourt,
            SignatureUnit = UnitName.Shadelords,
            StartingArmyUnits = new UnitName[] { UnitName.BoneclatterSpears, UnitName.DeathhavenFiends, UnitName.BoneshardArchers }
        };
        //SakuraDynasty
        public static Hero OdaNobukage = new()
        {
            HeroName = "heroName11",
            HeroDescription = "heroDescription11",
            HeroBonusDescription = new string[] { "heroBonusDescription21", "heroBonusDescription22" },//Nagoya Steel: All units gain +4 [Weapon Strength], //Hour of Destiny - Optional Post Battle Choice: Lose All Gold <sprite name=GoldSprite> to gain a random unit of any rarity
            HeroPrefabName = "OdaNobukage",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 11,
            HeroID = 11,
            Race = Race.SakuraDynasty,
            SignatureUnit = UnitName.GoldenSaru,
            StartingArmyUnits = new UnitName[] { UnitName.DaikyuCommoners, UnitName.AshigaruSpearmen, UnitName.FootSamurai }
        };
        public static Hero TokugawaHarunobu = new()
        {
            HeroName = "heroName12",
            HeroDescription = "heroDescription12",
            HeroBonusDescription = new string[] { "heroBonusDescription23", "heroBonusDescription24" }, //Empire's Wealth: Earn 2x <sprite name=GoldSprite> from interest, //Innovator's Legacy: Emperors Fusiliers Gain +10 [Accuracy] and +4 [Missile Strength]
            HeroPrefabName = "TokugawaHarunobu",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 12,
            HeroID = 12,
            Race = Race.SakuraDynasty,
            SignatureUnit = UnitName.Oni,
            StartingArmyUnits = new UnitName[] { UnitName.RoninWanderers, UnitName.AshigaruSpearmen, UnitName.BanryuBombardiers }
        };
        //DeepstoneHold
        public static Hero HrothgarGoblinslayer = new()
        {
            HeroName = "heroName13",
            HeroDescription = "heroDescription13",
            HeroBonusDescription = new string[] { "heroBonusDescription25", "heroBonusDescription26" },//Ancestral Hatred: All units gain +10 [Melee Attack] and +4 [Weapon Strength] when fignting the Gruntkin, // Swarm Breaker: Cragflayers gain the [Rage] ability
            HeroPrefabName = "HrothgarGoblinslayer",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 10,
            HeroID = 13,
            Race = Race.DeepstoneHold,
            SignatureUnit = UnitName.AbyssalDelveknights,
            StartingArmyUnits = new UnitName[] { UnitName.Cragflayers, UnitName.RiftpickLaborers, UnitName.RiftpickLaborers }
        };
        public static Hero BerthaBarrelstorm = new()
        {
            HeroName = "heroName14",
            HeroDescription = "heroDescription14",
            HeroBonusDescription = new string[] { "heroBonusDescription27", "heroBonusDescription28" }, //Blasting Barrels: Artillery units gain +10 [Accuracy] and +4 [Missile Strength]. //Supply Lines: All ranged units gain 50% increased ammunition capacity.
            HeroPrefabName = "BerthaBarrelstorm",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 13,
            HeroID = 14,
            Race = Race.DeepstoneHold,
            SignatureUnit = UnitName.ForgewrathHammers,
            StartingArmyUnits = new UnitName[] { UnitName.RiftpickLaborers, UnitName.RiftpickLaborers, UnitName.RiftpickLaborers, UnitName.GrimfireGuns }
        };
        //DrakosaurBrood
        public static Hero SkrixTheSwarmcaller = new()
        {
            HeroName = "heroName15",
            HeroDescription = "heroDescription15",
            HeroBonusDescription = new string[] { "heroBonusDescription29", "heroBonusDescription30" }, //Kobold Kammandos: Kobold units gain +10 [Leadership] and +4 [Melee Attack], //Swarm Tactics: If your army contains 5 or more Kobold units, a random Kobold will prestige on turn end
            HeroPrefabName = "SkrixTheSwarmcaller",
            UnlockCondition = UnlockCondition.None,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 15,
            HeroID = 15,
            Race = Race.DrakosaurBrood,
            SignatureUnit = UnitName.ObsidianScales,
            StartingArmyUnits = new UnitName[] { UnitName.Brutes, UnitName.KoboldBrawlers, UnitName.KoboldBrawlers, UnitName.ScalebowKobolds, UnitName.ScalebowKobolds }
        };
        public static Hero ValthrexPrimeclaw = new()
        {
            HeroName = "heroName16",
            HeroDescription = "heroDescription16",
            HeroBonusDescription = new string[] { "heroBonusDescription31", "heroBonusDescription32" }, // Beastmaster: Large units gain +10 [Leadership] and +4 [Melee Defense], // Sacred Guard: StegoplateGuard gain +15 [Armor] and +4 [Weapon Strength]
            HeroPrefabName = "ValthrexPrimeclaw",
            UnlockCondition = UnlockCondition.HeroCompletion,
            DemoUnlockCondition = UnlockCondition.NotAvailableInDemo,
            StartingGold = 16,
            HeroID = 16,
            Race = Race.DrakosaurBrood,
            SignatureUnit = UnitName.Kaiju,
            StartingArmyUnits = new UnitName[] { UnitName.Redhorns, UnitName.Redhorns, UnitName.ScalebowKobolds, UnitName.ScalebowKobolds }
        };
        public static Hero[] Heroes = new Hero[]
        {
            EdricValeward,
            RhydanGreythorne,
            BoblinTheGoblinKing,
            KragmukGorethirster,
            BjornIronskull,
            FreyjaStormweaver,
            IltharionStarpire,
            SerendaelOfNytherial,
            SisterMorvayne,
            LordDravenBloodreaver,
            OdaNobukage,
            TokugawaHarunobu,
            HrothgarGoblinslayer,
            BerthaBarrelstorm,
            SkrixTheSwarmcaller,
            ValthrexPrimeclaw
        };
        public static Hero GetHeroByID(int id)
        {
            foreach (Hero hero in Heroes)
            {
                if (hero.HeroID == id)
                {
                    return hero;
                }
            }
            return EdricValeward; // Default to first hero if not found
        }
        public static Race GetRaceFromHero(int _heroID)
        {
            switch (_heroID)
            {
                case 1:
                    return Race.IronLegion;
                case 2:
                    return Race.IronLegion;
                case 3:
                    return Race.Gruntkin;
                case 4:
                    return Race.Gruntkin;
                case 5:
                    return Race.RavenHost;
                case 6:
                    return Race.RavenHost;
                case 7:
                    return Race.TaelindorForest;
                case 8:
                    return Race.TaelindorForest;
                case 9:
                    return Race.SanguineCourt;
                case 10:
                    return Race.SanguineCourt;
                case 11:
                    return Race.SakuraDynasty;
                case 12:
                    return Race.SakuraDynasty;
                case 13:
                    return Race.DeepstoneHold;
                case 14:
                    return Race.DeepstoneHold;
                case 15:
                    return Race.DrakosaurBrood;
                case 16:
                    return Race.DrakosaurBrood;
                default:
                    break;
            }
            return Race.IronLegion;
        }
        public static Hero GetRandomHero()
        {
            return Heroes[UnityEngine.Random.Range(1, Heroes.Length)];
        }   
        public static List<Hero> GetHeroesByRace(Race race)
        {
            List<Hero> heroesOfRace = new List<Hero>();
            foreach (Hero hero in Heroes)
            {
                if (hero.Race == race)
                {
                    heroesOfRace.Add(hero);
                }
            }
            return heroesOfRace;
        }
        public static List<UnitName> GetSignatureUnitsByRace(Race race)
        {
            List<UnitName> signatureUnits = new List<UnitName>();
            foreach (Hero hero in Heroes)
            {
                if (hero.Race == race)
                {
                    signatureUnits.Add(hero.SignatureUnit);
                }
            }
            return signatureUnits;
        }
    }
}
// Edric, the Fallen Knight: Once a celebrated knight serving a proud noble house, Edric’s world shattered when war and betrayal brought his liege low and his homeland to ruin. Now a wanderer in a fractured realm, he clings to a fading code of honor amidst a sea of mercenaries and brigands. Driven by a vision of a kingdom reborn, he fights not for gold but for justice, wielding a worn blade and sharp mind to rally the disheartened. Clad in battered regal armor, Edric gathers loyal souls willing to defy the chaos, carving a fragile path toward stability in a land teetering on the edge of oblivion.

// Rhydan, the Exiled Noble: Born to a minor noble house, Rhydan’s privileged life ended when his family was branded traitors and hunted by the crown. Forced into the wild, he honed his survival skills, becoming a spectral figure among outlaws and exiles. Known as a master of hidden trails and forgotten ruins, he aids the forsaken while biding his time for vengeance. As war engulfs the land, Rhydan steps from the shadows—not as a broken lord, but as a cunning hunter of orcs and leader of the displaced. His blade and knowledge of the wilderness guide his growing band toward a reckoning with those who wronged him.

// Boblin, the Goblin King: From the squalid depths of Rotspike Hollow, Boblin clawed his way to power, a rotund goblin with a flair for chaos and cunning. Through rigged duels—laced with traps and poison—he crushed rival leaders, uniting the fractious clans under his crude crown. Now, with a horde at his back, he leads gleeful raids on dwarf mines and elf woods, dreaming of a goblin realm carved from enemy lands. His wild charisma holds his followers, but his reckless greed stirs dissent among the ranks. Boblin’s rule is as brutal as it is unstable, a bloody gamble in a world of warring giants.

// Kragmuk Gorethirst: A hulking orc warlord whose name strikes dread into the hearts of men and beasts alike, Kragmuk Gorethirst is a whirlwind of carnage. His insatiable lust for blood empowers his ravagers, turning them into frenzied terrors on the battlefield with sharpened blades and relentless fury. Known for leaving nothing but smoldering ruins in his wake, he doubles the spoils from sacked cities, his greed matched only by his brutality.

// Bjorn Ironskull: Bjorn Ironskull, a giant among Vikings, earned his title when a foe’s mace shattered against his unyielding head. A master of siege and slaughter, he leads his armored warriors with a stoic ferocity, their defenses bolstered by his unbreakable presence. Every battle’s end sees him piling skulls as tribute, earning extra gold from the chaos—a grim reward for his relentless campaign of destruction.

// Freyja Stormweaver: A warrior-priestess touched by divine winds, commands the battlefield with the grace of a tempest and the strength of stone. Her voice weaves fate itself, inspiring her shieldmaidens to stand tall with unwavering courage and reinforced defenses. Hailed as the Rock of Trondheim, she shields her forces from fear, ensuring no terror can break their spirit as they fight under her storm-wrought banner.

//Iltharion Starpire: From the Astralwind Plains, Iltharion Starpire, a rebellious noble, leads Iltharion Forest’s Starcharge cavalry, his starlit lance shattering foes. Dubbed the Supernova of the West, his radiant presence fuels his warriors’ charges with celestial might. Clad in gleaming armor, he rallies riders to carve through chaos, protecting the fading groves. His dream is a unified Elven realm under starlight, his gallops a beacon of hope.

//Serendael of Nytherial: Serendael, a seeress from Nytherial Glade’s lunar shrines, wields moonlight to heal and summon treants for Iltharion Forest. Her visions of darkness drove her to lead, her crescent staff mending allies and awakening trees. Known as the Light of Nytherial, she doubles healing, bolstering forest spirits. Her grace fuels the fight to preserve the twilight realm from defilers.

//Sister Morvayne, once a devout priestess in a forgotten cloister, fell to the Sanguine Court’s dark whispers, her prayers twisted into necromantic hymns. From the desecrated Bleakspire Crypt, she weaves chants that summon undead hordes and curse the living with despair. Clad in tattered vestments stained with blood, her rosary of skulls channels profane power, binding souls to her will. Morvayne’s cold piety fuels the Sanguine Court’s endless march, her voice a dirge that chills the hearts of foes and inspires her skeletal legions to rise anew.  

// Lord Draven Bloodreave, master of the Crimson Hollow, is a vampire lord whose thirst for blood is matched only by his ruthless ambition. Once a mortal prince, he embraced undeath to rule eternally, his blade dripping with the essence of fallen foes. His presence on the battlefield sows terror, his crimson eyes compelling enemies to flee or kneel. Leading the Sanguine Court’s elite, Draven carves through armies, his aristocratic disdain fueling the BloodKnights’ ferocity. He seeks to drown the world in darkness, claiming every soul for his eternal court.  

//Oda Nobukage, the ruthless daimyo of a fractured clan, rose through cunning and blade, unifying warring provinces under his iron will. From the shadowed halls of Nagoya Fortress, he leads with unyielding ambition, his katana forged in dragonfire, seeking to conquer all under the Shogun Dynasty's banner. His presence ignites his warriors' fervor, turning ashigaru into unstoppable forces.

//Tokugawa Harunobu, born to a minor noble house near the imperial palace, rose through intellect and cunning, mastering economics, diplomacy, and strategy. During a famine threatening the emperor's rule, his reforms—tax incentives and grain reserves—averted crisis, earning the emperor's trust. As chief advisor, he built the dynasty's golden age with spy networks and trade mastery, amassing wealth amid whispers of manipulation. In war, his genius funded fusiliers and samurai. Loyal yet ambitious, Harunobu marches with the army, fueling conquests for eternal prosperity.

//Bushido Discipline: If army contains only Sakura Dynasty units, all units gain +20 [Leadership] and +4 [Melee Attack]

// Hrothgar Goblinslayer was once thane of a prosperous minor hold until a massive goblin night raid slaughtered his entire clan in a single bloody evening. Emerging as the sole survivor from beneath a pile of corpses, he swore a lifelong oath of vengeance upon all gruntkin. For decades he has stalked tunnels and mountain passes, his massive rune-axe dripping with goblin ichor, teaching every warrior under his command the brutal art of swarm-breaking. His mere presence fills dwarves with grim resolve and strikes terror into goblin hearts—he is the nightmare that goblins whisper about around their campfires.

// Bertha Barrelstorm is the master engineer of the Deepstone artillery guilds, renowned for designing the most devastating black-powder weapons the hold has ever forged. Born in the great gun-forges, she lost an eye to a misfiring prototype but turned that failure into relentless innovation, creating multi-barrel cannons and rune-guided mortars that can level entire enemy formations from miles away. With a perpetual cloud of pipe smoke and the smell of sulfur about him, Bertha leads her gun crews with booming precision, turning battles into thunderous symphonies of destruction and ensuring no foe ever reaches the dwarven lines intact.

// Skrix the Swarmcaller, a cunning kobold warlord who commands vast chittering hordes through fear and clever manipulation. Skrix turns weak and scattered tribes into an unstoppable tide, drowning his enemies beneath sheer numbers and ruthless coordination.

// Valthrex Primeclaw, a towering drakosaur champion forged in ritual combat and sacred duty. Valthrex leads disciplined war-beasts and elite guardians with brutal precision, crushing foes beneath claw, scale, and unyielding authority.