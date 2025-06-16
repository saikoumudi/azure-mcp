using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AzureMcp.Tests.Client.Helpers;
using Xunit;

namespace AzureMcp.Tests.Client
{
    public class BicepSchemaCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output) : CommandTestsBase(liveTestFixture, output),
    IClassFixture<LiveTestFixture>
    {
        [Fact]
        [Trait("Category", "Live")]
        public async Task Should_get_bicep_schema_by_resource_id()
        {
            var result = await CallToolAsync(
                "azmcp-bicep-schema-list",
                new()
                {
                { "resourceId", Settings.ResourceBaseName }
                });

            var caches = result.AssertProperty("caches");
            Assert.Equal(JsonValueKind.Array, caches.ValueKind);
        }
    }
}
