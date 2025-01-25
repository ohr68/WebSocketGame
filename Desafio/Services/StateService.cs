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
    {
        var added = Connections.TryAdd(connection.ConnectionInfo.Id, new WebSocketWithMetaData(connection));

        if (added)
            TurnOrder.Add(connection.ConnectionInfo.Id);

        return added;
    }

    public static void Broadcast(ServerBroadcastsMessageWithUsername message)
    {
        foreach (var connection in Connections)
            connection.Value.Connection.Send(JsonSerializer.Serialize(message));
    }

    public static void SignIn(IWebSocketConnection connection, ClientWantsToSignInDto clientWantsToSignInDto)
        => Connections[connection.ConnectionInfo.Id].Username = clientWantsToSignInDto.Username!;

    public static string GetConnectionUsername(Guid connectionId)
        => Connections[connectionId].Username!;

    public static bool IsCurrentTurn(Guid connectionId)
        => TurnOrder.Count > 0 && TurnOrder[_currentTurnIndex] == connectionId;

    public static void ProcessTurn(Guid connectionId)
    {
        if (Connections.Count <= 1)
        {
            Connections[connectionId].Connection
                .Send(JsonSerializer.Serialize(
                    new ClientErrorDto("É necessário que haja pelo menos dois jogadores para iniciar")));
            return;
        }

        if (!IsCurrentTurn(connectionId))
        {
            Connections[connectionId].Connection
                .Send(JsonSerializer.Serialize(new ClientErrorDto("Não é o seu turno.")));
            return;
        }

        TimeToClick.Stop();
        Connections[connectionId].TotalTime += TimeToClick.Elapsed;

        var successMessage = GetCurrentStatus();
        Connections[connectionId].Connection.Send(JsonSerializer.Serialize(successMessage));

        if (Connections[connectionId].TotalTime >= TimeSpan.FromSeconds(30))
        {
            Connections[connectionId].Connection.Send(JsonSerializer.Serialize(new ClientReachedMaxTimeDto()));
            RemoveConnection(connectionId);

            if (Connections.Count == 1)
            {
                Connections.FirstOrDefault().Value.Connection.Send(JsonSerializer.Serialize(new ClientWonDto()));
                return;
            }
        }

        TimeToClick.Reset();

        NextTurn();
    }

    private static void RemoveConnection(Guid id)
    {
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
        Connections[currentPlayerId].Connection
            .Send(JsonSerializer.Serialize(new ClientTurnStartedDto(currentPlayerId.ToString())));
        TimeToClick.Start();
    }

    private static ClientCurrentStatusDto GetCurrentStatus()
        => new ClientCurrentStatusDto(Connections.Select(c =>
            new PlayerStatus(c.Key.ToString(), c.Value.Username!, c.Value.TotalTime.ToString())));
}