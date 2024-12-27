using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

//This is going to run in the client
[UpdateInGroup(typeof(GhostInputSystemGroup))]
partial struct NetcodePlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<NetcodePlayerInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ( RefRW<NetcodePlayerInput> netcodePlayerInput  
            in SystemAPI.Query<RefRW<NetcodePlayerInput>>().WithAll<GhostOwnerIsLocal>() ) 
        {
            var input = new float2();
            if (Input.GetKey(KeyCode.W))
            {
                input.y = +1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                input.y = -1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                input.x = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                input.x = 1;
            }
            netcodePlayerInput.ValueRW.inputVector  = input;

            if (Input.GetKeyUp(KeyCode.U))
            {
                Debug.Log("shoot ");
                netcodePlayerInput.ValueRW.shoot.Set(); //Instead of a bool, this is going to be network safe. This is like a true
            }
            else
            {
               netcodePlayerInput.ValueRW.shoot  = default; //this is going to be set to false
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
