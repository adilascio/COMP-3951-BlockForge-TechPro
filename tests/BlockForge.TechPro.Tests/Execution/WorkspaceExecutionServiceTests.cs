using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.Execution;

[TestClass]
public sealed class WorkspaceExecutionServiceTests
{
    private readonly WorkspaceExecutionService _service = new();
    private readonly BlockConnectorService _connectorService = new();

    [TestMethod]
    public void Execute_RunAssignmentThenPrint_OutputsUpdatedIntegerValue()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock score = new(0, 72, "score-1", 0, 1, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 0);
        CodeBlock assignment = new(140, 72, "assign-1", 1, 1, CodeBlockType.Assignment, "=");
        CodeBlock count = new(280, 72, "count-1", 2, 1, CodeBlockType.Variable, "count", VariableBlockType.Int, intValue: 2);
        CodeBlock op = new(420, 72, "op-1", 3, 1, CodeBlockType.Operator, "Operator", stringValue: "+");
        CodeBlock step = new(560, 72, "step-1", 4, 1, CodeBlockType.Variable, "step", VariableBlockType.Int, intValue: 1);
        CodeBlock print = new(0, 144, "print-1", 0, 2, CodeBlockType.Print, "Print");
        CodeBlock printScore = new(140, 144, "score-2", 1, 2, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 0);

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, score, assignment, count, op, step, print, printScore);

        _connectorService.Connect(run, score);
        _connectorService.Connect(score, print);
        _connectorService.ConnectStatement(score, assignment);
        _connectorService.ConnectStatement(assignment, count);
        _connectorService.ConnectStatement(count, op);
        _connectorService.ConnectStatement(op, step);
        _connectorService.ConnectStatement(print, printScore);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        CollectionAssert.AreEqual(new[] { "3" }, result.OutputLines);
        Assert.AreEqual(3, result.VariableState["score"]);
        Assert.AreEqual(0, result.Diagnostics.Count);
        Assert.AreEqual(3, blocks["score-2"].IntValue);
    }

    [TestMethod]
    public void Execute_PrintStringVariable_OutputsStoredStringValue()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock print = new(0, 72, "print-1", 0, 1, CodeBlockType.Print, "Print");
        CodeBlock message = new(140, 72, "message-1", 1, 1, CodeBlockType.Variable, "message", VariableBlockType.String, stringValue: "hello");

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, print, message);

        _connectorService.Connect(run, print);
        _connectorService.ConnectStatement(print, message);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        CollectionAssert.AreEqual(new[] { "hello" }, result.OutputLines);
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public void Execute_WithoutRunBlock_ReturnsDiagnosticInsteadOfThrowing()
    {
        CodeBlock print = new(0, 72, "print-1", 0, 1, CodeBlockType.Print, "Print");
        CodeBlock message = new(140, 72, "message-1", 1, 1, CodeBlockType.Variable, "message", VariableBlockType.String, stringValue: "hello");

        Dictionary<string, CodeBlock> blocks = CreateBlocks(print, message);
        _connectorService.ConnectStatement(print, message);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        Assert.AreEqual(0, result.OutputLines.Count);
        Assert.AreEqual("Execution aborted: no Run block found.", result.Diagnostics.Single());
    }

    [TestMethod]
    public void Execute_PrintInputBlock_OutputsTypedLiteralValue()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock print = new(0, 72, "print-1", 0, 1, CodeBlockType.Print, "Print");
        CodeBlock input = new(140, 72, "input-1", 1, 1, CodeBlockType.Input, "Input", VariableBlockType.Int, stringValue: "42", intValue: 42);

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, print, input);

        _connectorService.Connect(run, print);
        _connectorService.ConnectStatement(print, input);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        CollectionAssert.AreEqual(new[] { "42" }, result.OutputLines);
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public void Execute_VariableComparedToTypedInput_DoesNotProduceUnsupportedStatementWarning()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock score = new(0, 72, "score-1", 0, 1, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 42);
        CodeBlock equals = new(140, 72, "equals-1", 1, 1, CodeBlockType.Equals, "==");
        CodeBlock input = new(280, 72, "input-1", 2, 1, CodeBlockType.Input, "Input", VariableBlockType.Int, stringValue: "42", intValue: 42);

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, score, equals, input);

        _connectorService.Connect(run, score);
        _connectorService.ConnectStatement(score, equals);
        _connectorService.ConnectStatement(equals, input);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        Assert.AreEqual(0, result.OutputLines.Count);
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public void Execute_VariableComparedWithLessThanOrEqualInput_DoesNotProduceDiagnostics()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock score = new(0, 72, "score-1", 0, 1, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 42);
        CodeBlock lessThan = new(140, 72, "lt-1", 1, 1, CodeBlockType.LessThan, "<=", stringValue: "<=");
        CodeBlock input = new(280, 72, "input-1", 2, 1, CodeBlockType.Input, "Input", VariableBlockType.Int, stringValue: "42", intValue: 42);

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, score, lessThan, input);

        _connectorService.Connect(run, score);
        _connectorService.ConnectStatement(score, lessThan);
        _connectorService.ConnectStatement(lessThan, input);

        WorkspaceExecutionResult result = _service.Execute(blocks);

        Assert.AreEqual(0, result.OutputLines.Count);
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public void Execute_DivideByZeroExpression_ThrowsHelpfulError()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock score = new(0, 72, "score-1", 0, 1, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 4);
        CodeBlock assignment = new(140, 72, "assign-1", 1, 1, CodeBlockType.Assignment, "=");
        CodeBlock count = new(280, 72, "count-1", 2, 1, CodeBlockType.Variable, "count", VariableBlockType.Int, intValue: 4);
        CodeBlock op = new(420, 72, "op-1", 3, 1, CodeBlockType.Operator, "Operator", stringValue: "/");
        CodeBlock zero = new(560, 72, "zero-1", 4, 1, CodeBlockType.Variable, "zero", VariableBlockType.Int, intValue: 0);

        Dictionary<string, CodeBlock> blocks = CreateBlocks(run, score, assignment, count, op, zero);

        _connectorService.Connect(run, score);
        _connectorService.ConnectStatement(score, assignment);
        _connectorService.ConnectStatement(assignment, count);
        _connectorService.ConnectStatement(count, op);
        _connectorService.ConnectStatement(op, zero);

        DivideByZeroException ex = Assert.Throws<DivideByZeroException>(() => _service.Execute(blocks));

        Assert.AreEqual("Cannot divide by zero.", ex.Message);
    }

    private static Dictionary<string, CodeBlock> CreateBlocks(params CodeBlock[] blocks)
    {
        return blocks.ToDictionary(block => block.Uid, block => block);
    }
}
