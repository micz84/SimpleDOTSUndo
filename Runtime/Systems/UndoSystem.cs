using System.Linq;
using System.Reflection;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class UndoSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _BarrierSystem;
        private EntityQuery _PerformUndoCommand;

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
                    typeof(PerformUndo)
                }
            };

            _PerformUndoCommand = GetEntityQuery(query);

            var commandSystems = Assembly
                .GetAssembly(typeof(CommandSystemBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandSystemBase))).ToArray();
            _CommandSystems = new CommandSystemBase[commandSystems.Length];
            var i = 0;
            foreach (var systemType in commandSystems)
                _CommandSystems[i++] = (CommandSystemBase) World.DefaultGameObjectInjectionWorld.GetOrCreateSystem(systemType);
            RequireForUpdate(_PerformUndoCommand);
        }


        protected override void OnUpdate()
        {
            var performUndoEntities = _PerformUndoCommand.ToEntityArray(Allocator.Temp);

            var ecb = _BarrierSystem.CreateCommandBuffer();

            for (var i = 0; i < performUndoEntities.Length; i++)
            {
                var performDoEntity = performUndoEntities[i];
                ecb.DestroyEntity(performDoEntity);

                var activeCommandEntity = GetSingletonEntity<Active>();
                if (!EntityManager.HasComponent<PreviousCommand>(activeCommandEntity))
                    continue;

                var previousCommand = EntityManager.GetComponentData<PreviousCommand>(activeCommandEntity);
                if (previousCommand.Entity == Entity.Null)
                    continue;

                EntityManager.AddComponent<PerformUndo>(activeCommandEntity);
                EntityManager.RemoveComponent<Active>(activeCommandEntity);

                EntityManager.AddComponent<Active>(previousCommand.Entity);

                ecb.RemoveComponent<PerformUndo>(activeCommandEntity);

                for(var systemIndex = 0; systemIndex < _CommandSystems.Length;systemIndex++)
                    _CommandSystems[systemIndex].Update();

            }

            performUndoEntities.Dispose();
        }
    }
}