# PiPlateRelayDotNet
This is a C# wrapper for the PiPlateRELAY board.
It builds a python server which interacts natively with the PiPlate.
A c# class library is then used as a wrapper to interact with the python server.

By referencing this library, you can easily control the board.
```c#

var service = new PiPlateService();
await service.TryInitializeAsync();

await service.System.GetBoardInfoAsync(0);

await service.Relay.On(0, 1);

await service.Relay.Off(0, 1);

await service.Relay.Toggle(0, 1;

var status = await service.Relay.Status(0);
```
