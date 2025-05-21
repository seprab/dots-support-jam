 using Components.AI;
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
        AddComponent(zombie, new Zombie{});
        AddComponent(zombie, new NavAgent{});
        AddComponent(zombie, new NavAgentMovement{ CurrentBufferIndex = 0 });
        AddBuffer<NavAgentBuffer>(zombie);
        AddBuffer<NavAgentPathValidityBuffer>(zombie);
    }
}