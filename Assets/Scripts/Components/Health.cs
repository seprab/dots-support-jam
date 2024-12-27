using Unity.Entities;
using Unity.NetCode;

[GhostComponent]
public struct Health : IComponentData
{
    public float Value;
}
