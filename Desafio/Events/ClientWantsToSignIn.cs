using System.Text.Json;
using System.Text.Json.Serialization;
using Desafio.Services;
using Fleck;
using WebSocketBoilerplate;

namespace Desafio.Events;

public class ClientWantsToSignInDto : BaseDto
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

public class ClientWantsToSignIn : BaseEventHandler<ClientWantsToSignInDto>
{
    public override Task Handle(ClientWantsToSignInDto dto, IWebSocketConnection socket)
    {
        StateService.SignIn(socket, dto);
        
        var echo = new ClientSignedIn()
        {
            Message = $"Welcome {dto.Username}",
            UserName = dto.Username
        };
        
        socket.Send(JsonSerializer.Serialize(echo));
        
        return Task.CompletedTask;
    }
}

public class ClientSignedIn : BaseDto
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("username")]
    public string? UserName { get; set; }
}