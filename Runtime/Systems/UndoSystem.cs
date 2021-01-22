using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class UndoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _PerformUndoCommand;

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
                    typeof(PerformUndo)
                }
            };

            _PerformUndoCommand = GetEntityQuery(query);

            RequireForUpdate(_PerformUndoCommand);
        }


        protected override void OnUpdate()
        {
            var performUndoEntities = _PerformUndoCommand.ToEntityArray(Allocator.Temp);
            var undoPeformed = false;
            if (performUndoEntities.Length > 1)
                // TODO: multiple undos in one frame
                Debug.LogWarning("Currently multiple commands undo is not supported");

            var ecb = _BarrierSystem.CreateCommandBuffer();

            for (var i = 0; i < performUndoEntities.Length; i++)
            {
                var performDoEntity = performUndoEntities[i];
                ecb.DestroyEntity(performDoEntity);

                if (undoPeformed)
                    continue;
                var activeCommandEntity = GetSingletonEntity<Active>();
                if (!EntityManager.HasComponent<PreviousCommand>(activeCommandEntity))
                    continue;

                var previousCommand = EntityManager.GetComponentData<PreviousCommand>(activeCommandEntity);
                if (previousCommand.Entity == Entity.Null)
                    continue;

                EntityManager.AddComponent<PerformUndo>(activeCommandEntity);
                EntityManager.RemoveComponent<Active>(activeCommandEntity);

                EntityManager.AddComponent<Active>(previousCommand.Entity);

                ecb.RemoveComponent<PerformUndo>(activeCommandEntity);
                undoPeformed = true;
            }

            performUndoEntities.Dispose();
        }
    }
}