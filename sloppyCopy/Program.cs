using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics; // For Process.Start()

class Program
{
    // Import user32.dll to simulate keyboard events
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    // Virtual key codes for the keyboard
    const byte VK_SHIFT = 0x10;
    const byte VK_SPACE = 0x20;
    const byte VK_RETURN = 0x0D;
    const byte VK_BACK = 0x08;
    const byte VK_COMMA = 0xBC; // Comma
    const byte VK_PERIOD = 0xBE; // Period
    const byte VK_1 = 0x31; // Number 1
    const byte VK_2 = 0x32; // Number 2
    const byte VK_3 = 0x33; // Number 3
    const byte VK_4 = 0x34; // Number 4
    const byte VK_5 = 0x35; // Number 5
    const byte VK_6 = 0x36; // Number 6
    const byte VK_7 = 0x37; // Number 7
    const byte VK_8 = 0x38; // Number 8
    const byte VK_9 = 0x39; // Number 9
    const byte VK_0 = 0x30; // Number 0
    const byte VK_A = 0x41; // A
    const byte VK_Z = 0x5A; // Z

    static readonly byte[] scanCodes = new byte[]
   {
    0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, // 1-0
    0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, // A-Z
    0x1E, // [
    0x1F, // ]
    0x2B, // \ 
    0x27, // ; 
    0x28, // '
    0x33, // ,
    0x34, // .
    0x35, // /
    0x29, // ` (backtick)
    0x1C, // Enter (newline)
    0x00 // Placeholder for unhandled characters
   };

    static void Main(string[] args)
    {
        if (args.Length < 2 || args.Length > 4) // Adjust the argument length check
        {
            Console.WriteLine("Usage: sloppyCopy.exe <file> <delay> [--portable] [--citrix]");
            return;
        }

        string filePath = args[0];

        if (!File.Exists(filePath))
        {
            Console.WriteLine("[-] File not found: " + filePath);
            return;
        }

        if (!int.TryParse(args[1], out int delayInSeconds))
        {
            Console.WriteLine("[-] Invalid number for delay.");
            return;
        }

        bool isPortable = false;
        bool isCitrix = false;

        // Check if --portable or --citrix flags are present
        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--portable")
            {
                isPortable = true;
            }
            else if (args[i] == "--citrix")
            {
                isCitrix = true;
                Console.WriteLine("[!] Running in Citrix Compatibility Mode");
            }
        }

        string content;
        long fileSize = new FileInfo(filePath).Length; // Get the size of the original file in bytes
        Console.WriteLine($"[!] Original file size: {fileSize} bytes");

        // Set the transfer rate to 64 bytes per second if either Citrix or Portable mode is active
        double transferRate = (isPortable) ? 64.0 : 336.0;

        // Estimate time for regular file
        double estimatedTimeSeconds = fileSize / transferRate;
        TimeSpan estimatedTime = TimeSpan.FromSeconds(estimatedTimeSeconds);

        if (!isPortable)
        {
            Console.WriteLine($"[!] Estimated time to completion (regular): {estimatedTime.Minutes} minutes {estimatedTime.Seconds} seconds");
        }

        if (isPortable)
        {
            string tempTarGz = Path.Combine(Path.GetTempPath(), "temp_sloppyCopy.tar.gz");

            Console.WriteLine("[!] Creating compressed copy...");
            Process.Start("tar", $"-czf \"{tempTarGz}\" \"{filePath}\"")?.WaitForExit();

            if (!File.Exists(tempTarGz))
            {
                Console.WriteLine("[-] Compression failed.");
                return;
            }

            // Read the compressed file into a byte array
            byte[] compressedData = File.ReadAllBytes(tempTarGz);

            // Convert to Base64
            string base64Data = Convert.ToBase64String(compressedData);

            // Get the actual size of the Base64 encoded data
            long base64Size = base64Data.Length;
            Console.WriteLine($"[!] Base64 compressed data size: {base64Size} bytes");

            // Estimate time based on the actual size of the Base64 data
            double estimatedPortableTimeSeconds = base64Size / transferRate;
            TimeSpan estimatedPortableTime = TimeSpan.FromSeconds(estimatedPortableTimeSeconds);
            Console.WriteLine($"[!] Estimated time to completion (Base64 compressed): {estimatedPortableTime.Minutes} minutes {estimatedPortableTime.Seconds} seconds");

            content = base64Data;

            // Clean up the temporary compressed file
            File.Delete(tempTarGz);
        }
        else
        {
            content = File.ReadAllText(filePath);
        }

        Console.WriteLine($"[!] Waiting for {delayInSeconds} seconds...");
        Thread.Sleep(delayInSeconds * 1000);

        foreach (char c in content)
        {
            SimulateKeyPress(c, isCitrix);

            if (isPortable)
            {
                Thread.Sleep(1);  // Add delay for portable mode otherwise characters are desynced
            }
        }

