using System;
using HarmonyLib;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace Lobby;

public class Lobby : Plugin<Config>
{
    public static Lobby Singleton { get; private set; }

    public override string Name => "Lobby";

    public override string Description => "A plugin that adds a lobby when waiting for players.";

    public override string Author => "MrAfitol & MedveMarci";

    public override Version Version { get; } = new(1, 0, 2);

    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    public Harmony Harmony { get; private set; }

    private EventsHandler EventsHandler { get; set; }
    public RestrictionsHandler RestrictionsHandler { get; private set; }

    public override void Enable()
    {
        Singleton = this;
        Harmony = new Harmony("lobby.scp.sl");
        EventsHandler = new EventsHandler();
        RestrictionsHandler = new RestrictionsHandler();
        ServerEvents.WaitingForPlayers += EventsHandler.OnWaitingForPlayers;
        ServerEvents.RoundStarted += EventsHandler.OnRoundStarted;
    }

    public override void Disable()
    {
        ServerEvents.WaitingForPlayers -= EventsHandler.OnWaitingForPlayers;
        ServerEvents.RoundStarted -= EventsHandler.OnRoundStarted;
        EventsHandler.UnregisterHandlers();
        RestrictionsHandler = null;
        EventsHandler = null;
        Harmony = null;
        Singleton = null;
    }
}