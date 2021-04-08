using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(AddCommandSystem))]
    public class RemoveTemporaryCommandSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _PerformDoCommand;
        private EntityQuery _TempCommand;

        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PerformDo), typeof(Command)
                }
            };

            _PerformDoCommand = GetEntityQuery(queryDesc);

            queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Active), typeof(Command), typeof(TempCommand)
                }
            };
            _TempCommand = GetEntityQuery(queryDesc);
            RequireForUpdate(_PerformDoCommand);
            RequireForUpdate(_TempCommand);
        }

        protected override void OnUpdate()
        {
            using var performDoEntities = _PerformDoCommand.ToEntityArray(Allocator.Temp);
            using var activeCommands = _TempCommand.ToEntityArray(Allocator.Temp);
            if(activeCommands[0] == performDoEntities[0])
                return;

            var ecb = _BarrierSystem.CreateCommandBuffer();
            if (performDoEntities.Length != 0)
            {
                PerformUndoCommand(ecb);
            }
        }

        private void PerformUndoCommand(EntityCommandBuffer ecb)
        {
            var activeCommandEntity = GetSingletonEntity<Active>();
            if (!EntityManager.HasComponent<PreviousCommand>(activeCommandEntity))
                return;

            var previousCommand = EntityManager.GetComponentData<PreviousCommand>(activeCommandEntity);
            if (previousCommand.Entity == Entity.Null)
                return;

            EntityManager.AddComponent<PerformUndo>(activeCommandEntity);
            EntityManager.AddComponent<PerformCleanup>(activeCommandEntity);
            EntityManager.RemoveComponent<Active>(activeCommandEntity);

            EntityManager.AddComponent<Active>(previousCommand.Entity);
            EntityManager.RemoveComponent<NextCommand>(previousCommand.Entity);

            ecb.RemoveComponent<PerformUndo>(activeCommandEntity);
        }
    }
}