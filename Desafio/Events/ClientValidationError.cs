using System.Text.Json.Serialization;
using WebSocketBoilerplate;

namespace Desafio.Events;

public class ClientErrorDto(string errorMessage) : BaseDto
{
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; } = errorMessage;
}

public class ClientMustBeSignedInDto() : BaseDto
{
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; } = "Deve se autenticar para transmitir";
}

public class ClientReachedMaxTimeDto : BaseDto
{
    [JsonPropertyName("message")]
    public string? Message { get; set; } = "Tempo máximo atingido, você foi eliminado";
}

public class ClientWonDto : BaseDto
{
    [JsonPropertyName("message")] 
    public string? Message { get; set; } = "Parabéns, você venceu!";
}

public class ClientCurrentStatusDto(IEnumerable<PlayerStatus> playersStatus) : BaseDto
{
    public IEnumerable<PlayerStatus> PlayersStatus { get; set; } = playersStatus;
}

public class PlayerStatus(string playerId, string username, string totalTime)
{
    [JsonPropertyName("playerId")] 
    public string? PlayerId { get; set; } = playerId;

    [JsonPropertyName("username")] 
    public string? Username { get; set; } = username;
    
    [JsonPropertyName("totalTime")]
    public string? TotalTime { get; set; } = totalTime;
}