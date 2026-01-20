using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace Lobby.Command.DebugCommands;

public class TpLocationCommand : ICommand
{
    public string Command => "tploc";

    public string[] Aliases { get; } = ["tpl", "tl"];

    public string Description => "Teleport to a custom location.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var playerSender = Player.Get(sender);

        if (playerSender == null)
        {
            response = "This command can only be used by a player!";
            return false;
        }

        if (!playerSender.HasAnyPermission("lobby.*", "lobby.debug.*", "lobby.debug.tp"))
        {
            response = "You don't have permission to use this command!";
            return false;
        }

        if (arguments.Count != 2)
        {
            response = "Incorrect command, use: \nlobby tploc room (index)\nlobby tploc static (index)";
            return false;
        }

        if (!int.TryParse(arguments.At(1), out var index))
        {
            response = "The index must be a number";
            return false;
        }

        var point = new GameObject("Point");

        switch (arguments.At(0))
        {
            case "room":
                if (Lobby.Singleton.Config.CustomRoomLocations == null ||
                    Lobby.Singleton.Config.CustomRoomLocations?.Count - 1 < index)
                {
                    response = $"Custom location at index {index} was not found.";
                    return false;
                }

                if (Enum.TryParse(Lobby.Singleton.Config.CustomRoomLocations[index].RoomNameType,
                        out RoomName roomName))
                    point.transform.SetParent(RoomIdentifier.AllRoomIdentifiers.First(x => x.Name == roomName)
                        .transform);
                else if (RoomIdentifier.AllRoomIdentifiers.Count(x =>
                             x.name.Contains(Lobby.Singleton.Config.CustomRoomLocations[index].RoomNameType)) > 0)
                    point.transform.SetParent(RoomIdentifier.AllRoomIdentifiers.First(x =>
                        x.name.Contains(Lobby.Singleton.Config.CustomRoomLocations[index].RoomNameType)).transform);

                point.transform.localPosition =
                    new Vector3(Lobby.Singleton.Config.CustomRoomLocations[index].OffsetX,
                        Lobby.Singleton.Config.CustomRoomLocations[index].OffsetY,
                        Lobby.Singleton.Config.CustomRoomLocations[index].OffsetZ);
                point.transform.localEulerAngles = new Vector3(
                    Lobby.Singleton.Config.CustomRoomLocations[index].RotationX,
                    Lobby.Singleton.Config.CustomRoomLocations[index].RotationY,
                    Lobby.Singleton.Config.CustomRoomLocations[index].RotationZ);

                playerSender.Position = point.transform.position;
                playerSender.Rotation = point.transform.rotation;

                GameObject.Destroy(point);

                response = $"You have successfully teleported to a custom location at index {index}.";
                return true;
            case "static":
                if (Lobby.Singleton.Config.CustomLocations == null ||
                    Lobby.Singleton.Config.CustomLocations?.Count - 1 < index)
                {
                    response = $"Custom location at index {index} was not found.";
                    return false;
                }

                playerSender.Position = new Vector3(Lobby.Singleton.Config.CustomLocations[index].PositionX,
                    Lobby.Singleton.Config.CustomLocations[index].PositionY,
                    Lobby.Singleton.Config.CustomLocations[index].PositionZ);
                playerSender.Rotation = Quaternion.Euler(Lobby.Singleton.Config.CustomLocations[index].RotationX,
                    Lobby.Singleton.Config.CustomLocations[index].RotationY,
                    Lobby.Singleton.Config.CustomLocations[index].RotationZ);

                response = $"You have successfully teleported to a custom location at index {index}.";
                return true;
            default:
                response = "Incorrect command, use: \nlobby tploc room (index)\nlobby tploc static (index)";
                return false;
        }
    }
}