using System;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace Lobby.Command.DebugCommands;

public class GetLocalDataCommand : ICommand
{
    public string Command => "getlocaldata";

    public string[] Aliases { get; } = ["getlocdat", "getld", "gld"];

    public string Description => "Shows the exact position and rotation in the room where the player is located.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var playerSender = Player.Get(sender);

        if (playerSender == null)
        {
            response = "This command can only be used by a player!";
            return false;
        }

        if (!playerSender.HasAnyPermission("lobby.*", "lobby.debug.*", "lobby.debug.getld"))
        {
            response = "You don't have permission to use this command!";
            return false;
        }

        if (arguments.Count > 0)
        {
            response = "Incorrect command, use: \nlobby getlocalpos";
            return false;
        }

        if (playerSender.Room == null)
        {
            response = "An error occurred when getting the room you are in.";
            return false;
        }

        var point = new GameObject("Point");
        point.transform.position = playerSender.Position;
        point.transform.rotation = playerSender.Rotation;
        point.transform.SetParent(playerSender.Room.Transform);

        response =
            $"Room name {playerSender.Room.GameObject.name}; Local position: {point.transform.localPosition.ToString()}; Local Rotation: {point.transform.localEulerAngles.ToString()}.";
        return true;
    }
}