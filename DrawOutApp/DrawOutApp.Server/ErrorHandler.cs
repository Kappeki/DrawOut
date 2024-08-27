using System.Text;

namespace DrawOutApp.Server
{
    internal static class ErrorHandler
    {
        internal static string HandleError(Exception e)
        {
            StringBuilder sb = new();

            sb.AppendLine($"({e.GetType().Name}):");
            sb.Append($"{e.Message}");
            int indent = 4;

            Exception? exception = e.InnerException;

            while (exception != null)
            {
                sb.AppendLine($"{new string(' ', indent)}-> ({e.GetType().Name}):");
                sb.Append($"{exception.Message}");
                indent += 4;
                exception = exception.InnerException;
            }

            string errorText = sb.ToString();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(errorText);
            Console.ResetColor();
            return errorText;
        }
    }
}
