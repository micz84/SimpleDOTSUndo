using pl.breams.SimpleDOTSUndo.Components;
using pl.breams.SimpleDOTSUndo.Sample.Components;
using pl.breams.SimpleDOTSUndo.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace pl.breams.SimpleDOTSUndo.Sample.Systems
{
    [UpdateAfter(typeof(UndoSystemGroup))]
    [DisableAutoCreation]
    public partial class MoveCommandSystem : CommandSystemBase
    {
        protected override void OnUpdate()
        {

            Entities.WithoutBurst().WithAll<PerformDo, Command>().ForEach((in MoveEntityCommand command) =>
            {
                var transformAspect = SystemAPI.GetAspectRW<TransformAspect>(command.Target);
                transformAspect.LocalPosition = command.Position;
                
            }).Schedule();

            Entities.WithAll<PerformUndo, Command>().ForEach(
                (in MoveEntityCommand command, in RollbackMoveEntityCommand rollbackMoveEntityCommand) =>
                {
                    var transformAspect = SystemAPI.GetAspectRW<TransformAspect>(command.Target);
                    transformAspect.LocalPosition = rollbackMoveEntityCommand.Position;
                }).Schedule();
        }
    }
}