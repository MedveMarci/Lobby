using System;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace Lobby.Command.ControlCommands;

public class AddLocationCommand : ICommand
{
    public string Command => "addloc";

    public string[] Aliases { get; } = ["add", "ad"];

    public string Description => "Add a new lobby location.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var playerSender = Player.Get(sender);

        if (playerSender == null)
        {
            response = "This command can only be used by a player!";
            return false;
        }

        if (!playerSender.HasAnyPermission("lobby.*", "lobby.control.*", "lobby.control.add"))
        {
            response = "You don't have permission to use this command!";
            return false;
        }

        if (arguments.Count != 1)
        {
            response = "Incorrect command, use: \nlobby addloc room\nlobby addloc static";
            return false;
        }

        if (playerSender.Room == null)
        {
            response = "You must be inside a room to add a room location!";
            return false;
        }

        switch (arguments.At(0))
        {
            case "room":
                var point = new GameObject("Point");
                point.transform.position = playerSender.Position;
                point.transform.rotation = playerSender.Rotation;
                point.transform.SetParent(playerSender.Room.Transform);

                var roomLocationData = new CustomRoomLocationData();
                roomLocationData.RoomNameType = playerSender.Room.GameObject.name;
                roomLocationData.OffsetX = point.transform.localPosition.x;
                roomLocationData.OffsetY = point.transform.localPosition.y + 0.05f;
                roomLocationData.OffsetZ = point.transform.localPosition.z;
                roomLocationData.RotationX = point.transform.localEulerAngles.x;
                roomLocationData.RotationY = point.transform.localEulerAngles.y;
                roomLocationData.RotationZ = point.transform.localEulerAngles.z;

                Lobby.Singleton.Config.CustomRoomLocations.Add(roomLocationData);
                Lobby.Singleton.SaveConfig();
                response = "New custom location added to the config.";
                return true;
            case "static":
                var locationData = new CustomLocationData();
                locationData.PositionX = playerSender.Position.x;
                locationData.PositionY = playerSender.Position.y + 0.05f;
                locationData.PositionZ = playerSender.Position.z;
                locationData.RotationX = playerSender.Rotation.x;
                locationData.RotationY = playerSender.Rotation.y;
                locationData.RotationZ = playerSender.Rotation.z;

                Lobby.Singleton.Config.CustomLocations.Add(locationData);
                Lobby.Singleton.SaveConfig();
                response = "New custom location added to the config.";
                return true;
            default:
                response = "Incorrect command, use: \nlobby addloc room\nlobby addloc static";
                return false;
        }
    }
}