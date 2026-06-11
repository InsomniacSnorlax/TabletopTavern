using Unity.Entities;

namespace TJ.Morale
{
    public struct MoraleComponent : IComponentData
    {
        public float CurrentMorale;    // Current morale value (e.g., 0-100)
        public float MaxMorale;        // Base morale cap
        public float MoraleThreshold;  // Point at which unit breaks (e.g., 20)
        public byte MoraleState;       // 0 = Steady, 1 = Wavering, 2 = Broken, 3 = Rallying
    }
    public struct IsTerrified : IComponentData, IEnableableComponent { }
    public struct RetreatingNearbyAllies : IComponentData, IEnableableComponent { public float AlertTimer; }
    public struct AlertNearbyUnitsOfBreakingTag : IComponentData { }
    public struct SquadDamageComponent : IComponentData { 
        public int SquadId;
        public int DamageDealt;
    }
    public struct HealthLossEvent : IBufferElementData
    {
        public double Time;
        public int Loss;
    }
    public struct DamageDealtEvent : IBufferElementData
    {
        public double Time;
        public int Damage;
    }
    public struct PreviousHealth : IComponentData
    {
        public int Value;
    }
    public struct PreviousDamageDealt : IComponentData
    {
        public int Value;
    }
    public enum CombatStatus { None, Winning, Losing }
    public struct HealthLossPercent : IComponentData
    {
        public float Value;
        public int RecentDamageDiscrepency;
        public CombatStatus CombatStatus;
    }
}