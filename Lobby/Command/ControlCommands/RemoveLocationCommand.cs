using System;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace Lobby.Command.ControlCommands;

public class RemoveLocationCommand : ICommand
{
    public string Command => "removeloc";

    public string[] Aliases { get; } = ["remloc", "rloc", "rl"];

    public string Description => "Remove a new lobby location.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var playerSender = Player.Get(sender);

        if (playerSender == null)
        {
            response = "This command can only be used by a player!";
            return false;
        }

        if (!playerSender.HasAnyPermission("lobby.*", "lobby.control.*", "lobby.control.remove"))
        {
            response = "You don't have permission to use this command!";
            return false;
        }

        if (arguments.Count != 2)
        {
            response = "Incorrect command, use: \nlobby removeloc room (index)\nlobby removeloc static (index)";
            return false;
        }

        if (!int.TryParse(arguments.At(1), out var index))
        {
            response = "The index must be a number!";
            return false;
        }

        switch (arguments.At(0))
        {
            case "room":
                if (Lobby.Singleton.Config.CustomRoomLocations == null ||
                    Lobby.Singleton.Config.CustomRoomLocations?.Count - 1 < index)
                {
                    response = $"Custom location at index {index} was not found.";
                    return false;
                }

                Lobby.Singleton.Config.CustomRoomLocations.RemoveAt(index);
                Lobby.Singleton.SaveConfig();

                response = $"Custom location at index {index} has been removed.";
                return true;
            case "static":
                if (Lobby.Singleton.Config.CustomLocations == null ||
                    Lobby.Singleton.Config.CustomLocations?.Count - 1 < index)
                {
                    response = $"Custom location at index {index} was not found.";
                    return false;
                }

                Lobby.Singleton.Config.CustomLocations.RemoveAt(index);
                Lobby.Singleton.SaveConfig();

                response = $"Custom location at index {index} has been removed.";
                return true;
            default:
                response = "Incorrect command, use: \nlobby removeloc room (index)\nlobby removeloc static (index)";
                return false;
        }
    }
}