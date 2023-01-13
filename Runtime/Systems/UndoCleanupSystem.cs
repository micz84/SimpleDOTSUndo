using pl.breams.SimpleDOTSUndo.Components;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    public partial class UndoCleanupSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _CancelmQuery;
        private EntityQuery _CleanUpCommandsQuery;
        private EntityQuery _ConfirmQuery;
        private EntityQuery _RedoUndoCleanupQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var query = new EntityQueryDesc
            {
                None = new ComponentType[] {typeof(Command)},
                Any = new ComponentType[] {typeof(PerformDo), typeof(PerformUndo)}
            };
            _RedoUndoCleanupQuery = GetEntityQuery(query);

            query = new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(Command), typeof(PerformCleanup)}
            };

            _CleanUpCommandsQuery = GetEntityQuery(query);

            query = new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(ConfirmCommand)}
            };

            _ConfirmQuery = GetEntityQuery(query);
            query = new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(CancelCommand)}
            };

            _CancelmQuery = GetEntityQuery(query);
        }

        protected override void OnUpdate()
        {
            var ecb = _BarrierSystem.CreateCommandBuffer();
            ecb.DestroyEntity(_CleanUpCommandsQuery);
            ecb.DestroyEntity(_RedoUndoCleanupQuery);
            ecb.DestroyEntity(_ConfirmQuery);
            ecb.DestroyEntity(_CancelmQuery);

        }
    }
}