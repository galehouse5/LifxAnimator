using System;

namespace LifxAnimator
{
    public static class ConsoleHelper
    {
        // Use in conjunction with `Console.SetCursorPosition` to overwrite lines that were already written
        // so we don't need to use `Console.Clear`, which has display flicker issues.
        public static void OverwriteLine(string value = "")
        {
            // A more efficient version of `Math.Ceiling(value.Length / (decimal)Console.BufferWidth)`.
            int lineCount = (value.Length + Console.BufferWidth - 1) / Console.BufferWidth;
            Console.Write(value.PadRight(Math.Max(lineCount, 1) * Console.BufferWidth));
        }
    }
}
