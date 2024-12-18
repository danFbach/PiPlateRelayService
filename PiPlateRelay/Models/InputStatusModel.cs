namespace PiPlateRelay;

public class InputStatusModel
{
    public bool Input1 => BinaryStatus.Length >= 1 && BinaryStatus[0] == '1';

    public bool Input2 => BinaryStatus.Length >= 2 && BinaryStatus[1] == '1';

    public bool Input3 => BinaryStatus.Length >= 3 && BinaryStatus[2] == '1';

    public bool Input4 => BinaryStatus.Length >= 4 && BinaryStatus[3] == '1';

    public bool Input5 => BinaryStatus.Length >= 5 && BinaryStatus[4] == '1';

    public bool Input6 => BinaryStatus.Length >= 6 && BinaryStatus[5] == '1';

    public bool Input7 => BinaryStatus.Length >= 7 && BinaryStatus[6] == '1';

    private string BinaryStatus { get; set; } = string.Empty;

    public InputStatusModel() { }

    public InputStatusModel(string statusString)
    {
        Console.WriteLine($"raw status: {statusString}");
        if (ushort.TryParse(statusString, out var status))
        {
            BinaryStatus = string.Join(string.Empty, Convert.ToString(status, 2).Reverse());
            Console.WriteLine($"binary status: {BinaryStatus}");
        }
    }

    public override string ToString()
    {
        return $"[ DI1: {(Input1 ? "on" : "off")}, DI2: {(Input2 ? "on" : "off")}, DI3: {(Input3 ? "on" : "off")}" +
            $", DI4: {(Input4 ? "on" : "off")}, DI5: {(Input5 ? "on" : "off")}, DI6: {(Input6 ? "on" : "off")}" +
            $", DI7: {(Input7 ? "on" : "off")} ]";
    }
}
