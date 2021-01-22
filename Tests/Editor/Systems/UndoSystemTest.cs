using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class UndoSystemTest : SystemTestBase<UndoSystem>
    {
        [Test]
        public void
            Given_OneUndoCommand_When_ThereIsNoCommandInChain_Then_ActiveCommandShouldNotBeActivate_And_ActiveCommandShouldHavePerformUndo_And_ActiveRootCommandShouldBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());
            _EntityManager.AddComponentData(rootEntity, new Active());


            var undoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity, new PerformUndo());

            SystemUpdate();

            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(rootEntity));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(rootEntity));
            Assert.IsFalse(_EntityManager.Exists(undoEntity));
            Assert.IsTrue(_EntityManager.Exists(rootEntity));
        }

        [Test]
        public void
            Given_OneUndoCommand_When_ThereIsOneCommandInChain_Then_ActiveCommandShouldNotBeActivate_And_ActiveCommandShouldHavePerformUndo_And_ActiveRootCommandShouldBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());

            var activeCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new NextCommand
            {
                Entity = activeCommand
            });

            _EntityManager.AddComponentData(activeCommand, new PreviousCommand
            {
                Entity = rootEntity
            });
            _EntityManager.AddComponentData(activeCommand, new Command());
            _EntityManager.AddComponentData(activeCommand, new Active());

            var undoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity, new PerformUndo());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(undoEntity));
        }

        [Test]
        public void
            Given_TwoUndoCommands_When_ThereIsOneCommandInChain_Then_ActiveCommandShouldNotBeActivate_And_ActiveCommandShouldHavePerformUndo_And_ActiveRootCommandShouldBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());

            var activeCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new NextCommand
            {
                Entity = activeCommand
            });
            _EntityManager.AddComponentData(activeCommand, new Command());
            _EntityManager.AddComponentData(activeCommand, new PreviousCommand
            {
                Entity = rootEntity
            });

            _EntityManager.AddComponentData(activeCommand, new Active());

            var undoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity, new PerformUndo());
            var undoEntity2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity2, new PerformUndo());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(undoEntity));
            Assert.IsFalse(_EntityManager.Exists(undoEntity2));
        }

        [Test]
        public void
            Given_TwoUndoCommand_When_ThereIsTwoCommandInChain_Then_ActiveCommandShouldNotBeActivate_And_ActiveCommandShouldHavePerformUndo_And_ActiveRootCommandShouldBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());

            var activeCommand = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new NextCommand
            {
                Entity = activeCommand
            });
            _EntityManager.AddComponentData(activeCommand, new PreviousCommand
            {
                Entity = rootEntity
            });
            _EntityManager.AddComponentData(activeCommand, new Command());
            var activeCommand2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(activeCommand, new NextCommand
            {
                Entity = activeCommand2
            });
            _EntityManager.AddComponentData(activeCommand2, new Command());
            _EntityManager.AddComponentData(activeCommand2, new Active());
            _EntityManager.AddComponentData(activeCommand2, new PreviousCommand
            {
                Entity = activeCommand
            });

            var undoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity, new PerformUndo());
            var undoEntity2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(undoEntity2, new PerformUndo());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformUndo>(activeCommand2));
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(activeCommand2));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(activeCommand));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(activeCommand2));
            Assert.IsFalse(_EntityManager.HasComponent<PerformUndo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(undoEntity));
            Assert.IsFalse(_EntityManager.Exists(undoEntity));
        }
    }
}