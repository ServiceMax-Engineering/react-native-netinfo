using Microsoft.ReactNative.Managed;
using Microsoft.ReactNative;

namespace RNCNetInfo
{
    public sealed class RNCNetInfoPackage : IReactPackageProvider
    {
        public void CreatePackage(IReactPackageBuilder packageBuilder)
        {
            packageBuilder.AddAttributedModules();
        }
    }
}
