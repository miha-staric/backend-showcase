using Tests.Integration.Fixtures;

namespace Tests.Integration.Services;

public class ExampleIntegrationTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture = fixture;

    //[Fact]
    public async Task Example_WithPostgres_Works()
    {
        // Arrange
        string connectionString = _fixture.ConnectionString;

        // Act & Assert
        Assert.NotEmpty(connectionString);
    }
}
