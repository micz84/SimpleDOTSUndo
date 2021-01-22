using pl.breams.SimpleDOTSUndo.Components;
using Unity.Entities;


namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    public class UndoCleanupSystem:SystemBase
    {
        private EntityQuery _CleanUpCommandsQuery;
        private EntityQuery _RedoUndoCleanupQuery;
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var query = new EntityQueryDesc
            {
                None = new ComponentType[] {typeof(Command)},
                Any = new ComponentType[] {typeof(PerformDo),typeof(PerformUndo)}
            };
            _RedoUndoCleanupQuery = GetEntityQuery(query);

            query = new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(Command), typeof(PerformCleanup)}
            };

           _CleanUpCommandsQuery = GetEntityQuery(query);

        }

        protected override void OnUpdate()
        {
            var ecb = _BarrierSystem.CreateCommandBuffer();
            ecb.DestroyEntity(_CleanUpCommandsQuery);
            ecb.DestroyEntity(_RedoUndoCleanupQuery);
        }
    }
}