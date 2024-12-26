using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct TestNetcodeClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var rpc =state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(rpc, new SimpleRpc {
                Value = 54
            });
            state.EntityManager.AddComponentData(rpc, new SendRpcCommandRequest());
            Debug.Log("send RpcCommandRequest");
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
