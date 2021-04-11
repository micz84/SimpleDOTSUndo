using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(UndoSystem))]
    public class RemoveTemporaryCommandSystem : SystemBase
    {
        private EntityQuery _PerformDoCommand;
        private EntityQuery _TempCommand;
        private EntityArchetype _UndoArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            _UndoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformUndo>());
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

            EntityManager.CreateEntity(_UndoArchetype);
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