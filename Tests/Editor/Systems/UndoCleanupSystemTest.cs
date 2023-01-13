using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class UndoCleanupSystemTest:SystemTestBase<UndoCleanupSystem>
    {
        [Test]
        public void Given_NoAction_When_NoCommandsInChain_Then_NoException()
        {
            SystemUpdateNoExecution();
        }

        [Test]
        public void Given_PerformDo_When_NoCommandsInChain_Then_EntityDestroyedByBarrierSystem()
        {
            var doActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<PerformDo>(doActionEntity);
            SystemUpdate();
            Assert.True(_EntityManager.Exists(doActionEntity));
            var barrier = _TestWord.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(doActionEntity));
        }

        [Test]
        public void Given_PerformUndo_When_NoCommandsInChain_Then_EntityDestroyedByBarrierSystem()
        {
            var undoActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<PerformUndo>(undoActionEntity);
            SystemUpdate();
            Assert.True(_EntityManager.Exists(undoActionEntity));
            var barrier = _TestWord.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(undoActionEntity));
        }

        [Test]
        public void Given_CommandsWithCleanup_When_ThereAreOtherCommandsInChain_Then_OnlyCleanupCommandsGetDestroyed()
        {
            var entityCurrentCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityCurrentCommand);

            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityCurrentCommand);

            var entityNextCommand1 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entityCurrentCommand, new NextCommand()
            {
                Entity = entityNextCommand1
            });

            _EntityManager.AddComponent<Command>(entityNextCommand1);
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
            _EntityManager.AddComponent<RegisteredCommandSystemState>(entityNextCommand2);
            _EntityManager.AddComponent<PerformCleanup>(entityNextCommand2);
            _EntityManager.AddComponentData(entityNextCommand2, new PreviousCommand()
            {
                Entity = entityNextCommand1
            });

            var entityNewCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<Command>(entityNewCommand);
            _EntityManager.AddComponent<Active>(entityCurrentCommand);
            SystemUpdate();

            Assert.IsTrue(_EntityManager.Exists(entityNewCommand));
            Assert.IsTrue(_EntityManager.Exists(entityCurrentCommand));
            Assert.IsTrue(_EntityManager.Exists(entityNextCommand1));
            Assert.IsTrue(_EntityManager.Exists(entityNextCommand2));
            var barrier = _TestWord.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();

            Assert.IsTrue(_EntityManager.Exists(entityNewCommand));
            Assert.IsTrue(_EntityManager.Exists(entityCurrentCommand));
            Assert.IsFalse(_EntityManager.Exists(entityNextCommand1));
            Assert.IsFalse(_EntityManager.Exists(entityNextCommand2));
        }

        [Test]
        public void Given_ConfirmCommand_Then_EntityDestroyedByBarrierSystem()
        {
            var confirmActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<ConfirmCommand>(confirmActionEntity);
            SystemUpdate();
            Assert.True(_EntityManager.Exists(confirmActionEntity));
            var barrier = _TestWord.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(confirmActionEntity));
        }

        [Test]
        public void Given_CancelCommand_Then_EntityDestroyedByBarrierSystem()
        {
            var cancelActionEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponent<CancelCommand>(cancelActionEntity);
            SystemUpdate();
            Assert.True(_EntityManager.Exists(cancelActionEntity));
            var barrier = _TestWord.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.False(_EntityManager.Exists(cancelActionEntity));
        }
    }
}