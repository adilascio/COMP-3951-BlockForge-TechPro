using System;
using System.Collections.Generic;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge WorkspaceConsole
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    public class WorkspaceConsole : IConsoleMessageSink
    {
        private readonly List<ConsoleMessage> _messages = new();

        /// <summary>
        /// Gets the messages currently stored in the workspace console.
        /// </summary>
        public IReadOnlyList<ConsoleMessage> Messages => _messages;

        /// <summary>
        /// Appends a new message to the workspace console.
        /// </summary>
        /// <param name="message">The message to append.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the message text is null, empty, or whitespace.
        /// </exception>
        public void Append(ConsoleMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                throw new ArgumentException("Console messages must contain text.", nameof(message));
            }

            _messages.Add(message);
        }
    }
}
