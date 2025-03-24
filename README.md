# Copy/Paste file content without clipboard
Sometimes you may need to copy/paste data between applications or environments with restrictions in place preventing you from using the clipboard. In these cases, instead of typing the content by hand, possibly for hours, you can use sloppyCopy.
This program parses the input file you specify and simulates keyboard events for each character of the file after user specified delay, effectively automating the task of typing out the content. SloppyCopy supports data transfer rates of approximate 2.70 kbps in normal mode and 0.5 kbps for portable mode.

# Rationale
The program is useful for pentesters in specific circumstances where traditional file upload mechanisms are restricted. For example, after performing a breakout from a citrix environment where clipboard is disabled, and outbound access is filtered (HTTP, DNS, etc.) 
In this case, you can still copy various lengthy scripts or executables to the target computer via sloppyCopy. In case of executables, use the ```--portable``` flag to have sloppyCopy first compress the file and base64 encode the content so that it can be simulated without error.
SloppyCopy now supports transfering HTML data via specially crafted data-uri ! Use the ```--uri``` flag to have sloppyCopy parse your input file and simulate a customized data-uri to transport your html code to the target environment.

# Demo (normal mode)
![](https://github.com/PN-Tester/sloppyCopy/blob/main/sloppyCopy_demo.gif)

# Demo (data uri)
![](https://github.com/PN-Tester/sloppyCopy/blob/main/data-uri.gif)

# Usage
```sloppyCopy.exe <file> <delay> [--portable] [--citrix] [--uri]```


Simply point sloppyCopy.exe at the file you wish to parse for input, and specify the time in seconds between run and start of keyboard simulation. In this timeframe, click somewhere in the target application where your keyboard events will go.

SloppyCopy can also be used on executable files, it will compress and base64 encode the data before performing the simulation. just add the optional ```--portable``` argument. Once the data has been transfered to your target, follow the instruction in the terminal output to base64 decode and decompress the binary to its original state.

When used with the ```--uri``` flag, sloppyCopy will perform some transformation on the input data in order to optimize transport. Your input html file will be gzip compressed using the DEFLATE algorithm, base64 encoded, and added to a sloppyCopy HTML template as the variable Base64GzipData. The template will then auto populate with the sloppyCopy title and theme, and include a custom Gzip decompression function which uses only native browser functionality, avoiding any dependencies on external libraries (in case your target environment has no outbound internet access). The template will include a custom <script> element which will dynamically decompress your original content into the template using HTML preprocessing. The resultant page is base64 encoded and sent via sloppyCopy simulation as a data uri to your target. Encapsulating the original document this way allows us to use compression while maintaining a standard URI with mime-type text/html, which can bypass most security policies (unlike application/gzip or equivalent).

CITRIX NOTE : Citrix environments use special scancodes added to keyboard events in order to differentiate between hardware generated events and the resultant citrix generated virtual event.
This behaviour causes a duplication of transfered data due to double registration of the keyboard event, meaning *abcdef* becomes *aabbccddeeff* on the target system! Very annoying!
To prevent this, use the ```--citrix``` flag for compatibility mode. This will send only the keyboard events with the scancodes so keypresses are only registered once in the citrix app.

```~``` character for some reason is not supported. If you absolutely need tilde, use ```--citrix``` with ```--portable``` option to copy the compressed base64 and avoid errors. 

WARNING : There is currently no way to stop sloppyCopy once the simulation begins, so ensure you have your cursor in the right place to receive keyboard events or things will get messy !
