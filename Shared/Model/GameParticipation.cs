﻿namespace Treachery.Shared;

public class GameParticipation
{
    /// <summary>
    /// For each User (player or observer) in the game, hold the name
    /// </summary>
    public Dictionary<int, string> Users { get; } = [];

    /// <summary>
    /// All Users (UserIds) that wish to participate while awaiting players (before the game has actually started)
    /// </summary>
    public List<int> StandingPlayers { get; } = [];
    
    /// <summary>
    /// For all Users (UserIds) that are players, hold their Seat
    /// </summary>
    public Dictionary<int, int> SeatedPlayers { get; } = [];

    /// <summary>
    /// All joined users that are Observers (UserIds)
    /// </summary>
    public HashSet<int> Observers { get; } = [];

    /// <summary>
    /// All joined users (UserIds) that are Hosts
    /// </summary>
    public HashSet<int> Hosts { get; } = [];
    
    /// <summary>
    /// Seats that may be taken by other players
    /// </summary>
    public HashSet<int> AvailableSeats { get; } = [];
    
    /// <summary>
    /// Kicked users (UserIds)
    /// </summary>
    public HashSet<int> Kicked { get; } = [];
    
    public bool BotsArePaused { get; set; }
    
    public bool BotPositionsAreAvailable { get; set; }
}