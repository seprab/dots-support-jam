using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ShootPoint : IComponentData
{
    public Entity Value;
}
