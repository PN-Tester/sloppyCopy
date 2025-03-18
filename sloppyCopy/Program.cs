using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

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

    static void Main(string[] args)
    {
        // Check if the file path and delay argument are provided
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: sloppyCopy.exe <file> <delay>");
            return;
        }

        // Path to the input file
        string filePath = args[0];

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        // Parse the delay argument
        if (!int.TryParse(args[1], out int delayInSeconds))
        {
            Console.WriteLine("Invalid number for delay.");
            return;
        }

        // Read the content of the file
        string content = File.ReadAllText(filePath);

        // Wait for the specified number of seconds before starting the simulation
        Console.WriteLine($"Waiting for {delayInSeconds} seconds...");
        Thread.Sleep(delayInSeconds * 1000); // Convert seconds to milliseconds

        // Simulate the keyboard keypresses
        foreach (char c in content)
        {
            SimulateKeyPress(c);
            Thread.Sleep(10); // Add a slight delay to simulate human typing speed
        }

        Console.WriteLine("Simulation complete.");
    }

    // Simulate a key press using keybd_event
    static void SimulateKeyPress(char c)
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

            // Set shiftRequired for any special character that needs Shift (like + and _)
            if (c == '+' || c == '_' || (vkCode >= 0x30 && vkCode <= 0x39))
            {
                shiftRequired = true;
            }

            // Handle missing cases explicitly
            switch (c)
            {
                case '~':
                    vkCode = 0xC0;
                    shiftRequired = true;
                    break; // Shift + Backtick
                case ':':
                    vkCode = 0xBA;
                    shiftRequired = true;
                    break; // Shift + Semicolon
                case '"':
                    vkCode = 0xDE;
                    shiftRequired = true;
                    break; // Shift + Single quote
                case '<':
                    vkCode = 0xBC;
                    shiftRequired = true;
                    break; // Shift + Comma
                case '>':
                    vkCode = 0xBE;
                    shiftRequired = true;
                    break; // Shift + Period
                case '?':
                    vkCode = 0xBF;
                    shiftRequired = true;
                    break; // Shift + Slash
                case '{':
                    vkCode = 0xDB;
                    shiftRequired = true;
                    break; // Shift + Left bracket
                case '}':
                    vkCode = 0xDD;
                    shiftRequired = true;
                    break; // Shift + Right bracket
                case '|':
                    vkCode = 0xDC;
                    shiftRequired = true;
                    break; // Shift + Backslash
            }
        }

        // If shift is required (for uppercase or symbols)
        if (shiftRequired)
        {
            keybd_event(VK_SHIFT, 0, 0, 0); // Key down for SHIFT
        }

        // Simulate key down
        keybd_event(vkCode, 0, 0, 0);

        // Simulate key up (release)
        keybd_event(vkCode, 0, 2, 0);

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
            case '}': return 0xDD; // Right curly brace (Shift + ])
            case '|': return 0xDC; // Pipe (Shift + \)
            default: return 0; // If unhandled
        }
    }
}
