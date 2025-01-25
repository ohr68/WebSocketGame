using System.Text.Json;
using System.Text.Json.Serialization;
using Desafio.Services;
using Fleck;
using WebSocketBoilerplate;

namespace Desafio.Events;

public class ClientWantsToBroadcastDto : BaseDto
{
    [JsonPropertyName("message")] 
    public string? Message { get; set; }
}

public class ClientWantsToBroadcast : BaseEventHandler<ClientWantsToBroadcastDto>
{
    public override Task Handle(ClientWantsToBroadcastDto dto, IWebSocketConnection socket)
    {
        var username = StateService.GetConnectionUsername(socket.ConnectionInfo.Id);

        if (string.IsNullOrEmpty(username))
        {
            socket.Send(JsonSerializer.Serialize(new ClientMustBeSignedInDto()));
            return Task.CompletedTask;
        }

        var message = new ServerBroadcastsMessageWithUsername()
        {
            Id = Guid.NewGuid(),
            Message = dto.Message,
            Username = username,
            Time = DateTime.Now.ToString("HH:mm"),
        };

        StateService.Broadcast(message);

        return Task.CompletedTask;
    }
}

public class ServerBroadcastsMessageWithUsername : BaseDto
{
    [JsonPropertyName("id")] 
    public Guid Id { get; set; }

    [JsonPropertyName("message")] 
    public string? Message { get; set; }

    [JsonPropertyName("username")] 
    public string? Username { get; set; }

    [JsonPropertyName("time")] 
    public string? Time { get; set; }
}