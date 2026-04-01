using Tests.Integration.Fixtures;

namespace Tests.Integration.Services;

public class ExampleIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public ExampleIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    //[Fact]
    public async Task Example_WithPostgres_Works()
    {
        // Arrange
        String connectionString = _fixture.ConnectionString;

        // Act & Assert
        Assert.NotEmpty(connectionString);
    }
}
