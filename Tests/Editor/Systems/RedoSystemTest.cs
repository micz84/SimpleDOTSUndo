using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class RedoSystemTest : SystemTestBase<RedoSystem>
    {
        [Test]
        public void
            Given_OneRedoCommand_When_ThereIsNoCommandInChain_Then_ActiveCommandShouldBeActivate_And_ActiveCommandShouldNotHavePerformDo_And_ActiveRootCommandShouldBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());
            _EntityManager.AddComponentData(rootEntity, new Active());


            var doEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(doEntity, new PerformDo());

            SystemUpdate();

            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(rootEntity));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(rootEntity));
            Assert.IsFalse(_EntityManager.Exists(doEntity));
            Assert.IsTrue(_EntityManager.Exists(rootEntity));
        }

        [Test]
        public void
            Given_OneRedoCommand_When_ThereIsOneCommandInChain_Then_ActiveCommandShouldBeActivate_And_ActiveCommandShouldNotHavePerformDo_And_ActiveRootCommandShouldNotBeActive()
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

            var redoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity, new PerformDo());

            SystemUpdate();

            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(redoEntity));
        }
        [Test]
        public void
            Given_OneRedoCommand_When_ThereIsOneCommandInChain_And_RootIsActive_Then_CommandShouldBeActivated_And_CommandShouldHavePerformDo_And_ActiveRootCommandShouldNotBeActive()
        {
            var rootEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(rootEntity, new RootCommand());
            _EntityManager.AddComponentData(rootEntity, new Active());

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

            var redoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity, new PerformDo());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(redoEntity));
        }
        [Test]
        public void
            Given_TwoRedoCommands_When_ThereIsOneCommandInChain_Then_ActiveCommandShouldBeActivate_And_ActiveCommandShouldNotHavePerformDo_And_ActiveRootCommandNotShouldBeActive()
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

            var redoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity, new PerformDo());
            var redoEntity2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity2, new PerformDo());

            SystemUpdate();

            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(rootEntity));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsFalse(_EntityManager.Exists(redoEntity));
            Assert.IsFalse(_EntityManager.Exists(redoEntity2));
        }

        [Test]
        public void
            Given_TwoRedoCommand_When_ThereIsTwoCommandAfterActiveInChain_Then_ActiveCommandShouldNotBeActivate_And_NextCommandActiveAndHavePerformDo()
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
            var nextCommand1 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(activeCommand, new NextCommand
            {
                Entity = nextCommand1
            });
            _EntityManager.AddComponentData(nextCommand1, new Command());
            _EntityManager.AddComponentData(nextCommand1, new PreviousCommand
            {
                Entity = activeCommand
            });

            var nextCommand2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(nextCommand1, new NextCommand
            {
                Entity = nextCommand2
            });
            _EntityManager.AddComponentData(nextCommand2, new Command());
            _EntityManager.AddComponentData(nextCommand2, new PreviousCommand
            {
                Entity = nextCommand1
            });
            var redoEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity, new PerformDo());
            var redoEntity2 = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(redoEntity2, new PerformDo());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(nextCommand1));
            Assert.IsTrue(_EntityManager.HasComponent<PerformDo>(nextCommand2));
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<Active>(nextCommand1));
            Assert.IsTrue(_EntityManager.HasComponent<Active>(nextCommand2));

            var barrier = _TestWord.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            barrier.Update();
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(nextCommand1));
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(activeCommand));
            Assert.IsFalse(_EntityManager.HasComponent<PerformDo>(nextCommand2));
            Assert.IsFalse(_EntityManager.Exists(redoEntity));
            Assert.IsFalse(_EntityManager.Exists(redoEntity));
        }
    }
}