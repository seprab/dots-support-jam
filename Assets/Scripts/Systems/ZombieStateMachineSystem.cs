using Components.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

partial struct ZombieStateMachineSystem : ISystem
{
    private EntityQuery playerQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Player>()
            .WithAll<LocalTransform>();  // If the EntityGuid is not there, the entity was deleted
        playerQuery = state.GetEntityQuery(builder);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Since the state machine is based on the Zombie component, we need to make structural changes with ECBs
        // and not directly in the system.
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        #region ZombieIdleState
        // Zombies in the idle state should look for players to attack.
        var players = playerQuery.ToEntityArray(Allocator.TempJob);
        foreach (var (
                     idleZombieState,
                     transform,
                     zombie,
                     entity)
                 in SystemAPI.Query<
                         RefRW<ZombieStateIdle>,
                         RefRO<LocalTransform>,
                         RefRW<Zombie>>()
                     .WithEntityAccess())
        {
            if (players.Length > 0)
            {
                Entity closestPlayer = Entity.Null;
                float closestDistance = float.MaxValue;

                foreach (var player in players)
                {
                    var playerTransform = SystemAPI.GetComponent<LocalTransform>(player);
                    float distanceToPlayer = math.distance(transform.ValueRO.Position, playerTransform.Position);

                    if (distanceToPlayer < closestDistance)
                    {
                        closestDistance = distanceToPlayer;
                        closestPlayer = player;
                    }
                }

                if (closestPlayer != Entity.Null)
                {
                    ecb.RemoveComponent<ZombieStateIdle>(entity);
                    ecb.AddComponent<NavAgent>(entity);
                    
                    var agentMovement = new NavAgentMovement { CurrentBufferIndex = 0 };
                    ecb.AddComponent(entity, agentMovement);
                    
                    var searchState = new ZombieStateSearch { Target = closestPlayer, };
                    ecb.AddComponent(entity, searchState);
                }
            }
        }
        #endregion
        #region ZombieAttackState
        // Zombies in the attack state should check for their target and perform actions accordingly.
        foreach (var (
                     attackZombieState,
                     transform,
                     zombie,
                     entity)
                 in SystemAPI.Query<
                         RefRW<ZombieStateAttack>,
                         RefRO<LocalTransform>,
                         RefRW<Zombie>>()
                     .WithEntityAccess())
        {
            if (attackZombieState.ValueRO.Target != Entity.Null)
            {
                var targetTransform = SystemAPI.GetComponent<LocalTransform>(attackZombieState.ValueRO.Target);
                float distanceToTarget = math.distance(transform.ValueRO.Position, targetTransform.Position);
                
                if (distanceToTarget > zombie.ValueRO.AttackRange)
                {
                    if (SystemAPI.HasComponent<Health>(attackZombieState.ValueRO.Target))
                    {
                        Health targetHealth = SystemAPI.GetComponent<Health>(attackZombieState.ValueRW.Target);
                        targetHealth.Value -= zombie.ValueRO.Damage;
                        zombie.ValueRW.LastAttackTime = SystemAPI.Time; // Reset last attack time
                    }
                    else
                    {
                        Debug.LogError("Target entity does not have Health component.");
                    }
                }
                else
                {
                    ecb.RemoveComponent<ZombieStateAttack>(entity);
                    ecb.AddComponent<ZombieStateIdle>(entity);
                }
            }
        }
        #endregion
        #region ZombieSearchState
        // Zombies in the search state should look for players and transition to attack state if found.
        foreach (var (
                     ana,
                     searchZombieState,
                     entity
                     ) in SystemAPI.Query<
                     AgentNavAspect,
                     RefRW<ZombieStateSearch>>()
                     .WithEntityAccess())
        {
            if (ana.agentMovement.ValueRW.Reached)
            {
                // Check if the agent has reached its target location, so we change it's state and transfer the target to the attack state.
                var targetEntity = searchZombieState.ValueRO.Target;
                ecb.RemoveComponent<ZombieStateSearch>(entity);
                ecb.RemoveComponent<NavAgent>(entity);
                ecb.RemoveComponent<NavAgentBuffer>(entity);
                
                var zombiestateAttack = new ZombieStateAttack() { Target = targetEntity };
                ecb.AddComponent(entity, zombiestateAttack);
            }
            else
            {
                // If the agent has not reached its target location, we update the agent's ToLocation to the target entity's position.
                var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(searchZombieState.ValueRO.Target);
                ana.agent.ValueRW.ToLocation = new float3(
                    targetTransform.Position.x,
                    targetTransform.Position.y,
                    targetTransform.Position.z);
            }
        }
        #endregion
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
