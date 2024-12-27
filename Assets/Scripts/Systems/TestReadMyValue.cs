using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct TestReadMyValue : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRO<MyValue> myValue , Entity entity ) in
                 SystemAPI.Query<RefRO<MyValue>>().WithEntityAccess()
                )

        {
            //Debug.Log(myValue.ValueRO.value + " : " + entity.ToString() + state.World);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct TesMyValueServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<MyValue> myValue in SystemAPI.Query<RefRW<MyValue>>())
        {
            if (Input.GetKeyUp(KeyCode.T))
            {
                myValue.ValueRW.value = UnityEngine.Random.Range(0, 100);
                
            }
        }
    }
}

