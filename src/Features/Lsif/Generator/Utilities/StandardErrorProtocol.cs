namespace Microsoft.CodeAnalysis.LanguageServerIndexFormat.Generator.Utilities
{
    using System.Collections.Generic;
    using System;
    using System.Text.Json;

    /// <summary>
    /// Implements methods for a simple, one way, JSON encoded protocol written to <see cref="Console.Error"/>
    /// that enables granular, aggregatable insights to be communicated between the LSIF generator
    /// tool and the invoking process for diagnostics purposes.
    /// </summary>
    internal static class StandardErrorProtocol
    {
        // **************************
        // * Protocol definition
        // **************************
        // - Each line is its own 'command' JSON object.
        //   - 'command' - The name of a command.
        //   - 'parameters' - Parameters to the command agreed upon between the LSIF generator and the invoking application.
        //   - 'properties' - Arbitrary qualitative metrics that we want logged or reported.
        //   - 'measures' - Arbitrary quantitative metrics that we want logged or reported.

        /// <summary>
        /// Logs diagnostics information and measurements to <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="message">An optional message.</param>
        /// <param name="properties">Our own custom key/value pair metrics that may help with diagnosis.</param>
        /// <param name="measures">Our own custom key/value pair metrics that may help with diagnosis.</param>
        public static void LogInfo(
            string? message = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, double>? measures = null)
        {
            var parameters = new Dictionary<string, string>();

            var messageValue = message;
            if (messageValue is not null)
            {
                parameters.Add("message", messageValue);
            }

            Console.Error.WriteLine(
                JsonSerializer.Serialize(
                    new StandardErrorProtocolCommand(CommandNames.LogInformation, parameters, properties, measures)));
        }

        /// <summary>
        /// Logs an error to <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="message">The exception message. If omitted, defaults to <paramref name="ex"/>.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="properties">Our own custom key/value pair metrics that may help with diagnosis.</param>
        /// <param name="measures">Our own custom key/value pair metrics that may help with diagnosis.</param>
        public static void LogError(
            string? message = null,
            Exception? ex = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, double>? measures = null)
        {
            var parameters = new Dictionary<string, string>();

            var messageValue = message ?? ex?.Message;
            if (messageValue is not null)
            {
                parameters.Add("message", messageValue);
            }

            if (ex is not null)
            {
                parameters.Add("exception", ex.GetType().FullName ?? "Unknown Exception Type");
            }

            if (ex?.StackTrace is not null)
            {
                parameters.Add("callstack", ex.StackTrace);
            }

            Console.Error.WriteLine(
                JsonSerializer.Serialize(
                    new StandardErrorProtocolCommand(CommandNames.LogError, parameters, properties, measures)));
        }

        // The standard schema for a command.
        private record StandardErrorProtocolCommand(
            string command,
            Dictionary<string, string>? parameters = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, double>? measures = null);

        // The well-known command names.
        private static class CommandNames
        {
            public const string LogError = "logError";

            public const string LogInformation = "logInfo";
        }
    }
}
