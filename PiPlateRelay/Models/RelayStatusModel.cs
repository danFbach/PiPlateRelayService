namespace PiPlateRelay.Models;

public class RelayStatusModel
{
    public bool Relay1 => BinaryStatus.Length >= 1 && BinaryStatus[0] == '1';

    public bool Relay2 => BinaryStatus.Length >= 2 && BinaryStatus[1] == '1';

    public bool Relay3 => BinaryStatus.Length >= 3 && BinaryStatus[2] == '1';

    public bool Relay4 => BinaryStatus.Length >= 4 && BinaryStatus[3] == '1';

    public bool Relay5 => BinaryStatus.Length >= 5 && BinaryStatus[4] == '1';

    public bool Relay6 => BinaryStatus.Length >= 6 && BinaryStatus[5] == '1';

    public bool Relay7 => BinaryStatus.Length >= 7 && BinaryStatus[6] == '1';

    public bool[] Statuses => [Relay1, Relay2, Relay3, Relay4, Relay5, Relay6, Relay7];

    private string BinaryStatus { get; set; } = string.Empty;

    public RelayStatusModel() { }

    public RelayStatusModel(string statusString)
    {
        if (ushort.TryParse(statusString, out var status))
            BinaryStatus = string.Join(string.Empty, Convert.ToString(status, 2).Reverse());
    }
}
