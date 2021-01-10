using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UndoCleanupSystem))]
    public class AddCommandSystem : SystemBase
    {
        private EntityQuery _NewCommandQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            var query = new EntityQueryDesc
            {
                None = new[] {ComponentType.ReadOnly<RegisteredCommandSystemState>()},
                All = new[] {ComponentType.ReadOnly<Command>(), ComponentType.ReadOnly<DoComponent>()}
            };

            _NewCommandQuery = GetEntityQuery(query);

            RequireForUpdate(_NewCommandQuery);
        }

        protected override void OnUpdate()
        {
            var entities = _NewCommandQuery.ToEntityArray(Allocator.Temp);
            if (entities.Length != 1)
                return;

            var commandEntity = entities[0];

            if (HasSingleton<Active>())
            {
                // there are commands in chain, check if active command has more
                // commands after it.
                var activeCommandEntity = GetSingletonEntity<Active>();
                if (!HasComponent<NextCommand>(activeCommandEntity))
                {
                    EntityManager.AddComponentData(activeCommandEntity, new NextCommand
                    {
                        Entity = commandEntity
                    });
                }
                else
                {
                    var nextCommand = GetComponent<NextCommand>(activeCommandEntity);

                    if (nextCommand.Entity == Entity.Null)
                        // current command in chain is last
                        EntityManager.SetComponentData(activeCommandEntity, new NextCommand
                        {
                            Entity = commandEntity
                        });
                    else
                    {
                        while (nextCommand.Entity != Entity.Null)
                        {
                            EntityManager.AddComponentData(nextCommand.Entity, new PerformCleanup());
                            if (HasComponent<NextCommand>(nextCommand.Entity))
                                nextCommand = GetComponent<NextCommand>(nextCommand.Entity);
                            else
                                break;
                        }
                        // there are more commands after need to mark them for cleanup
                        EntityManager.SetComponentData(activeCommandEntity, new NextCommand
                        {
                            Entity = commandEntity
                        });
                    }
                }

                EntityManager.AddComponentData(commandEntity, new PreviousCommand
                {
                    Entity = activeCommandEntity
                });

                EntityManager.RemoveComponent<Active>(activeCommandEntity);
            }

            EntityManager.AddComponentData(commandEntity, new Active());
            EntityManager.AddComponentData(commandEntity, new PerformDo());
            EntityManager.AddComponentData(commandEntity, new RegisteredCommandSystemState());
        }
    }

    public struct RegisteredCommandSystemState : IComponentData
    {
    }
}