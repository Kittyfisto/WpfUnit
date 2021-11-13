using NUnit.Framework;

namespace WpfUnit.Test
{
    [SetUpFixture]
    public sealed class SetupFixture
    {
        [OneTimeSetUp]
        public void Setup()
        {
            AssemblySetup.EnsureIsPatched();
        }
    }
}
