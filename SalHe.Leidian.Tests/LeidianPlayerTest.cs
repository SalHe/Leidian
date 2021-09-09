using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SalHe.Leidian.Tests
{
    public class LeidianPlayerTest
    {
        private readonly string installDir = @"E:\leidian\LDPlayer4\";
        private readonly string dataDir = @"E:\leidian\LDPlayer4\vms";
        private readonly LeidianEmulator[] localEmulators =
        {
            new ()
            {
                Index = 0,
                Title = "雷电模拟器",
                IsRunning = true
            },
            new ()
            {
                Index = 1,
                Title = "雷电模拟器-1",
                IsRunning = false,
            }
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DirectoryFetchingTest()
        {
            Assert.AreEqual(installDir, LeidianPlayer.Instance.InstallDirectory);
            Assert.AreEqual(dataDir, LeidianPlayer.Instance.DataDirectory);
        }

        [Test]
        public void ListEmulatorsTest()
        {
            var list = LeidianPlayer.Instance.ListEmulators();
            for (var index = 0; index < list.Count; index++)
            {
                var emulator = list[index];
                Assert.AreEqual(localEmulators[index].Index, emulator.Index);
                Assert.AreEqual(localEmulators[index].Title, emulator.Title);
                Assert.AreEqual(localEmulators[index].IsRunning, emulator.IsRunning);
            }
        }

        [Test]
        public void LaunchAndQuitTest()
        {
            var controller = new LeidianEmulatorController(LeidianPlayer.Instance.ListEmulators().Last(), LeidianPlayer.Instance);

            controller.Quit();
            Thread.Sleep(2929);

            controller.Launch();
            Assert.True(controller.WaitForReady());

            controller.Quit();
            Thread.Sleep(2929);
            controller.UpdateEmulator();
            Assert.False(controller.Emulator.IsRunning);
        }

        [Test]
        public void QuitAllTest()
        {
            LeidianPlayer.Instance.ListEmulators()
                .Select(x => new LeidianEmulatorController(x, LeidianPlayer.Instance))
                .ToList()
                .AsParallel()
                .ForAll(x =>
                {
                    x.Launch();
                    x.WaitForReady();
                });

            Assert.True(LeidianPlayer.Instance.ListEmulators().All(x => x.IsRunning));
            LeidianPlayer.Instance.QuitAll();
            Thread.Sleep(3000);
            Assert.True(LeidianPlayer.Instance.ListEmulators().All(x => !x.IsRunning));
        }
    }
}
