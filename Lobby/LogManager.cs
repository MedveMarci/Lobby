using System;
using LabApi.Features.Console;

namespace Lobby;

internal abstract class LogManager
{
    public static bool DebugEnabled => Lobby.Instance.Config.Debug;

    public static void Debug(string message)
    {
        if (!DebugEnabled)
            return;

        Logger.Raw($"[DEBUG] [{Lobby.Instance.Name}] {message}", ConsoleColor.Green);
    }

    public static void Info(string message, ConsoleColor color = ConsoleColor.Cyan)
    {
        Logger.Raw($"[INFO] [{Lobby.Instance.Name}] {message}", color);
    }

    public static void Warn(string message)
    {
        Logger.Warn(message);
    }

    public static void Error(string message)
    {
        Logger.Raw($"[ERROR] [{Lobby.Instance.Name}] Details:\nVersion: {Lobby.Instance.Version}\n{message}", ConsoleColor.Red);
    }
}