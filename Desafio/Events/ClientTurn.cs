using System.Text.Json;
using System.Text.Json.Serialization;
using Desafio.Services;
using Fleck;
using WebSocketBoilerplate;

namespace Desafio.Events;

public class ClientTurnDto : BaseDto
{
    [JsonPropertyName("clicked")] 
    public bool Clicked { get; set; }
}

public class ClientTurn : BaseEventHandler<ClientTurnDto>
{
    public override Task Handle(ClientTurnDto dto, IWebSocketConnection socket)
    { 
        var username = StateService.GetConnectionUsername(socket.ConnectionInfo.Id);

        if (string.IsNullOrEmpty(username))
        {
            socket.Send(JsonSerializer.Serialize(new ClientMustBeSignedInDto()));
            return Task.CompletedTask;
        }
        
        if (dto.Clicked)
            StateService.ProcessTurn(socket.ConnectionInfo.Id);

        return Task.CompletedTask;
    }
}

public class ClientTurnStartedDto(string playerId) : BaseDto
{
    [JsonPropertyName("playerId")] 
    public string? PlayerId { get; set; } = playerId;
}