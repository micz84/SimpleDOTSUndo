using NUnit.Framework;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class SystemTestBase<T> where T:SystemBase
    {
        protected World _TestWord;
        protected EntityManager _EntityManager;
        protected T _System;

        [SetUp]
        public void SetUp()
        {
            _TestWord = new World("TestWorld");
            _EntityManager = _TestWord.EntityManager;
            _System = _TestWord.GetOrCreateSystem<T>();
        }

        [TearDown]
        public void TearDown()
        {
            _TestWord?.Dispose();
        }

        protected void SystemUpdate()
        {
            var version = _System.GlobalSystemVersion;
            _System.Update();
            Assert.AreEqual(version+1, _System.GlobalSystemVersion);
        }

        protected void SystemUpdateNoExecution()
        {
            var version = _System.GlobalSystemVersion;
            _System.Update();
            Assert.AreEqual(version, _System.GlobalSystemVersion);
        }
    }
}