using System;
using System.Collections.Generic;
using System.Linq;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(UndoCleanupSystem))]
    public partial class RedoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private readonly List<CommandSystemBase> _CommandSystems = new List<CommandSystemBase>();
        private EntityQuery _PerformDoCommand;

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
                    typeof(PerformDo)
                }
            };

            _PerformDoCommand = GetEntityQuery(query);
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

            RequireForUpdate(_PerformDoCommand);
        }


        protected override void OnUpdate()
        {
            using var performEntities = _PerformDoCommand.ToEntityArray(Allocator.Temp);

            var ecb = _BarrierSystem.CreateCommandBuffer();
            for (var i = 0; i < performEntities.Length; i++)
            {
                var performDoEntity = performEntities[i];

                ecb.DestroyEntity(performDoEntity);
                var activeCommandEntity = GetSingletonEntity<Active>();
                if (!EntityManager.HasComponent<NextCommand>(activeCommandEntity))
                    continue;

                var nextCommand = EntityManager.GetComponentData<NextCommand>(activeCommandEntity);
                if (nextCommand.Entity == Entity.Null)
                    continue;

                EntityManager.AddComponent<PerformDo>(nextCommand.Entity);
                EntityManager.RemoveComponent<Active>(activeCommandEntity);

                EntityManager.AddComponentData(nextCommand.Entity, new Active());

                ecb.RemoveComponent<PerformDo>(nextCommand.Entity);
                for (var systemIndex = 0; systemIndex < _CommandSystems.Count; systemIndex++)
                    _CommandSystems[systemIndex].Update();
            }

        }
    }
}