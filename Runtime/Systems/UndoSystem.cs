using System;
using System.Collections.Generic;
using System.Linq;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(AddCommandSystem))]
    public partial class UndoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;

        private List<CommandSystemBase> _CommandSystems = new List<CommandSystemBase>();
        private EntityQuery _PerformUndoCommand;

        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();

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

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var assembliesIndex = 0; assembliesIndex < assemblies.Length; assembliesIndex++)
            {
                var assembly = assemblies[assembliesIndex];
                var types = assembly.GetTypes();

                var commandSystems = types.Where(t => t.IsSubclassOf(typeof(CommandSystemBase))).ToArray();
                foreach (var systemType in commandSystems)
                    _CommandSystems.Add(
                        (CommandSystemBase) World.GetOrCreateSystemManaged(systemType));
            }

        }

        protected override void OnUpdate()
        {
            using var performUndoEntities = _PerformUndoCommand.ToEntityArray(Allocator.Temp);
            var ecb = _BarrierSystem.CreateCommandBuffer();

            for (var i = 0; i < performUndoEntities.Length; i++)
                PerformUndoCommand(performUndoEntities[i], ecb);

        }

        protected void PerformUndoCommand(Entity performUndoEntity, EntityCommandBuffer ecb)
        {
            ecb.DestroyEntity(performUndoEntity);

            var activeCommandEntity = GetSingletonEntity<Active>();
            if (!EntityManager.HasComponent<PreviousCommand>(activeCommandEntity))
                return;

            var previousCommand = EntityManager.GetComponentData<PreviousCommand>(activeCommandEntity);
            if (previousCommand.Entity == Entity.Null)
                return;

            EntityManager.AddComponent<PerformUndo>(activeCommandEntity);
            EntityManager.RemoveComponent<Active>(activeCommandEntity);

            EntityManager.AddComponent<Active>(previousCommand.Entity);

            ecb.RemoveComponent<PerformUndo>(activeCommandEntity);

            for (var systemIndex = 0; systemIndex < _CommandSystems.Count; systemIndex++)
                _CommandSystems[systemIndex].Update();
        }
    }
}