using System.Text.RegularExpressions;

namespace StudentManagementSystem.Utils;

/// <summary>Small helpers for reading validated input from the console.</summary>
public static class ConsoleHelper
{
    private static readonly Regex EmailPattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
                return input;

            Console.WriteLine("  This field cannot be empty.");
        }
    }

    public static string ReadEmail(string prompt)
    {
        while (true)
        {
            var email = ReadRequired(prompt);
            if (EmailPattern.IsMatch(email))
                return email;

            Console.WriteLine("  Please enter a valid email address.");
        }
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"  Please enter a whole number between {min} and {max}.");
        }
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (y/n): ");
        return Console.ReadLine()?.Trim().ToLowerInvariant() is "y" or "yes";
    }

    public static void Header(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press Enter to continue...");
        Console.ReadLine();
    }
}
