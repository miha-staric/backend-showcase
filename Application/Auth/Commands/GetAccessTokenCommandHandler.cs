using MediatR;

public class GetAccessTokenCommandHandler : IRequestHandler<GetAccessTokenCommand, String>
{
    private readonly HttpClient _httpClient;

    public GetAccessTokenCommandHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> Handle(GetAccessTokenCommand request, CancellationToken ct)
    {
        HttpResponseMessage? response = await _httpClient.PostAsync(
            "http://localhost:8080/realms/realm1/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = "saas-app",
                    ["username"] = request.username,
                    ["password"] = request.password,
                    ["grant_type"] = "password",
                }
            ),
            ct
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
