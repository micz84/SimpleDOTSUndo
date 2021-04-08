using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using ComponentType = Unity.Entities.ComponentType;
using EntityQueryDesc = Unity.Entities.EntityQueryDesc;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class RemoveTemporaryCommandSystemTest:SystemTestBase<RemoveTemporaryCommandSystem>
    {

        [Test]
        public void
            Given_NewCommand_When_CurrentCommandIsTemp_Then_NewCommandShouldBeAddedAsActiveWithPreviousCommandSet_And_PreviousCommandShouldNotBeActiveAndHaveNextCommandSet_And_AllOldNextCommandsShouldBeMarkedForCleanup()
        {
            var query = new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Command>(), ComponentType.ReadOnly<RegisteredCommandSystemState>(), ComponentType.ReadOnly<Active>()
                }
            };

            var newCommandQuery = _EntityManager.CreateEntityQuery(query);

            var entityCurrentCommand = _EntityManager.CreateEntity();
            var entityPrevCommand1 = _EntityManager.CreateEntity();
            var entityPrevCommand2 = _EntityManager.CreateEntity();

            _EntityManager.AddComponent<Command>(entityCurrentCommand);
            _EntityManager.AddComponent<Active>(entityCurrentCommand);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityCurrentCommand);
            _EntityManager.AddComponent<TempCommand>(entityCurrentCommand);
            _EntityManager.AddComponentData(entityCurrentCommand, new PreviousCommand()
            {
                Entity = entityPrevCommand1
            });

            _EntityManager.AddComponent<Command>(entityPrevCommand1);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityPrevCommand1);
            _EntityManager.AddComponentData(entityPrevCommand1, new PreviousCommand
            {
                Entity = entityPrevCommand2
            });
            _EntityManager.AddComponentData(entityPrevCommand1, new NextCommand
            {
                Entity = entityCurrentCommand
            });

            _EntityManager.AddComponent<Command>(entityPrevCommand2);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityPrevCommand2);
            _EntityManager.AddComponentData(entityPrevCommand2, new NextCommand()
            {
                Entity = entityPrevCommand1
            });

            var entityNewCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityNewCommand);
            _EntityManager.AddComponent<PerformDo>(entityNewCommand);

            SystemUpdate();
            var entities = newCommandQuery.ToEntityArray(Allocator.Temp);
            Assert.AreEqual(1, entities.Length);
            Assert.IsTrue(_EntityManager.HasComponent<Active>(entityPrevCommand1));
            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(entityNewCommand));

            Assert.IsFalse(_EntityManager.HasComponent<Active>(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.HasComponent<PerformCleanup>(entityCurrentCommand));

            entities.Dispose();
        }
    }
}