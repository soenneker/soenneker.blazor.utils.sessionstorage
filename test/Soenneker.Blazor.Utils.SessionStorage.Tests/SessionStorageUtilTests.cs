using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.Utils.SessionStorage.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class SessionStorageUtilTests : HostedUnitTest
{
    private readonly ISessionStorageUtil _blazorlibrary;

    public SessionStorageUtilTests(Host host) : base(host)
    {
        _blazorlibrary = Resolve<ISessionStorageUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
