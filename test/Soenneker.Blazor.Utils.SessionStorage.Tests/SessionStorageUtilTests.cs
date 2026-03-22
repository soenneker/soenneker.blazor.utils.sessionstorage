using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Blazor.Utils.SessionStorage.Tests;

[Collection("Collection")]
public sealed class SessionStorageUtilTests : FixturedUnitTest
{
    private readonly ISessionStorageUtil _blazorlibrary;

    public SessionStorageUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _blazorlibrary = Resolve<ISessionStorageUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
