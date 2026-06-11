namespace TJ.Spells
{
public enum SpellName { LightningStrike, NaturesWrath, Heal, Fireball}
public enum SpellTargetingType { World, Squad }
[System.Serializable] public struct SpellData
{
    public SpellName SpellName;
    public SpellTargetingType SpellTargetingType;
    public string SpellDescription;
    public float SpellCooldown;
    public string SpellIcon;
    public DamageBufferElement damageBufferElement;
    public float SpellRadius;
    public float SpellWarmUpDuration;
    public float SpellDuration;
    public float SpellForce;
    public bool IsOneOff;
    public string spellWarmupSound;
    public string spellHitSound;
    public Team TargetTeam;
}
public static class SpellDataLibrary
{
    public static SpellData LightningStrike = new() {
        SpellName = SpellName.LightningStrike,
        SpellTargetingType = SpellTargetingType.World,
        SpellDescription = "Summon a bolt of lightning that deals magic damage to any enemies caught in it's blast",
        SpellCooldown = 2f,
        SpellIcon = "LightningStrike",
        damageBufferElement = new DamageBufferElement() {
            DamageType = DamageType.Magical,
            TeamOfSource = Team.Player,
            AttackStrength = 5
        },
        SpellForce = 5f,
        SpellRadius = 5f,
        SpellWarmUpDuration = 2f,
        SpellDuration = 3f,
        spellWarmupSound = "lighting-charge",
        spellHitSound = "lightning-strike",
        IsOneOff = true
    };
    public static SpellData NaturesWrath = new() {
        SpellName = SpellName.NaturesWrath,
        SpellTargetingType = SpellTargetingType.Squad,
        SpellDescription = "Summon a bolt of lightning that deals magic damage to any enemies caught in it's blast",
        SpellCooldown = 5f,
        SpellIcon = "NaturesWrath",
        damageBufferElement = new DamageBufferElement() {
            DamageType = DamageType.Magical,
            TeamOfSource = Team.Player,
            AttackStrength = 0
        },
        SpellForce = 0f,
        SpellRadius = 10f,
        SpellWarmUpDuration = 1f,
        SpellDuration = 10f,
        spellWarmupSound = "NaturesWrath",
        spellHitSound = "NaturesWrath",
        TargetTeam = Team.Enemy,
        IsOneOff = true
    };
    public static SpellData GetSpellData(SpellName spellName)
    {
        return spellName switch
        {
            SpellName.LightningStrike => LightningStrike,
            SpellName.NaturesWrath => NaturesWrath,
            _ => new SpellData(),
        };
    }
}
}