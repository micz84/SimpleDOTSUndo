using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(RemoveTemporaryCommandSystem))]
    public class CancelCommandSystem:SystemBase
    {
        private EntityQuery _TempCommand;

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
            _TempCommand = GetEntityQuery(queryDesc);
            RequireForUpdate(_TempCommand);
            RequireSingletonForUpdate<CancelCommand>();
        }

        protected override void OnUpdate()
        {
            using var tempCommandEntities = _TempCommand.ToEntityArray(Allocator.Temp);
            var tempCommandEntity = tempCommandEntities[0];

            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new PerformUndo());
            EntityManager.AddComponentData(tempCommandEntity, new PerformCleanup());

        }
    }
}