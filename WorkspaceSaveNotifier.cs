using System;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge WorkspaceSaveNotifier
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    public class WorkspaceSaveNotifier
    {
        /// <summary>
        /// The standard message shown after a successful save.
        /// </summary>
        public const string SaveSuccessMessage = "successfully saved workspace";

        private readonly IConsoleMessageSink _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceSaveNotifier"/> class.
        /// </summary>
        /// <param name="console">The console sink that receives save messages.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="console"/> is null.
        /// </exception>
        public WorkspaceSaveNotifier(IConsoleMessageSink console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        /// <summary>
        /// Sends the default successful save message to the console sink.
        /// </summary>
        public void ReportSaveSuccess()
        {
            _console.Append(new ConsoleMessage(ConsoleMessageSeverity.Message, SaveSuccessMessage));
        }
    }
}
