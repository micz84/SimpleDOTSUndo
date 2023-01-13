using NUnit.Framework;
using pl.breams.SimpleDOTSUndo.Components;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class CancelCommandSystemTest : SystemTestBase<CancelCommandSystem>
    {
        [Test]
        public void Given_CancelCommand_When_ThereIsTempCommand_Then_CommandShouldNotBeTemp()
        {
            var entity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entity, new Command());
            _EntityManager.AddComponentData(entity, new TempCommand());
            _EntityManager.AddComponentData(entity, new Active());


            var confirmEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(confirmEntity, new CancelCommand());

            SystemUpdate();

            Assert.IsTrue(_EntityManager.HasComponent<PerformUndo>(entity));
            Assert.IsTrue(_EntityManager.HasComponent<PerformCleanup>(entity));
            Assert.IsTrue(_EntityManager.Exists(entity));
        }

        [Test]
        public void
            Given_NoConfirmCommand_When_ThereIsTempCommand_Then_CommandShouldHaveTempCommand_And_SystemShouldNotExecute()
        {
            var entity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(entity, new Command());
            _EntityManager.AddComponentData(entity, new TempCommand());

            SystemUpdateNoExecution();

            Assert.IsTrue(_EntityManager.HasComponent<TempCommand>(entity));
            Assert.IsTrue(_EntityManager.Exists(entity));
        }

        [Test]
        public void Given_ConfirmCommand_When_ThereNoIsTempCommand_Then_SystemShouldNotExecute()
        {
            var confirmEntity = _EntityManager.CreateEntity();
            _EntityManager.AddComponentData(confirmEntity, new CancelCommand());

            SystemUpdateNoExecution();

        }
    }
}