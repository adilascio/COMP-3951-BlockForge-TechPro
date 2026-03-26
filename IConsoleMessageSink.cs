namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge IConsoleMessageSink
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    public interface IConsoleMessageSink
    {
        /// <summary>
        /// Appends a message to the console sink.
        /// </summary>
        /// <param name="message">The message to append.</param>
        void Append(ConsoleMessage message);
    }
}
