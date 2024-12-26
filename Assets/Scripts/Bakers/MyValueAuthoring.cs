using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

class MyValueAuthoring : MonoBehaviour
{
    
}

class MyValueAuthoringBaker : Baker<MyValueAuthoring>
{
    public override void Bake(MyValueAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MyValue());
    }
}

public struct MyValue : IComponentData
{
    [GhostField]
    public int value;
}
