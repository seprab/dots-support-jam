 using Unity.Entities;
using UnityEngine;

class ZombieAuthoring : MonoBehaviour
{
}
class ZombieBaker : Baker<ZombieAuthoring>
{
    public override void Bake(ZombieAuthoring authoring)
    {
        var player = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(player, new Zombie{} ); 
    }
}

public struct Zombie : IComponentData
{
}