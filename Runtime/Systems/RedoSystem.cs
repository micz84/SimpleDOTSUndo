using System.Linq;
using System.Reflection;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class RedoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _PerformDoCommand;
        private CommandSystemBase[] _CommandSystems = null;
        protected override void OnCreate()
        {
            base.OnCreate();
            _BarrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

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
            var commandSystems = Assembly
                .GetAssembly(typeof(CommandSystemBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandSystemBase))).ToArray();
            _CommandSystems = new CommandSystemBase[commandSystems.Length];
            var i = 0;
            foreach (var systemType in commandSystems)
                _CommandSystems[i++] = (CommandSystemBase) World.DefaultGameObjectInjectionWorld.GetOrCreateSystem(systemType);
            RequireForUpdate(_PerformDoCommand);
        }


        protected override void OnUpdate()
        {
            var performEntieties = _PerformDoCommand.ToEntityArray(Allocator.Temp);


            var ecb = _BarrierSystem.CreateCommandBuffer();
            for (var i = 0; i < performEntieties.Length; i++)
            {
                var performDoEntity = performEntieties[i];

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
                for(var systemIndex = 0; systemIndex < _CommandSystems.Length;systemIndex++)
                    _CommandSystems[systemIndex].Update();
            }

            performEntieties.Dispose();
        }
    }
}