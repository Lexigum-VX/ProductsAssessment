
using NUnit.Framework;

namespace Products.IntegrationTests.Infrastructure;

[NonParallelizable]
public abstract class IntegrationTestBase
{
    protected HttpClient Client => IntegrationTestFixture.Client;

    [SetUp]
    public async Task SetUp()
    {
        await IntegrationTestFixture.ResetDatabaseAsync();
    }
}
