using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


partial struct PlayerSpawnerSystem : ISystem
{
    /*
    EntityQuery playerQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerSpawner>();
        playerQuery = SystemAPI.QueryBuilder().WithAny<Player>().Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (playerQuery.CalculateEntityCount() == 0)
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().playerPrefab;
            var player = state.EntityManager.Instantiate(prefab);
            state.EntityManager.SetComponentData(player, LocalTransform.FromPosition(4,0,0));
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
    
    */
}
