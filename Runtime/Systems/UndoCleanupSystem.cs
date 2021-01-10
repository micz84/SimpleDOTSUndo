using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
                Any = new ComponentType[] {typeof(DoComponent),typeof(UndoComponent)}
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