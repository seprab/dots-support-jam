using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Systems
{
    // This runs in the client and the server
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct ShootSystem : ISystem
    {
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            foreach (RefRO<NetcodePlayerInput> playerInput in
                     SystemAPI.Query<RefRO<NetcodePlayerInput>>().WithAll<Simulate>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    if (playerInput.ValueRO.shoot.IsSet)
                    {
                        Debug.Log("shoot player" + state.World);
                    }
                }
            }
        }
    }
}