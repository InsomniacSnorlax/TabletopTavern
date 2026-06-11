using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ShootAttack : IComponentData, IEnableableComponent {

    public float timer;
    public float timerMax;
    public int damageAmount;
    public float Range;
    public int Accuracy;
    public OnShootEvent onShoot;
    public Entity ProjectileEntity;
    public float shootAnimationDelay; // seconds before the shot that the animation plays
    public bool animationTriggered;   // prevents re-triggering within a single shot cycle

    public struct OnShootEvent {
        public bool isTriggered;
        public float3 shootFromPosition;
    }
}