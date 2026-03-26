namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge ConsoleMessage
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    /// <param name="Severity">The severity level of the console message.</param>
    /// <param name="Text">The message text displayed in the console window.</param>
    public readonly record struct ConsoleMessage(ConsoleMessageSeverity Severity, string Text);
}
