# Copy/Paste file content without clipboard
Sometimes you may need to copy paste data between applications or environments with restrictions in place preventing you from using the clipboard. In these cases, instead of typing the content by hand, possibly for hours, you can use sloppyCopy.
This program parses the input file you specify and simulates keyboard events for each character of the file after user specified delay, effectively automating the task of typing out the content.

# Rational
The program is useful for pentesters in specific circumstances where traditional file upload mechanisms are restricted. For example, after performing a breakout from a citrix environment where clipboard is disabled, and outbound access is filtered (HTTP, DNS, etc.) 
In this case, you can still copy various lengthy scripts to the target computer via sloppyCopy.

# Usage
```sloppyCopy.exe <file> <delay>```


Simply point sloppyCopy.exe at the file you wish to parse for input, and specify the time in seconds between run and start of keyboard simulation. In this timeframe, click somewhere in the target application where your keyboard events will go.

WARNING : There is currently no way to stop sloppyCopy once the simulation begins, so ensure you have your cursor in the right place to receive keyboard events or things will get messy !
