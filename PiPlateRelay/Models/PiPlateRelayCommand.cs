namespace PiPlateRelay.Models;

internal class PiPlateRelayCommand
{

    public int? BoardAddress { get; set; }

    public object? CommandType { get; set; }

    public int? CommandArg { get; set; }

    internal bool IsConfigured => CommandType != null && BoardAddress.HasValue;
}
