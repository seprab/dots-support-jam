using Unity.Entities;

public struct Zombie : IComponentData
{
    public int Health; // Health of the zombie
    public int Damage; // Damage dealt by the zombie
    public float Speed; // Movement speed of the zombie
    public float AttackRange; // Range within which the zombie can attack
    public float DetectionRange; // Range within which the zombie can detect targets
    public float AttackCooldown; // Time between attacks
    public Unity.Core.TimeData LastAttackTime; // Timestamp of the last attack
}
