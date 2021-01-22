using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class RedoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _PerformDoCommand;

        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            var query = new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(Command)
                },
                All = new ComponentType[]
                {
                    typeof(PerformDo)
                }
            };

            _PerformDoCommand = GetEntityQuery(query);

            RequireForUpdate(_PerformDoCommand);
        }


        protected override void OnUpdate()
        {
            var performEntieties = _PerformDoCommand.ToEntityArray(Allocator.Temp);
            var redoPerformed = false;
            if (performEntieties.Length > 1)
                // TODO: multiple undos in one frame
                Debug.LogWarning("Currently multiple commands undo is not supported");

            var ecb = _BarrierSystem.CreateCommandBuffer();
            for (var i = 0; i < performEntieties.Length; i++)
            {
                var performDoEntity = performEntieties[i];

                ecb.DestroyEntity(performDoEntity);

                if (redoPerformed)
                    continue;
                var activeCommandEntity = GetSingletonEntity<Active>();
                if (!EntityManager.HasComponent<NextCommand>(activeCommandEntity))
                    continue;

                var nextCommand = EntityManager.GetComponentData<NextCommand>(activeCommandEntity);
                if (nextCommand.Entity == Entity.Null)
                    continue;

                EntityManager.AddComponent<PerformDo>(nextCommand.Entity);
                EntityManager.RemoveComponent<Active>(activeCommandEntity);

                Debug.Log($"Next command entity {nextCommand.Entity} active {activeCommandEntity}");
                EntityManager.AddComponentData(nextCommand.Entity, new Active());

                ecb.RemoveComponent<PerformDo>(nextCommand.Entity);
                redoPerformed = true;
            }

            performEntieties.Dispose();
        }
    }
}