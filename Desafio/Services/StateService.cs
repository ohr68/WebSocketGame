using System.Diagnostics;
using System.Text.Json;
using Desafio.Events;
using Fleck;

namespace Desafio.Services;

public class WebSocketWithMetaData(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; } = connection;
    public string? Username { get; set; }
    public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
}

public static class StateService
{
    private static readonly Dictionary<Guid, WebSocketWithMetaData> Connections = new();
    private static readonly List<Guid> TurnOrder = new();
    private static readonly Stopwatch TimeToClick = new();
    private static int _currentTurnIndex = 0;

    public static bool AddConnection(IWebSocketConnection connection)
        => Connections.TryAdd(connection.ConnectionInfo.Id, new WebSocketWithMetaData(connection));

    public static void Broadcast(ServerBroadcastsMessageWithUsername message)
    {
        foreach (var connection in Connections)
            connection.Value.Connection.Send(JsonSerializer.Serialize(message));
    }

    public static void SignIn(IWebSocketConnection connection, ClientWantsToSignInDto clientWantsToSignInDto)
    {
        Connections[connection.ConnectionInfo.Id].Username = clientWantsToSignInDto.Username!;
        TurnOrder.Add(connection.ConnectionInfo.Id);
    }

    public static string GetConnectionUsername(Guid connectionId)
        => Connections[connectionId].Username!;

    public static void ProcessTurn(Guid connectionId)
    {
        if (Connections.Count(c => !string.IsNullOrEmpty(c.Value.Username)) <= 1)
        {
            Connections[connectionId].Connection
                .Send(JsonSerializer.Serialize(
                    new ClientErrorDto("É necessário que haja pelo menos dois jogadores para iniciar")));
            return;
        }

        if (!IsCurrentTurn(connectionId))
        {
            Connections[connectionId].Connection
                .Send(JsonSerializer.Serialize(new NotYourTurnDto()));
            return;
        }

        TimeToClick.Stop();
        Connections[connectionId].TotalTime += TimeToClick.Elapsed;

        var successMessage = GetCurrentStatus();

        NotifyEveryone(JsonSerializer.Serialize(successMessage));

        if (Connections[connectionId].TotalTime >= TimeSpan.FromSeconds(30))
        {
            Connections[connectionId].Connection.Send(JsonSerializer.Serialize(new ClientReachedMaxTimeDto()));
            RemoveConnection(connectionId);

            if (Connections.Count(c => !string.IsNullOrEmpty(c.Value.Username)) == 1)
            {
                Connections.FirstOrDefault(c => !string.IsNullOrEmpty(c.Value.Username)).Value.Connection
                    .Send(JsonSerializer.Serialize(new ClientWonDto()));
                
                return;
            }
        }

        TimeToClick.Reset();

        NextTurn();
    }

    private static bool IsCurrentTurn(Guid connectionId)
        => TurnOrder.Count > 0 && TurnOrder[_currentTurnIndex] == connectionId;

    private static void RemoveConnection(Guid id)
    {
        Connections[id].Connection.Close();

        if (Connections.Remove(id))
        {
            TurnOrder.Remove(id);
            if (TurnOrder.Count > 0)
                _currentTurnIndex %= TurnOrder.Count;
        }
    }

    private static void NextTurn()
    {
        if (TurnOrder.Count == 0)
            return;

        _currentTurnIndex = (_currentTurnIndex + 1) % TurnOrder.Count;

        var currentPlayerId = TurnOrder[_currentTurnIndex];

        NotifyEveryone(JsonSerializer.Serialize(new ClientTurnStartedDto(currentPlayerId.ToString(),
            Connections[currentPlayerId].Username!)));

        TimeToClick.Start();
    }

    private static ClientCurrentStatusDto GetCurrentStatus()
        => new ClientCurrentStatusDto(Connections.Where(c => !string.IsNullOrEmpty(c.Value.Username))
            .Select(c => new PlayerStatus(c.Key.ToString(), c.Value.Username!, c.Value.TotalTime.ToString())));


    private static void NotifyEveryone(string message)
    {
        foreach (var connection in Connections.Where(c => !string.IsNullOrEmpty(c.Value.Username)))
        {
            connection.Value.Connection
                .Send(message);
        }
    }
}