        Console.WriteLine("[+] Simulation complete.");
        if (isPortable)
        {
            Console.WriteLine("[!] You can decompress data with :\ncertutil -decode input.txt output.tar.gz && tar -xf output.tar.gz");
        }
    }



    // Simulate a key press using keybd_event
    static void SimulateKeyPress(char c, bool isCitrix)
    {
        byte vkCode = 0;
        bool shiftRequired = false;

        // Handle printable characters
        if (c >= 'a' && c <= 'z')
        {
            vkCode = (byte)(c - 'a' + VK_A); // Lowercase letters
        }
        else if (c >= 'A' && c <= 'Z')
        {
            vkCode = (byte)(c - 'A' + VK_A); // Uppercase letters
            shiftRequired = true; // Shift needed for uppercase
        }
        else if (c == ' ')
        {
            vkCode = VK_SPACE;  // Spacebar
        }
        else if (c == '\n')
        {
            vkCode = VK_RETURN; // Enter key
        }
        else if (c == '\b')
        {
            vkCode = VK_BACK;   // Backspace key
        }
        else if (c >= '0' && c <= '9') // Handle numbers
        {
            vkCode = (byte)(c - '0' + VK_0);
        }
        else
        {
            // Handle special characters like punctuation
            vkCode = GetSpecialCharacterVKCode(c);
            if (vkCode == 0)
            {
                return; // Skip unsupported characters
            }

            if (c == '+' || c == '_' || c == '{' || c == '}' || c == '|' || c == ':' || c == '"' || c == '<' || c == '>' || c == '?' || c == '~' || (vkCode >= 0x30 && vkCode <= 0x39))
            {
                shiftRequired = true;
            }
        }

        // If using Citrix, simulate the scan code instead of the virtual key code
        byte scanCode = isCitrix ? GetScanCodeForKey(c) : vkCode;

        // If shift is required (for uppercase or symbols)
        if (shiftRequired)
        {
            keybd_event(VK_SHIFT, 0, 0, 0); // Key down for SHIFT
        }

        // Simulate key down with scan code
        keybd_event(vkCode, scanCode, 0, 0);

        // Simulate key up (release)
        keybd_event(vkCode, scanCode, 2, 0);

        // If shift was pressed, release it
        if (shiftRequired)
        {
            keybd_event(VK_SHIFT, 0, 2, 0); // Key up for SHIFT
        }
    }

    // Helper function to return virtual key code for special characters
    static byte GetSpecialCharacterVKCode(char c)
    {
        switch (c)
        {
            case '!': return 0x31; // Shift + 1
            case '@': return 0x32; // Shift + 2
            case '#': return 0x33; // Shift + 3
            case '$': return 0x34; // Shift + 4
            case '%': return 0x35; // Shift + 5
            case '^': return 0x36; // Shift + 6
            case '&': return 0x37; // Shift + 7
            case '*': return 0x38; // Shift + 8
            case '(': return 0x39; // Shift + 9
            case ')': return 0x30; // Shift + 0
            case '-': return 0xBD; // Hyphen
            case '=': return 0xBB; // Equal sign
            case '+': return 0xBB; // Plus sign (Shift + =)
            case '`': return 0xC0; // Backtick
            case '~': return 0xC0; // Tilde (Shift + `)
            case '[': return 0xDB; // Left bracket
            case ']': return 0xDD; // Right bracket
            case '\\': return 0xDC; // Backslash
            case ';': return 0xBA; // Semicolon
            case '\'': return 0xDE; // Single quote
            case ',': return 0xBC; // Comma
            case '.': return 0xBE; // Period
            case '/': return 0xBF; // Slash
            case ':': return 0xBA; // Colon (Shift + ;)
            case '"': return 0xDE; // Double quote (Shift + ')
            case '<': return 0xBC; // Shift + Comma
            case '>': return 0xBE; // Shift + Period
            case '?': return 0xBF; // Question mark
            case '_': return 0xBD; // Shift + Hyphen
            case ' ': return 0x20; // Spacebar
            case '{': return 0xDB; // Left curly brace (Shift + [)
            case '}': return 0xDD; // Right curly brace (Shift + ]))
            case '|': return 0xDC; // Pipe (Shift + \)
            default: return 0; // Default scan code for unsupported keys
        }
    }

    // Helper function to get the scan code for the character
    static byte GetScanCodeForKey(char c)
    {
        switch (c)
        {
            // Handling lowercase and uppercase characters
            case 'a': return 0x1E; // 'a' -> 0x1E
            case 'b': return 0x30; // 'b' -> 0x30
            case 'c': return 0x2E; // 'c' -> 0x2E
            case 'd': return 0x20; // 'd' -> 0x20
            case 'e': return 0x12; // 'e' -> 0x12
            case 'f': return 0x21; // 'f' -> 0x21
            case 'g': return 0x22; // 'g' -> 0x22
            case 'h': return 0x23; // 'h' -> 0x23
            case 'i': return 0x17; // 'i' -> 0x17
            case 'j': return 0x24; // 'j' -> 0x24
            case 'k': return 0x25; // 'k' -> 0x25
            case 'l': return 0x26; // 'l' -> 0x26
            case 'm': return 0x32; // 'm' -> 0x32
            case 'n': return 0x31; // 'n' -> 0x31
            case 'o': return 0x18; // 'o' -> 0x18
            case 'p': return 0x19; // 'p' -> 0x19
            case 'q': return 0x10; // 'q' -> 0x10
            case 'r': return 0x13; // 'r' -> 0x13
            case 's': return 0x1F; // 's' -> 0x1F
            case 't': return 0x14; // 't' -> 0x14
            case 'u': return 0x16; // 'u' -> 0x16
            case 'v': return 0x2F; // 'v' -> 0x2F
            case 'w': return 0x11; // 'w' -> 0x11
            case 'x': return 0x2D; // 'x' -> 0x2D
            case 'y': return 0x15; // 'y' -> 0x15
            case 'z': return 0x2C; // 'z' -> 0x2C

            // Handling uppercase characters
            case 'A': return 0x1E; // 'A' -> 0x1E (shifted)
            case 'B': return 0x30; // 'B' -> 0x30 (shifted)
            case 'C': return 0x2E; // 'C' -> 0x2E (shifted)
            case 'D': return 0x20; // 'D' -> 0x20 (shifted)
            case 'E': return 0x12; // 'E' -> 0x12 (shifted)
            case 'F': return 0x21; // 'F' -> 0x21 (shifted)
            case 'G': return 0x22; // 'G' -> 0x22 (shifted)
            case 'H': return 0x23; // 'H' -> 0x23 (shifted)
            case 'I': return 0x17; // 'I' -> 0x17 (shifted)
            case 'J': return 0x24; // 'J' -> 0x24 (shifted)
            case 'K': return 0x25; // 'K' -> 0x25 (shifted)
            case 'L': return 0x26; // 'L' -> 0x26 (shifted)
            case 'M': return 0x32; // 'M' -> 0x32 (shifted)
            case 'N': return 0x31; // 'N' -> 0x31 (shifted)
            case 'O': return 0x18; // 'O' -> 0x18 (shifted)
            case 'P': return 0x19; // 'P' -> 0x19 (shifted)
            case 'Q': return 0x10; // 'Q' -> 0x10 (shifted)
            case 'R': return 0x13; // 'R' -> 0x13 (shifted)
            case 'S': return 0x1F; // 'S' -> 0x1F (shifted)
            case 'T': return 0x14; // 'T' -> 0x14 (shifted)
            case 'U': return 0x16; // 'U' -> 0x16 (shifted)
            case 'V': return 0x2F; // 'V' -> 0x2F (shifted)
            case 'W': return 0x11; // 'W' -> 0x11 (shifted)
            case 'X': return 0x2D; // 'X' -> 0x2D (shifted)
            case 'Y': return 0x15; // 'Y' -> 0x15 (shifted)
            case 'Z': return 0x2C; // 'Z' -> 0x2C (shifted)

            // Numbers and symbols
            case '0': return 0x0B; // '0' -> 0x0B
            case '1': return 0x02; // '1' -> 0x02
            case '2': return 0x03; // '2' -> 0x03
            case '3': return 0x04; // '3' -> 0x04
            case '4': return 0x05; // '4' -> 0x05
            case '5': return 0x06; // '5' -> 0x06
            case '6': return 0x07; // '6' -> 0x07
            case '7': return 0x08; // '7' -> 0x08
            case '8': return 0x09; // '8' -> 0x09
            case '9': return 0x0A; // '9' -> 0x0A
            case '!': return 0x02; // '!' -> shift + 1
            case '@': return 0x03; // '@' -> shift + 2
            case '#': return 0x04; // '#' -> shift + 3
            case '$': return 0x05; // '$' -> shift + 4
            case '%': return 0x06; // '%' -> shift + 5
            case '^': return 0x07; // '^' -> shift + 6
            case '&': return 0x08; // '&' -> shift + 7
            case '*': return 0x09; // '*' -> shift + 8
            case '(': return 0x0A; // '(' -> shift + 9
            case ')': return 0x0B; // ')' -> shift + 0
            case '-': return 0x0C; // '-' -> normal
            case '=': return 0x0D; // '=' -> normal
            case '+': return 0x0D; // '+' -> shift + equal
            case '`': return 0x0E; // '`' -> normal
            case '~': return 0x0E; // '~' -> shift backtick
            case '[': return 0x1A; // '[' -> normal
            case ']': return 0x1B; // ']' -> normal
            case '\\': return 0x2B; // '\\' -> normal
            case ';': return 0x27; // ';' -> normal
            case '\'': return 0x28; // '\'' -> normal
            case ',': return 0x33; // ',' -> normal
            case '.': return 0x34; // '.' -> normal
            case '/': return 0x35; // '/' -> normal

            // Special characters that were problematic
            case '{': return 0x1A; // Shift + [
            case '}': return 0x1B; // Shift + ]
            case '|': return 0x2B; // Shift + \
            case ':': return 0x27; // Shift + ;
            case '"': return 0x28; // Shift + '
            case '<': return 0x33; // Shift + ,
            case '>': return 0x34; // Shift + .
            case '?': return 0x35; // Shift + /

            // Space and other characters
            case ' ': return 0x39; // Space bar

            default: return 0x00; // If no match, return 0
        }
    }
}
