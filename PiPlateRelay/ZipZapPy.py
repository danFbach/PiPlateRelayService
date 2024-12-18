import piplates.RELAYplate as _relay
import sys

print('server ready');
sys.stdout.flush();

try:
    while True:

        line = input();
        
        if not line or line == '':
            break;

        if line == "exit":
            exit;

        args = line.split(' ');

        if len(args) != 2:
            break;
        
        command = args[0];
        relayNumber = 0;
        boardAddress = 0;
                    
        relayCommands = [ 'relay.toggle', 'relay.on', 'relay.off', 'relay.state' ];

        ledCommands = [ 'LED.set', 'LED.clear', 'LED.toggle' ];

        systemCommands = [ 'system.getId', 'system.getFWRev', 'system.getHWRev', 'system.getPMRev', 'system.getAddress', 'system.reset' ];

        if command in relayCommands:
                
            relayParams = args[1].split('-');
            boardAddressString = relayParams[0];
            relayNumString = relayParams[1];

            if str(relayNumString).isdigit():
                relayNumber = int(relayNumString);

            if str(boardAddressString).isdigit():
                boardAddress = int(boardAddress);

            if boardAddress < 0 | boardAddress > 7:
                print('error: invalid board address. 0-7');
                sys.stdout.flush();
                # break;

            if relayNumber < 1 | relayNumber > 7:
                print('error: invalid relay selection. 1-7');
                sys.stdout.flush();
                # break;

            if command == 'relay.toggle':
                _relay.relayTOGGLE(boardAddress, relayNumber);
                print(f"success: relay {relayNumber} on board {boardAddress} toggled");
                sys.stdout.flush();
                # break;

            elif command == 'relay.on':            
                _relay.relayON(boardAddress, relayNumber);
                print(f"success: relay {relayNumber} on board {boardAddress} turned on");
                sys.stdout.flush();
                # break;
                                
            elif command == 'relay.off':            
                _relay.relayOFF(boardAddress, relayNumber);
                print(f"success: relay {relayNumber} on board {boardAddress} turned off");
                sys.stdout.flush();
                # break;

            elif command == 'relay.state':
                relayStates = _relay.relaySTATE(boardAddress);
                print(f"success: {relayStates}");
                sys.stdout.flush();
                # break;

            else: 
                print('error: unsupported function'); 
                sys.stdout.flush(); 
                # break;

        elif command in ledCommands:

            ledParams = args[1];

            if str(ledParams).isdigit():
                boardAddress = int(ledParams);

            if boardAddress < 0 | boardAddress > 7:
                print('error: invalid board address. 0-7'); 
                sys.stdout.flush(); 
                # break;

            if command == 'LED.set':
                _relay.setLED(boardAddress);
                print(f"success: LED set on board {boardAddress}");
                sys.stdout.flush();
                # break;

            elif command == 'LED.clear':
                _relay.clrLED(boardAddress);                    
                print(f"success: LED cleared on board {boardAddress}");
                sys.stdout.flush();
                # break;

            elif command == 'LED.toggle':
                _relay.toggleLED(boardAddress);   
                print(f"success: LED toggled on board {boardAddress}");
                sys.stdout.flush();
                # break;

            else: 
                print('error: unsupported function'); 
                sys.stdout.flush(); 
                # break;

        elif command in systemCommands:

            sysParams = args[1];
            boardAddress = 0;
            if str(sysParams).isdigit():
                boardAddress = int(sysParams);

            if boardAddress < 0 | boardAddress > 7:
                print('error: invalid board address. 0-7'); 
                sys.stdout.flush(); 
                # break;

            resp = '';
                
            if command == 'system.getId':
                resp = _relay.getID(boardAddress);
                print(f'success: {resp}'); 
                sys.stdout.flush(); 
                # break;
            elif command == 'system.getFWRev':
                resp = _relay.getFWrev(boardAddress);
                print(f'success: {resp}'); 
                sys.stdout.flush(); 
                # break;
            elif command == 'system.getHWRev':
                resp = _relay.getHWrev(boardAddress);
                print(f'success: {resp}'); 
                sys.stdout.flush(); 
                # break;
            elif command == 'system.getPMRev':
                resp = _relay.getPMrev();
                print(f'success: {resp}'); 
                sys.stdout.flush(); 
                # break;
            elif command == 'system.getAddress':
                resp = _relay.getADDR(boardAddress);
                print(f'success: {resp}'); 
                sys.stdout.flush(); 
                # break;
            elif command == 'system.reset':
                resp = _relay.RESET(boardAddress);
                print(f'success: board {resp} reset'); 
                sys.stdout.flush(); 
                # break;
            else: 
                print('error: unsupported function'); 
                sys.stdout.flush(); 
                # break;

        else: 
            print('error: unsupported function');
            sys.stdout.flush(); 
            # break;

except KeyboardInterrupt:
    print("\nScript stopped.")
