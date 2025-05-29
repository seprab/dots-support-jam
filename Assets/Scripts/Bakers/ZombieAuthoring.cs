 using Components.AI;
 using Unity.Core;
 using Unity.Entities;
using UnityEngine;

class ZombieAuthoring : MonoBehaviour
{
}
class ZombieBaker : Baker<ZombieAuthoring>
{
    public override void Bake(ZombieAuthoring authoring)
    {
        var zombie = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(zombie, new Zombie
        {
            Health = 100,
            Damage = 10,
            Speed = 2.0f,
            AttackRange = 1.5f,
            DetectionRange = 5.0f,
            AttackCooldown = 1.0f,
            LastAttackTime = new TimeData(){}
        });
        AddComponent(zombie, new ZombieStateIdle{});
        AddBuffer<NavAgentBuffer>(zombie);
        AddBuffer<NavAgentPathValidityBuffer>(zombie);
    }
}