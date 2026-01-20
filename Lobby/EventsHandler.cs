using System;
using System.Collections.Generic;
using System.Linq;
using CentralAuth;
using CustomPlayerEffects;
using GameCore;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Lobby.API;
using Lobby.ApiFeatures;
using MEC;
using PlayerRoles;
using PlayerRoles.Voice;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace Lobby;

public class EventsHandler
{
    private CoroutineHandle _lobbyTimer, _rainbowColor;
    private int _r = 255, _g, _b;
    private string _text;
    public static bool IsIntercom { get; set; }
    public static bool IsLobby { get; private set; } = true;

    #region Events

    public void OnWaitingForPlayers()
    {
        ApiManager.CheckForUpdates();
        try
        {
            LogManager.Debug("OnWaitingForPlayers event triggered.");
            LobbyLocationHandler.Point = new GameObject("LobbyPoint");
            IsLobby = true;
            IsIntercom = false;
            Lobby.Singleton.Harmony.PatchAll();
            RegisterHandlers();
            LogManager.Debug("Handlers registered and Harmony patches applied.");
            SpawnManager();
            LogManager.Debug("Lobby location spawned.");

            Timing.CallDelayed(0.1f, () =>
            {
                GameObject.Find("StartRound").transform.localScale = Vector3.zero;
                LogManager.Debug($"LobbyTimer: {_lobbyTimer.IsRunning} | RainbowColor: {_rainbowColor.IsRunning}");
                if (_lobbyTimer.IsRunning)
                    Timing.KillCoroutines(_lobbyTimer);
                if (_rainbowColor.IsRunning)
                    Timing.KillCoroutines(_rainbowColor);

                if (Lobby.Singleton.Config.TitleText.Contains("<rainbow>") ||
                    Lobby.Singleton.Config.PlayerCountText.Contains("<rainbow>"))
                    _rainbowColor = Timing.RunCoroutine(RainbowColor());

                _lobbyTimer = Timing.RunCoroutine(LobbyTimer());
            });
        }
        catch (Exception e)
        {
            Logger.Error("[Event: OnWaitingForPlayers] " + e);
        }
    }

