using MediatR;

namespace TaskManagementApi.Application.Auth.Commands;

public class GetAccessTokenCommandHandler(HttpClient httpClient)
    : IRequestHandler<GetAccessTokenCommand, string>
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<string> Handle(
        GetAccessTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        HttpResponseMessage? response = await _httpClient.PostAsync(
            "http://localhost:8080/realms/realm1/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = "saas-app",
                    ["username"] = request.Username,
                    ["password"] = request.Password,
                    ["grant_type"] = "password",
                }
            ),
            cancellationToken
        );

        _ = response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
