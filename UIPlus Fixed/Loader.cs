using System;
using System.IO;
using System.Reflection;
using Offworld.AppCore;

namespace UIPlusFixedLoader
{
    public class ModEntryPoint : ModEntryPointAdapter
    {
        private static IModEntryPoint payload;

        static ModEntryPoint()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string shortName = new AssemblyName(args.Name).Name + ".dll";
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                foreach (string resourceName in executingAssembly.GetManifestResourceNames())
                {
                    if (resourceName.EndsWith(shortName, StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream stream = executingAssembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                byte[] data = new byte[stream.Length];
                                stream.Read(data, 0, data.Length);
                                return Assembly.Load(data);
                            }
                        }
                    }
                }
                return null;
            };
        }

        public override void Initialize()
        {
            try
            {
                Assembly payloadAssembly = AppDomain.CurrentDomain.Load("UIPlusFixedCore");
                Type entryType = payloadAssembly.GetType("UIPlus.MyClass");
                if (entryType != null)
                {
                    payload = (IModEntryPoint)Activator.CreateInstance(entryType);
                    payload.Initialize();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[UIPlusFixed] Loader failed to initialize core: " + ex);
            }
        }

        public override void Shutdown() => payload?.Shutdown();
        public override void Update() => payload?.Update();
        public override void OnGameReady() => payload?.OnGameReady();
        public override void OnUIReady() => payload?.OnUIReady();
        public override void OnExitGame() => payload?.OnExitGame();
        public override void OnGameClientReady() => payload?.OnGameClientReady();
    }
}
