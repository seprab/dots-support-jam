using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

class NetcodePlayerInputAuthoring : MonoBehaviour
{
    
}

class NetcodeInputAuthoringBaker : Baker<NetcodePlayerInputAuthoring>
{
    public override void Bake(NetcodePlayerInputAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new NetcodePlayerInput());
    }
}

public struct NetcodePlayerInput : IInputComponentData
{
    public float2 inputVector;
    public InputEvent shoot;
}