    private static void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        try
        {
            if (IsLobby && (RoundStart.singleton.NetworkTimer > 1 || RoundStart.singleton.NetworkTimer == -2))
                Timing.CallDelayed(1f, () =>
                {
                    if (!ev.Player.IsOverwatchEnabled)
                    {
                        ev.Player.SetRole(Lobby.Singleton.Config.LobbyPlayerRole);

                        ev.Player.IsGodModeEnabled = true;

                        if (Lobby.Singleton.Config.LobbyInventory.Count > 0)
                            foreach (var item in Lobby.Singleton.Config.LobbyInventory)
                                ev.Player.AddItem(item);

                        Timing.CallDelayed(0.1f, () =>
                        {
                            ev.Player.Position = LobbyLocationHandler.Point.transform.position;
                            ev.Player.Rotation = LobbyLocationHandler.Point.transform.rotation;

                            if (Lobby.Singleton.Config.EnableMovementBoost)
                                ev.Player.EnableEffect<MovementBoost>(Lobby.Singleton.Config.MovementBoostIntensity);
                        });
                    }
                });
        }
        catch (Exception e)
        {
            Logger.Error("[Event: OnPlayerJoined] " + e);
        }
    }

    public void OnRoundStarted()
    {
        try
        {
            UnregisterHandlers();
            IsLobby = false;

            if (!string.IsNullOrEmpty(IntercomDisplay._singleton.Network_overrideText))
                IntercomDisplay._singleton.Network_overrideText = "";

            foreach (var player in Player.ReadyList)
            {
                if (!player.IsOverwatchEnabled)
                    player.SetRole(RoleTypeId.Spectator);

                Timing.CallDelayed(0.1f, () =>
                {
                    player.IsGodModeEnabled = false;
                    if (Lobby.Singleton.Config.EnableMovementBoost) player.DisableEffect<MovementBoost>();
                });
            }

            Timing.CallDelayed(1f, () =>
            {
                if (_lobbyTimer.IsRunning)
                    Timing.KillCoroutines(_lobbyTimer);
                if (_rainbowColor.IsRunning)
                    Timing.KillCoroutines(_rainbowColor);
            });

            Lobby.Singleton.Harmony.UnpatchAll("lobby.scp.sl");
        }
        catch (Exception e)
        {
            Logger.Error("[Event: OnRoundStarted] " + e);
        }
    }

    #endregion

    #region Methods

    private void RegisterHandlers()
    {
        try
        {
            PlayerEvents.InteractingDoor += Lobby.Singleton.RestrictionsHandler.OnPlayerInteractingDoor;
            PlayerEvents.InteractingElevator += Lobby.Singleton.RestrictionsHandler.OnPlayerInteractingElevator;
            PlayerEvents.SearchingPickup += Lobby.Singleton.RestrictionsHandler.OnPlayerSearchingPickup;
            PlayerEvents.DroppingItem += Lobby.Singleton.RestrictionsHandler.OnPlayerDroppingItem;
            PlayerEvents.DroppingAmmo += Lobby.Singleton.RestrictionsHandler.OnPlayerDroppingAmmo;
            PlayerEvents.ThrowingItem += Lobby.Singleton.RestrictionsHandler.OnPlayerThrowingItem;
            PlayerEvents.UsingIntercom += Lobby.Singleton.RestrictionsHandler.OnPlayerUsingIntercom;
            PlayerEvents.Joined += OnPlayerJoined;
        }
        catch (Exception e)
        {
            Logger.Error("[Lobby] [Method: RegisterHandlers] " + e);
        }
    }

    public void UnregisterHandlers()
    {
        try
        {
            PlayerEvents.InteractingDoor -= Lobby.Singleton.RestrictionsHandler.OnPlayerInteractingDoor;
            PlayerEvents.InteractingElevator -= Lobby.Singleton.RestrictionsHandler.OnPlayerInteractingElevator;
            PlayerEvents.SearchingPickup -= Lobby.Singleton.RestrictionsHandler.OnPlayerSearchingPickup;
            PlayerEvents.DroppingItem -= Lobby.Singleton.RestrictionsHandler.OnPlayerDroppingItem;
            PlayerEvents.DroppingAmmo -= Lobby.Singleton.RestrictionsHandler.OnPlayerDroppingAmmo;
            PlayerEvents.ThrowingItem -= Lobby.Singleton.RestrictionsHandler.OnPlayerThrowingItem;
            PlayerEvents.UsingIntercom -= Lobby.Singleton.RestrictionsHandler.OnPlayerUsingIntercom;
            PlayerEvents.Joined -= OnPlayerJoined;
        }
        catch (Exception e)
        {
            Logger.Error("[Lobby] [Method: UnregisterHandlers] " + e);
        }
    }

    private static void SpawnManager()
    {
        try
        {
            var locationList = new List<LocationData>();

            if (Lobby.Singleton.Config.LobbyLocation?.Count > 0)
                locationList.AddRange(Lobby.Singleton.Config.LobbyLocation
                    .Where(x => LobbyLocationHandler.LocationDatas.ContainsKey(x))
                    .Select(x => LobbyLocationHandler.LocationDatas[x]));

            if (Lobby.Singleton.Config.CustomLocations?.Count > 0)
                locationList.AddRange(Lobby.Singleton.Config.CustomLocations);

            if (Lobby.Singleton.Config.CustomRoomLocations?.Count > 0)
                locationList.AddRange(Lobby.Singleton.Config.CustomRoomLocations);

            if (locationList.Count <= 0)
                locationList.Add(LobbyLocationHandler.LocationDatas
                    .ElementAt(Random.Range(0, LobbyLocationHandler.LocationDatas.Count - 1)).Value);
            LobbyLocationHandler.SetLocation(locationList.RandomItem());
        }
        catch (Exception e)
        {
            Logger.Error("[Lobby] [Method: SpawnManager] " + e);
        }
    }

    private IEnumerator<float> RainbowColor()
    {
        _r = 255;
        _g = 0;
        _b = 0;

        while (!Round.IsRoundStarted)
        {
            if (_r > 0 && _b == 0)
            {
                _r -= 2;
                _g += 2;
            }

            if (_g > 0 && _r == 0)
            {
                _g -= 2;
                _b += 2;
            }

            if (_b > 0 && _g == 0)
            {
                _b -= 2;
                _r += 2;
            }

            _r = Mathf.Clamp(_r, 0, 255);
            _g = Mathf.Clamp(_g, 0, 255);
            _b = Mathf.Clamp(_b, 0, 255);

            yield return Timing.WaitForSeconds(0.4f);
        }
    }

    private IEnumerator<float> LobbyTimer()
    {
        while (!Round.IsRoundStarted)
        {
            LogManager.Debug("LobbyTimer tick start");
            _text = string.Empty;

            if (Lobby.Singleton.Config.VerticalPos < 0)
                for (var i = 0; i < ~Lobby.Singleton.Config.VerticalPos; i++)
                    _text += "\n";

            _text +=
                $"<size={(IsIntercom && Lobby.Singleton.Config.DisplayInIcom ? Lobby.Singleton.Config.TopTextIcomSize : Lobby.Singleton.Config.TopTextSize)}>" +
                Lobby.Singleton.Config.TitleText + "</size>";

            _text += "\n" +
                     $"<size={(IsIntercom && Lobby.Singleton.Config.DisplayInIcom ? Lobby.Singleton.Config.BottomTextIcomSize : Lobby.Singleton.Config.BottomTextSize)}>" +
                     Lobby.Singleton.Config.PlayerCountText + "</size>";

            var networkTimer = RoundStart.singleton.NetworkTimer;
            LogManager.Debug($"NetworkTimer value: {networkTimer}");

            switch (networkTimer)
            {
                case -2: _text = _text.Replace("{seconds}", Lobby.Singleton.Config.ServerPauseText); break;
                case -1: _text = _text.Replace("{seconds}", Lobby.Singleton.Config.RoundStartText); break;
                case 1:
                    _text = _text.Replace("{seconds}",
                        Lobby.Singleton.Config.SecondLeftText.Replace("{seconds}", networkTimer.ToString())); break;
                case 0: _text = _text.Replace("{seconds}", Lobby.Singleton.Config.RoundStartText); break;
                default:
                    _text = _text.Replace("{seconds}",
                        Lobby.Singleton.Config.SecondsLeftText.Replace("{seconds}", networkTimer.ToString())); break;
            }

            _text = Player.Count == 1
                ? _text.Replace("{players}", $"{Player.Count} " + Lobby.Singleton.Config.PlayerJoinText)
                : _text.Replace("{players}", $"{Player.Count} " + Lobby.Singleton.Config.PlayersJoinText);
            LogManager.Debug($"Player placeholder replaced, current player count: {Player.Count}");

            var hex = $"{_r:X2}{_g:X2}{_b:X2}";
            _text = _text.Replace("<rainbow>", $"<color=#{hex}>");
            _text = _text.Replace("</rainbow>", "</color>");
            LogManager.Debug($"Applied rainbow color: #{hex}");

            if (Lobby.Singleton.Config.VerticalPos >= 0)
                for (var i = 0; i < Lobby.Singleton.Config.VerticalPos; i++)
                    _text += "\n";

            if (!IsIntercom || !Lobby.Singleton.Config.DisplayInIcom)
            {
                LogManager.Debug($"Broadcasting to players. Player.List.Count = {Player.List.Count}");
                foreach (var ply in Player.List)
                    if (ply.ReferenceHub.Mode != ClientInstanceMode.Unverified &&
                        ply.ReferenceHub.Mode != ClientInstanceMode.DedicatedServer && ply != null)
                        ply.SendBroadcast(_text, (ushort)1.25, Broadcast.BroadcastFlags.Normal, true);
            }
            else
            {
                LogManager.Debug("Setting Intercom override text");
                IntercomDisplay._singleton.Network_overrideText =
                    $"<size={Lobby.Singleton.Config.IcomTextSize}>" + _text + "</size>";
            }

            LogManager.Debug("LobbyTimer tick end, waiting 1s");
            yield return Timing.WaitForSeconds(1f);
        }
    }

    #endregion
}