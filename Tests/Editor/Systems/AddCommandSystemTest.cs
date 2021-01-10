using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class AddCommandSystemTest : SystemTestBase<AddCommandSystem>
    {
        [Test]
        public void Given_NewCommand_When_NoCommandsInChain_Then_NewCommandShouldBeAddedAsActive()
        {
            var query = new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Command>(), ComponentType.ReadOnly<DoComponent>(),
                    ComponentType.ReadOnly<RegisteredCommandSystemState>(), ComponentType.ReadOnly<Active>()
                }
            };

            var newCommandQuery = _EntityManager.CreateEntityQuery(query);

            var entity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entity);
            _EntityManager.AddComponent<DoComponent>(entity);
            _System.Update();
            var entities = newCommandQuery.ToEntityArray(Allocator.Temp);
            Assert.AreEqual(1, entities.Length);
            Assert.IsTrue(_EntityManager.HasComponent<Active>(entity));
            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(entity));
            Assert.IsTrue(_EntityManager.HasComponent<RegisteredCommandSystemState>(entity));

            entities.Dispose();
            Assert.IsTrue(true);
        }

        [Test]
        public void
            Given_NewCommand_When_CurrentCommandLastInChain_Then_NewCommandShouldBeAddedAsActiveWithPreviousCommandSet_And_PreviousCommandShouldNotBeActiveAndHaveNextCommandSet()
        {
            var query = new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Command>(), ComponentType.ReadOnly<DoComponent>(),
                    ComponentType.ReadOnly<RegisteredCommandSystemState>(), ComponentType.ReadOnly<Active>()
                }
            };

            var newCommandQuery = _EntityManager.CreateEntityQuery(query);

            var entityCurrentCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityCurrentCommand);
            _EntityManager.AddComponent<DoComponent>(entityCurrentCommand);
            _EntityManager.AddComponent<Active>(entityCurrentCommand);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityCurrentCommand);

            var entityNewCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityNewCommand);
            _EntityManager.AddComponent<DoComponent>(entityNewCommand);

            _System.Update();
            var entities = newCommandQuery.ToEntityArray(Allocator.Temp);
            Assert.AreEqual(1, entities.Length);
            Assert.IsTrue(_EntityManager.HasComponent<Active>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<RegisteredCommandSystemState>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<PreviousCommand>(entityNewCommand));

            var previousCommand = _EntityManager.GetComponentData<PreviousCommand>(entityNewCommand);
            Assert.AreEqual(entityCurrentCommand, previousCommand.Entity);

            Assert.IsFalse(_EntityManager.HasComponent<Active>(entityCurrentCommand));
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.HasComponent<RegisteredCommandSystemState>(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.HasComponent<NextCommand>(entityCurrentCommand));
            var nextCommand = _EntityManager.GetComponentData<NextCommand>(entityCurrentCommand);
            Assert.AreEqual(entityNewCommand, nextCommand.Entity);

            entities.Dispose();
            Assert.IsTrue(true);
        }

        [Test]
        public void
            Given_NewCommand_When_CurrentCommandIsNotLastInChain_Then_NewCommandShouldBeAddedAsActiveWithPreviousCommandSet_And_PreviousCommandShouldNotBeActiveAndHaveNextCommandSet_And_AllOldNextCommandsShouldBeMarkedForCleanup()
        {
            var query = new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Command>(), ComponentType.ReadOnly<DoComponent>(),
                    ComponentType.ReadOnly<RegisteredCommandSystemState>(), ComponentType.ReadOnly<Active>()
                }
            };

            var newCommandQuery = _EntityManager.CreateEntityQuery(query);

            var entityCurrentCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityCurrentCommand);
            _EntityManager.AddComponent<DoComponent>(entityCurrentCommand);
            _EntityManager.AddComponent<Active>(entityCurrentCommand);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityCurrentCommand);

            var entityNextCommand1 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entityCurrentCommand, new NextCommand
            {
                Entity = entityNextCommand1
            });

            _EntityManager.AddComponent<Command>(entityNextCommand1);
            _EntityManager.AddComponent<DoComponent>(entityNextCommand1);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityNextCommand1);
            _EntityManager.AddComponentData(entityNextCommand1, new PreviousCommand
            {
                Entity = entityCurrentCommand
            });

            var entityNextCommand2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entityNextCommand1, new NextCommand
            {
                Entity = entityNextCommand2
            });
            _EntityManager.AddComponent<Command>(entityNextCommand2);
            _EntityManager.AddComponent<DoComponent>(entityNextCommand2);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityNextCommand2);
            _EntityManager.AddComponentData(entityNextCommand2, new PreviousCommand
            {
                Entity = entityNextCommand1
            });

            var entityNewCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityNewCommand);
            _EntityManager.AddComponent<DoComponent>(entityNewCommand);

            _System.Update();
            var entities = newCommandQuery.ToEntityArray(Allocator.Temp);
            Assert.AreEqual(1, entities.Length);
            Assert.IsTrue(_EntityManager.HasComponent<Active>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<RegisteredCommandSystemState>(entityNewCommand));
            Assert.IsTrue(_EntityManager.HasComponent<PreviousCommand>(entityNewCommand));

            var previousCommand = _EntityManager.GetComponentData<PreviousCommand>(entityNewCommand);
            Assert.AreEqual(entityCurrentCommand, previousCommand.Entity);

            Assert.IsFalse(_EntityManager.HasComponent<Active>(entityCurrentCommand));
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.HasComponent<RegisteredCommandSystemState>(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.HasComponent<NextCommand>(entityCurrentCommand));
            var nextCommand = _EntityManager.GetComponentData<NextCommand>(entityCurrentCommand);
            Assert.AreEqual(entityNewCommand, nextCommand.Entity);

            Assert.True(_EntityManager.HasComponent<PerformCleanup>(entityNextCommand1));
            Assert.True(_EntityManager.HasComponent<PerformCleanup>(entityNextCommand2));

            entities.Dispose();
        }
    }
}