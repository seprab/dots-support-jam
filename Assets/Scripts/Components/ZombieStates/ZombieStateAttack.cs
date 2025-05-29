using Unity.Entities;

public struct ZombieStateAttack : IComponentData
{
    public Entity Target; // The entity that the zombie is attacking
}
