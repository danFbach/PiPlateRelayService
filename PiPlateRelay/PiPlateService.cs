using PiPlateRelay.Models;

namespace PiPlateRelay;

public class PiPlateService
{
    private readonly PiPlateServer _server;

    private static readonly List<BoardModel> _availableBoards = [];

    private readonly static PiPlateRelayCommand _nextCommand = new();

    private static bool Initialized { get; set; }

    public RelayHandler Relay;

    public SystemHandler System;

    public PiPlateService()
    {
        _server = new();
        Relay = new RelayHandler(_server);
        System = new SystemHandler(_server);
    }

    private async Task InitServiceAsync()
    {
        Console.Write("Initializing PiPlate python server...");
        _availableBoards.Clear();
        _availableBoards.AddRange(await _server.InitAsync());
        Initialized = true;
        Console.Write($"Complete!{Environment.NewLine}");
    }

    public Task TryInitializeAsync()
    {
        return !Initialized ? InitServiceAsync() : Task.CompletedTask;
    }

    private static class Board
    {
        internal static void Use(int boardAddress)
        {
            if (_availableBoards.Any(x => x.Address == boardAddress))
                _nextCommand.BoardAddress = boardAddress;
            else
                throw new PiPlateRelayException($"Board {boardAddress} does not exist.");
        }
    }

    public class SystemHandler
    {
        private readonly PiPlateServer _server;

        internal SystemHandler(PiPlateServer server)
        {
            _server = server;
        }

        public Task<BoardModel> GetBoardInfoAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            return _nextCommand.BoardAddress.HasValue ? _server.GetBoardInfoAsync(_nextCommand.BoardAddress.Value) : Task.FromResult(new BoardModel());
        }

        public async Task<string> HardwareVersionAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = SystemCommands.GetHWRevision;
            return StripStatusInfo(await _server.SendAsync(_nextCommand));
        }

        public async Task<string> FirmwareVersionAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = SystemCommands.GetFWRevision;
            return StripStatusInfo(await _server.SendAsync(_nextCommand));
        }

        public Task ToggleLEDAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = LEDCommands.Toggle;
            return _server.SendAsync(_nextCommand);
        }

        public Task LEDOnAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = LEDCommands.Set;
            return _server.SendAsync(_nextCommand);
        }

        public Task LEDOffAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = LEDCommands.Clear;
            return _server.SendAsync(_nextCommand);
        }
    }

    public class RelayHandler
    {
        private readonly PiPlateServer _server;

        internal RelayHandler(PiPlateServer server)
        {
            _server = server;
        }

        public Task OnAsync(int boardAddress, int relayNumber)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = RelayCommands.On;
            _nextCommand.CommandArg = relayNumber;
            return _server.SendAsync(_nextCommand);
        }

        public Task OffAsync(int boardAddress, int relayNumber)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = RelayCommands.Off;
            _nextCommand.CommandArg = relayNumber;
            return _server.SendAsync(_nextCommand);
        }

        public Task ToggleAsync(int boardAddress, int relayNumber)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = RelayCommands.Toggle;
            _nextCommand.CommandArg = relayNumber;
            return _server.SendAsync(_nextCommand);
        }

        public async Task<RelayStatusModel> StatusAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = RelayCommands.State;
            var response = await _server.SendAsync(_nextCommand);
            return new RelayStatusModel(StripStatusInfo(response));
        }

        public Task ResetAsync(int boardAddress)
        {
            Board.Use(boardAddress);
            _nextCommand.CommandType = SystemCommands.Reset;
            return _server.SendAsync(_nextCommand);
        }
    }

    private static string StripStatusInfo(IEnumerable<string> response) => StripStatusInfo(string.Join(" ", response));

    private static string StripStatusInfo(string response) => response.Replace("success: ", string.Empty).Replace("error: ", string.Empty);
}

