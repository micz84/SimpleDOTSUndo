using pl.breams.SimpleDOTSUndo.Components;
using pl.breams.SimpleDOTSUndo.Sample.Components;
using pl.breams.SimpleDOTSUndo.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace pl.breams.SimpleDOTSUndo.Sample.Systems
{
    [UpdateAfter(typeof(UndoSystemGroup))]
    [DisableAutoCreation]
    public class MoveCommandSystem : CommandSystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            var ecb = _BarrierSystem.CreateCommandBuffer();

            Entities.WithoutBurst().WithAll<PerformDo, Command>().ForEach((in MoveEntityCommand command) =>
            {
                ecb.SetComponent(command.Target, new Translation
                {
                    Value = command.Position
                });
            }).Schedule();

            Entities.WithAll<PerformUndo, Command>().ForEach(
                (in MoveEntityCommand command, in RollbackMoveEntityCommand rollbackMoveEntityCommand) =>
                {
                    ecb.SetComponent(command.Target, new Translation
                    {
                        Value = rollbackMoveEntityCommand.Position
                    });
                }).Schedule();
        }
    }
}