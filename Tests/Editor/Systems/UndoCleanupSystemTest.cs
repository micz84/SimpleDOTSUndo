using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Collections;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class UndoCleanupSystemTest:SystemTestBase<UndoCleanupSystem>
    {
        [Test]
        public void Given_NoAction_When_NoCommandsInChain_Then_NoException()
        {
            _System.Update();
        }

        [Test]
        public void Given_PerformDo_When_NoCommandsInChain_Then_EntityDestroyedByBarrierSystem()
        {
            var doActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<DoComponent>(doActionEntity);
            _System.Update();
            Assert.True(_EntityManager.Exists(doActionEntity));
            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(doActionEntity));
        }
        [Test]
        public void Given_PerformDoCommand_When_NoCommandsInChain_Then_EntityNotDestroyedByBarrierSystem()
        {
            var doCommandEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(doCommandEntity);
            _EntityManager.AddComponent<DoComponent>(doCommandEntity);
            _System.Update();
            Assert.True(_EntityManager.Exists(doCommandEntity));
            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.True(_EntityManager.Exists(doCommandEntity));
        }

        [Test]
        public void Given_PerformUndo_When_NoCommandsInChain_Then_EntityDestroyedByBarrierSystem()
        {
            var undoActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<UndoComponent>(undoActionEntity);
            _System.Update();
            Assert.True(_EntityManager.Exists(undoActionEntity));
            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(undoActionEntity));
        }
        [Test]
        public void Given_PerformUndoCommand_When_NoCommandsInChain_Then_EntityNotDestroyedByBarrierSystem()
        {
            var undoCommandEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(undoCommandEntity);
            _EntityManager.AddComponent<UndoComponent>(undoCommandEntity);
            _System.Update();
            Assert.True(_EntityManager.Exists(undoCommandEntity));
            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.True(_EntityManager.Exists(undoCommandEntity));
        }

        [Test]
        public void Given_CommandsWithCleanup_When_ThereAreOtherCommandsInChain_Then_OnlyCleanupCommandsGetDestroyed()
        {


            var entityCurrentCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityCurrentCommand);
            _EntityManager.AddComponent<DoComponent>(entityCurrentCommand);

            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityCurrentCommand);

            var entityNextCommand1 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entityCurrentCommand, new NextCommand()
            {
                Entity = entityNextCommand1
            });

            _EntityManager.AddComponent<Command>(entityNextCommand1);
            _EntityManager.AddComponent<DoComponent>(entityNextCommand1);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityNextCommand1);
            _EntityManager.AddComponent<PerformCleanup>(entityNextCommand1);
            _EntityManager.AddComponentData(entityNextCommand1, new PreviousCommand()
            {
                Entity = entityCurrentCommand
            });

            var entityNextCommand2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entityNextCommand1, new NextCommand()
            {
                Entity = entityNextCommand2
            });
            _EntityManager.AddComponent<Command>(entityNextCommand2);
            _EntityManager.AddComponent<DoComponent>(entityNextCommand2);
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityNextCommand2);
            _EntityManager.AddComponent<PerformCleanup>(entityNextCommand2);
            _EntityManager.AddComponentData(entityNextCommand2, new PreviousCommand()
            {
                Entity = entityNextCommand1
            });

            var entityNewCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityNewCommand);
            _EntityManager.AddComponent<DoComponent>(entityNewCommand);
            _EntityManager.AddComponent<Active>(entityCurrentCommand);
            _System.Update();

            Assert.IsTrue(_EntityManager.Exists(entityNewCommand));
            Assert.IsTrue(_EntityManager.Exists(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.Exists(entityNextCommand1));
            Assert.IsTrue(_EntityManager.Exists(entityNextCommand2));
            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();

            Assert.IsTrue(_EntityManager.Exists(entityNewCommand));
            Assert.IsTrue(_EntityManager.Exists(entityCurrentCommand));
            Assert.IsFalse(_EntityManager.Exists(entityNextCommand1));
            Assert.IsFalse(_EntityManager.Exists(entityNextCommand2));
        }
    }
}