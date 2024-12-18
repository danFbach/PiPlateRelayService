using Instances;

using PiPlateRelay.Models;

using System.Diagnostics;

namespace PiPlateRelay;

internal class PiPlateServer
{

    private readonly IProcessInstance _pythonInstance;

    public PiPlateServer()
    {
        _pythonInstance = Instance.Start("python", "ZipZapPy.py", PythonEventHandlers.HandleOutput!, PythonEventHandlers.HandleError!);
    }

    public Task<List<string>> SendAsync(PiPlateRelayCommand command)
    {
        if (!command.IsConfigured)
            throw new PiPlateRelayException("Next command is not fully configured.");
        var result = SendCommandAsync(command.CommandType, command.BoardAddress ?? 0, command.CommandArg);
        command.CommandArg = default;
        return result;
    }

    public List<string> Send(PiPlateRelayCommand command)
    {
        if (!command.IsConfigured)
            throw new PiPlateRelayException("Next command is not fully configured.");

        var result = SendCommand(command.CommandType, command.BoardAddress ?? 0, command.CommandArg);
        command.CommandArg = default;
        return result;
    }

    internal async Task<List<string>> SendCommandAsync<T>(T command, int boardAddress, int? arg = default)
    {
        if (command is SystemCommands sysCmd)
        {
            if (systemCommands.TryGetValue(sysCmd, out var commandString))
                return await PipeCommandAsync($"{commandString} {boardAddress}");
        }
        else if (command is RelayCommands relayCommand)
        {
            if (relayCommands.TryGetValue(relayCommand, out var commandString))
                return await PipeCommandAsync($"{commandString} {boardAddress}-{arg}");
        }
        else if (command is LEDCommands LEDCommand)
        {
            if (ledCommands.TryGetValue(LEDCommand, out var commandString))
                return await PipeCommandAsync($"{commandString} {boardAddress}");
        }

        return [];
    }

    internal List<string> SendCommand<T>(T command, int boardAddress, int? arg = default)
    {
        if (command is SystemCommands sysCmd)
        {
            if (systemCommands.TryGetValue(sysCmd, out var commandString))
                return PipeCommand($"{commandString} {boardAddress}");
        }
        else if (command is RelayCommands relayCommand)
        {
            if (!arg.HasValue)
                throw new PiPlateRelayException("Relay commands require a relay argument.");

            if (relayCommands.TryGetValue(relayCommand, out var commandString))
                return PipeCommand($"{commandString} {boardAddress}-{arg}");
        }
        else if (command is LEDCommands LEDCommand)
        {
            if (ledCommands.TryGetValue(LEDCommand, out var commandString))
                return PipeCommand($"{commandString} {boardAddress}");
        }

        return [];
    }

    private async Task<List<string>> PipeCommandAsync(string command)
    {
        await _pythonInstance.SendInputAsync($"{command}{Environment.NewLine}");

        return AwaitServerResponse();
    }

    private List<string> PipeCommand(string command)
    {
        _pythonInstance.SendInput($"{command}{Environment.NewLine}");

        return AwaitServerResponse();
    }

    private static List<string> AwaitServerResponse()
    {
        var response = new List<string>();
        var sw = new Stopwatch();
        while (true)
        {
            if (PythonEventHandlers.OutputQueue.TryDequeue(out var r))
            {
                if (!string.IsNullOrEmpty(r))
                {
                    response.Add(r);
                    if (sw.IsRunning)
                        sw.Restart();
                    else
                        sw.Start();
                }
            }
            if (sw.IsRunning && sw.ElapsedMilliseconds > 5)
                break;

            Thread.Sleep(1);
        }
        return response;
    }

    public async Task<List<BoardModel>> InitAsync()
    {
        var response = AwaitServerResponse();

        if (response.Any(x => x.Contains("server ready")))
            return await GetBoardsInfoAsync();

        throw new PyPipeException("Failed to initialize.");
    }

    public async Task<List<BoardModel>> GetBoardsInfoAsync()
    {
        var liveBoards = new List<BoardModel>();
        for (var i = 0; i < 1; i++)
        {
            var response = await SendCommandAsync(SystemCommands.GetAddress, i);
            foreach (var r in response)
            {
                if (r.Contains("success: "))
                {
                    var status = r.Replace("success: ", string.Empty);
                    if (int.TryParse(status, out var value) &&
                        value == i)
                    {
                        liveBoards.Add(await GetBoardInfoAsync(i));
                    }
                }
            }
        }

        return liveBoards;
    }

    public async Task<BoardModel> GetBoardInfoAsync(int boardAddress)
    {
        return new()
        {
            Address = boardAddress,
            Id = StripStatusInfo(await SendCommandAsync(SystemCommands.GetId, boardAddress)),
            FW = StripStatusInfo(await SendCommandAsync(SystemCommands.GetFWRevision, boardAddress)),
            HW = StripStatusInfo(await SendCommandAsync(SystemCommands.GetHWRevision, boardAddress)),
        };
    }

    private static string StripStatusInfo(IEnumerable<string> response) => StripStatusInfo(string.Join(" ", response));

    private static string StripStatusInfo(string response) => response.Replace("success: ", string.Empty).Replace("error: ", string.Empty);

    public void ExitAsync()
    {
        _pythonInstance.Kill();
    }


    private static readonly Dictionary<RelayCommands, string> relayCommands = new()
    {
        { RelayCommands.Toggle, "relay.toggle" },
        { RelayCommands.On, "relay.on" },
        { RelayCommands.Off, "relay.off" },
        { RelayCommands.State, "relay.state" },
    };

    private static readonly Dictionary<LEDCommands, string> ledCommands = new()
    {
        { LEDCommands.Set, "LED.set" },
        { LEDCommands.Clear, "LED.clear" },
        { LEDCommands.Toggle, "LED.toggle" },
    };

    private static readonly Dictionary<SystemCommands, string> systemCommands = new()
    {
        { SystemCommands.GetId, "system.getId" },
        { SystemCommands.GetFWRevision, "system.getFWRev" },
        { SystemCommands.GetHWRevision, "system.getHWRev" },
        { SystemCommands.GetPMRevision, "system.getPMRev" },
        { SystemCommands.GetAddress, "system.getAddress" },
        { SystemCommands.Reset, "system.reset" },
    };
}

public static class PythonEventHandlers
{
    public readonly static Queue<string> OutputQueue = new();
    public readonly static Queue<string> ErrorQueue = new();

    public static void HandleOutput(object _, string message)
    {
        OutputQueue.Enqueue(message);
    }

    public static void HandleError(object _, string message)
    {
        ErrorQueue.Enqueue(message);
    }
}

public enum RelayCommands
{
    Toggle,
    On,
    Off,
    State
}

public enum LEDCommands
{
    Set,
    Clear,
    Toggle
}

public enum SystemCommands
{
    GetId,
    GetFWRevision,
    GetHWRevision,
    GetPMRevision,
    GetAddress,
    Reset
}