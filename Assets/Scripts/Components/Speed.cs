using Unity.Entities;
using Unity.NetCode;

[GhostComponent]
public struct Speed : IComponentData
{
    public float Value;
}
