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

        private LeidianEmulator _defaultEmulator;
        private LeidianEmulatorController _defaultEmulatorController;

        [SetUp]
        public void Setup()
        {
            _defaultEmulator = LeidianPlayer.Instance.ListEmulators().Last();
            _defaultEmulatorController = new LeidianEmulatorController(_defaultEmulator, LeidianPlayer.Instance);
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

        [Test]
        public void EmulatorsManagementTest()
        {
            string name = "test-emulator";
            string name2 = "test-emulator2";
            LeidianEmulator emulator = LeidianPlayer.Instance.AddEmulator(name);
            Assert.NotNull(emulator);

            LeidianEmulatorController controller = new LeidianEmulatorController(emulator, LeidianPlayer.Instance);
            LeidianEmulator emulator2 = controller.Copy(name2);
            Assert.NotNull(emulator2);

            string newName = "hello-new-name";
            controller.Rename(newName);
            controller.UpdateEmulator();
            Assert.AreEqual(newName, controller.Emulator.Title);

            LeidianPlayer.Instance.RemoveEmulator(emulator2.Index);
            LeidianPlayer.Instance.RemoveEmulator(emulator.Index);
            Assert.False(LeidianPlayer.Instance.ListEmulators().Any(x => x.Title.Contains(name2)));
        }

        [Test]
        public void PowerTest()
        {
            _defaultEmulatorController.Quit();
            _defaultEmulatorController.UpdateEmulator();
            Assert.False(_defaultEmulatorController.Emulator.IsRunning);

            _defaultEmulatorController.Launch();
            _defaultEmulatorController.WaitForReady();
            Assert.True(_defaultEmulatorController.Emulator.IsRunning);

            _defaultEmulatorController.Reboot();
            Thread.Sleep(3000);

            _defaultEmulatorController.UpdateEmulator();
            _defaultEmulatorController.WaitForReady();
            Assert.True(_defaultEmulatorController.Emulator.IsRunning);

            _defaultEmulatorController.Quit();
        }

        [Test]
        public void BakUpTest()
        {
            // 这个不太好写测试，暂时先不写了
        }

        [Test]
        public void PropsTest()
        {
            _defaultEmulatorController.Launch();
            _defaultEmulatorController.WaitForReady();
            string oldNumber = _defaultEmulatorController["phone.number"];
            string newNumber = "00000000000";
            _defaultEmulatorController["phone.number"] = newNumber;
            Assert.AreEqual(newNumber, _defaultEmulatorController["phone.number"].Trim());
            _defaultEmulatorController["phone.number"] = oldNumber;
        }

        [Test]
        public void GlobalSettingTest()
        {
            string configPath = Path.Join(LeidianPlayer.Instance.DataDirectory, "config", "leidians.config");
            string oldConfig = File.ReadAllText(configPath);

            int fps = new Random().Next(59) + 1;
            LeidianPlayer.Instance.GlobalSettings(fps);

            var document = JsonDocument.Parse(File.ReadAllText(configPath));
            File.WriteAllText(configPath, oldConfig);

            Assert.AreEqual(fps, document.RootElement.GetProperty("framesPerSecond").GetInt32());
        }

    }
}
