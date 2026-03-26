using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.Console;

/// <summary>
/// BlockForge WorkspaceConsoleTests
/// Author: Andre Di Lascio
/// Date: Mar 24 2026
/// Source: Written with the help of AI.
/// </summary>
[TestClass]
public sealed class WorkspaceConsoleTests
{
    [TestMethod]
    public void ReportSaveSuccess_AppendsSuccessfullySavedWorkspaceMessage()
    {
        var console = new WorkspaceConsole();
        var notifier = new WorkspaceSaveNotifier(console);

        notifier.ReportSaveSuccess();

        Assert.AreEqual(1, console.Messages.Count);
        Assert.AreEqual(ConsoleMessageSeverity.Message, console.Messages[0].Severity);
        Assert.AreEqual(WorkspaceSaveNotifier.SaveSuccessMessage, console.Messages[0].Text);
    }

    [TestMethod]
    public void Console_StoresMessageAndSeverityCorrectly()
    {
        var console = new WorkspaceConsole();

        console.Append(new ConsoleMessage(ConsoleMessageSeverity.Warning, "Low disk space"));

        Assert.AreEqual(1, console.Messages.Count);
        Assert.AreEqual(ConsoleMessageSeverity.Warning, console.Messages[0].Severity);
        Assert.AreEqual("Low disk space", console.Messages[0].Text);
    }

    [TestMethod]
    public void ReportSaveSuccess_RepeatedSaves_AppendConsistently()
    {
        var console = new WorkspaceConsole();
        var notifier = new WorkspaceSaveNotifier(console);

        notifier.ReportSaveSuccess();
        notifier.ReportSaveSuccess();
        notifier.ReportSaveSuccess();

        Assert.AreEqual(3, console.Messages.Count);
        CollectionAssert.AreEqual(
            new[]
            {
                WorkspaceSaveNotifier.SaveSuccessMessage,
                WorkspaceSaveNotifier.SaveSuccessMessage,
                WorkspaceSaveNotifier.SaveSuccessMessage
            },
            console.Messages.Select(message => message.Text).ToArray());
    }

    [TestMethod]
    public void ReportSaveSuccess_EmptyWorkspaceScenario_StillProducesValidMessage()
    {
        var console = new WorkspaceConsole();
        var notifier = new WorkspaceSaveNotifier(console);

        notifier.ReportSaveSuccess();

        Assert.AreEqual(WorkspaceSaveNotifier.SaveSuccessMessage, console.Messages.Single().Text);
    }

    [TestMethod]
    public void Append_WhitespaceMessage_ThrowsArgumentException()
    {
        var console = new WorkspaceConsole();

        Assert.Throws<ArgumentException>(() =>
            console.Append(new ConsoleMessage(ConsoleMessageSeverity.Error, " ")));
    }
}
