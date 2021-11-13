using System.Reflection;
using HarmonyLib;
using NUnit.Framework;

namespace WpfUnit
{
	public static class AssemblySetup
    {
        public static Harmony Harmony;

		static AssemblySetup()
		{
            Harmony = new Harmony("com.github.wpfunit");
            Harmony.DEBUG = true;
            Harmony.PatchAll();
		}

		public static void EnsureIsPatched()
		{
            TestContext.Progress.WriteLine("All is Patched");
		}
	}
}