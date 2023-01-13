using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(RemoveTemporaryCommandSystem))]
    public partial class CancelCommandSystem:SystemBase
    {
        private EntityQuery _TempCommand;
        private EntityArchetype _UndoArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            var queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Active), typeof(Command), typeof(TempCommand)
                }
            };
            _UndoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformUndo>());
            _TempCommand = GetEntityQuery(queryDesc);
            RequireForUpdate(_TempCommand);
            RequireForUpdate<CancelCommand>();
        }

        protected override void OnUpdate()
        {
            using var tempCommandEntities = _TempCommand.ToEntityArray(Allocator.Temp);
            var tempCommandEntity = tempCommandEntities[0];

            EntityManager.CreateEntity(_UndoArchetype);
            //EntityManager.AddComponentData(tempCommandEntity, new PerformCleanup());

        }
    }
}