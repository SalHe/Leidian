using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        [Test]
        public void ModifyTest()
        {
            LeidianEmulator emulator = LeidianPlayer.Instance.ListEmulators().Last();
            LeidianEmulatorController controller = new LeidianEmulatorController(emulator, LeidianPlayer.Instance);
            string configPath = Path.Join(LeidianPlayer.Instance.DataDirectory, "config", $"leidian{emulator.Index}.config");
            string oldConfig = File.ReadAllText(configPath);

            ScreenResolution resolution = new ScreenResolution()
            {
                Width = 360,
                Height = 480,
                DPI = 5
            };
            string phoneNumber = "12345678901";
            bool autoRotate = new Random().Next(10) % 2 == 0;
            controller.Modify(resolution, autoRotate: autoRotate, phoneNumber: phoneNumber);
            
            var document = JsonDocument.Parse(File.ReadAllText(configPath));
            File.WriteAllText(configPath, oldConfig);
            Assert.AreEqual(autoRotate, document.RootElement.GetProperty("basicSettings.autoRotate").GetBoolean());
            Assert.AreEqual(phoneNumber, document.RootElement.GetProperty("propertySettings.phoneNumber").GetString());
            Assert.AreEqual(resolution.DPI, document.RootElement.GetProperty("advancedSettings.resolutionDpi").GetInt32());
            Assert.AreEqual(resolution.Width, document.RootElement.GetProperty("advancedSettings.resolution").GetProperty("width").GetInt32());
            Assert.AreEqual(resolution.Height, document.RootElement.GetProperty("advancedSettings.resolution").GetProperty("height").GetInt32());

        }
    }
}
