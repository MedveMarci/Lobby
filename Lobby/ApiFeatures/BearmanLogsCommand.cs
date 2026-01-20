using System;
using CommandSystem;

namespace Lobby.ApiFeatures;

[CommandHandler(typeof(GameConsoleCommandHandler))]
public class BearmanLogsLobby: ICommand
{
    public string Command => "bearmanlogsLobby";

    public string[] Aliases { get; } = ["bmlogsLobby"];

    public string Description => "Sends collected plugin logs to the log server and returns the log id.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var getLogHistory = LogManager.GetLogHistory();
        response = getLogHistory.logResult;
        return getLogHistory.success;
    }
}