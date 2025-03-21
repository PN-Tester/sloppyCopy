# Copy/Paste file content without clipboard
Sometimes you may need to copy/paste data between applications or environments with restrictions in place preventing you from using the clipboard. In these cases, instead of typing the content by hand, possibly for hours, you can use sloppyCopy.
This program parses the input file you specify and simulates keyboard events for each character of the file after user specified delay, effectively automating the task of typing out the content. SloppyCopy supports data transfer rates of approximate 2.70 kpbs in normal mode and 0.5 kbps for portable mode.

# Rational
The program is useful for pentesters in specific circumstances where traditional file upload mechanisms are restricted. For example, after performing a breakout from a citrix environment where clipboard is disabled, and outbound access is filtered (HTTP, DNS, etc.) 
In this case, you can still copy various lengthy scripts or executables to the target computer via sloppyCopy. In case of executables, use the ```--portable``` flag to have sloppyCopy first compress the file and base64 encode the content so that it can be simulated without error.

# Demo
![](https://github.com/PN-Tester/sloppyCopy/blob/main/sloppyCopy_demo.gif)

# Usage
```sloppyCopy.exe <file> <delay> [--portable] [--citrix]```


Simply point sloppyCopy.exe at the file you wish to parse for input, and specify the time in seconds between run and start of keyboard simulation. In this timeframe, click somewhere in the target application where your keyboard events will go.

SloppyCopy can also be used on executable files, it will compress and base64 encode the data before performing the simulation. just add the optional ```--portable``` argument. Once the data has been transfered to your target, follow the instruction in the terminal output to base64 decode and decompress the binary to its original state.

CITRIX NOTE : Citrix environments use special scancodes added to keyboard events in order to differentiate between hardware generated events and the resultant citrix generated virtual event.
This behaviour causes a duplication of transfered data due to double registration of the keyboard event, meaning *abcdef* becomes *aabbccddeeff* on the target system! Very annoying!
To prevent this, use the ```--citrix``` flag for compatibility mode. This will send only the keyboard events with the scancodes so keypresses are only registered once in the citrix app.
I am still working on support for the pesky ```~``` character which for some reason is not supported. If you absolutely need tilde, use ```--citrix``` with ```--portable``` option to copy the compressed base64 and avoid errors. 

WARNING : There is currently no way to stop sloppyCopy once the simulation begins, so ensure you have your cursor in the right place to receive keyboard events or things will get messy !
