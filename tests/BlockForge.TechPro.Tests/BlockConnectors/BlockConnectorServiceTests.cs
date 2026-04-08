using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.BlockConnectors;

[TestClass]
public sealed class BlockConnectorServiceTests
{
    private readonly BlockConnectorService _service = new();
    private readonly StatementChainService _statementService = new();

    [TestMethod]
    public void Connect_CompatibleBlocks_AttachSuccessfullyAtValidConnectorPoints()
    {
        CodeBlock parent = new(280, 216, "run-1", 2, 3, CodeBlockType.Run, "Run");
        CodeBlock child = new(0, 0, "print-1", 0, 0, CodeBlockType.Print, "Print");

        _service.Connect(parent, child);

        Assert.AreEqual(child.Uid, parent.ChildBlockUid);
        Assert.AreEqual(parent.Uid, child.ParentBlockUid);
        Assert.AreEqual(parent.GridColumn, child.GridColumn);
        Assert.AreEqual(parent.GridRow + 1, child.GridRow);
        Assert.AreEqual(parent.PosX, child.PosX);
        Assert.AreEqual(parent.PosY + 72, child.PosY);
    }

    [TestMethod]
    public void Connect_IncompatibleBlockTypes_IsRejectedAndBlocksRemainSeparate()
    {
        CodeBlock parent = new(80, 120, "equals-1", 2, 3, CodeBlockType.Equals, "==");
        CodeBlock child = new(0, 0, "print-1", 0, 0, CodeBlockType.Print, "Print");

        Assert.Throws<InvalidOperationException>(() => _service.Connect(parent, child));
        Assert.IsNull(parent.ChildBlockUid);
        Assert.IsNull(child.ParentBlockUid);
    }

    [TestMethod]
    public void Disconnect_ConnectedBlocks_SeparatesCleanlyWithoutAffectingUnrelatedBlocks()
    {
        CodeBlock parent = new(280, 216, "run-1", 2, 3, CodeBlockType.Run, "Run");
        CodeBlock child = new(80, 160, "print-1", 2, 4, CodeBlockType.Print, "Print");
        CodeBlock unrelated = new(200, 120, "while-1", 5, 3, CodeBlockType.While, "While");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [parent.Uid] = parent,
            [child.Uid] = child,
            [unrelated.Uid] = unrelated
        };
        _service.Connect(parent, child);

        _service.Disconnect(child, blocks);

        Assert.IsNull(parent.ChildBlockUid);
        Assert.IsNull(child.ParentBlockUid);
        Assert.IsNull(child.ChildBlockUid);
        Assert.IsNull(unrelated.ParentBlockUid);
        Assert.IsNull(unrelated.ChildBlockUid);
    }

    [TestMethod]
    public void MoveConnectedBlockChain_EntireStructureMovesTogetherAndKeepsAlignment()
    {
        CodeBlock root = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock middle = new(0, 0, "print-1", 0, 0, CodeBlockType.Print, "Print");
        CodeBlock child = new(0, 0, "if-1", 0, 0, CodeBlockType.If, "If");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [root.Uid] = root,
            [middle.Uid] = middle,
            [child.Uid] = child
        };
        _service.Connect(root, middle);
        _service.Connect(middle, child);

        _service.MoveChain(root, blocks, 4, 2);

        Assert.AreEqual((4, 2), (root.GridColumn, root.GridRow));
        Assert.AreEqual((4, 3), (middle.GridColumn, middle.GridRow));
        Assert.AreEqual((4, 4), (child.GridColumn, child.GridRow));
        Assert.AreEqual(560d, root.PosX);
        Assert.AreEqual(144d, root.PosY);
        Assert.AreEqual(216d, middle.PosY);
        Assert.AreEqual(288d, child.PosY);
    }

    [TestMethod]
    public void DeleteBlock_WithAttachedConnector_HandlesChildBlocksSafelyAccordingToRules()
    {
        CodeBlock parent = new(280, 216, "run-1", 2, 3, CodeBlockType.Run, "Run");
        CodeBlock child = new(80, 160, "print-1", 2, 4, CodeBlockType.Print, "Print");
        CodeBlock unrelated = new(200, 120, "while-1", 5, 3, CodeBlockType.While, "While");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [parent.Uid] = parent,
            [child.Uid] = child,
            [unrelated.Uid] = unrelated
        };
        _service.Connect(parent, child);

        _service.DeleteBlock(parent, blocks);

        Assert.IsFalse(blocks.ContainsKey(parent.Uid));
        Assert.IsTrue(blocks.ContainsKey(child.Uid));
        Assert.IsNull(child.ParentBlockUid);
        Assert.IsTrue(blocks.ContainsKey(unrelated.Uid));
    }

    [TestMethod]
    public void ReconnectBlock_AfterDisconnect_CanBeAttachedAgainToValidConnector()
    {
        CodeBlock parent = new(280, 216, "run-1", 2, 3, CodeBlockType.Run, "Run");
        CodeBlock child = new(80, 160, "print-1", 2, 4, CodeBlockType.Print, "Print");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [parent.Uid] = parent,
            [child.Uid] = child
        };
        _service.Connect(parent, child);
        _service.Disconnect(child, blocks);

        _service.Connect(parent, child);

        Assert.AreEqual(child.Uid, parent.ChildBlockUid);
        Assert.AreEqual(parent.Uid, child.ParentBlockUid);
    }

    [TestMethod]
    public void ConnectorAlignmentAfterSnap_ConnectedBlocksAlignWithNoGapInConnectorRows()
    {
        CodeBlock parent = new(420, 360, "while-1", 3, 5, CodeBlockType.While, "While");
        CodeBlock child = new(0, 0, "var-1", 0, 0, CodeBlockType.Variable, "score", VariableBlockType.Int, intValue: 1);

        _service.Connect(parent, child);

        Assert.AreEqual(parent.GridColumn, child.GridColumn);
        Assert.AreEqual(parent.GridRow + 1, child.GridRow);
        Assert.AreEqual(parent.PosX, child.PosX);
        Assert.AreEqual(parent.PosY + 72, child.PosY);
    }

    [TestMethod]
    public void ExportWorkspaceWithConnectedBlocks_PreservesConnectorHierarchyOrderInGeneratedSource()
    {
        CodeBlock root = new(280, 216, "run-1", 2, 3, CodeBlockType.Run, "Run");
        CodeBlock child = new(0, 0, "print-1", 0, 0, CodeBlockType.Print, "Print");
        _service.Connect(root, child);
        Project project = new("connected_project", new List<CodeBlock> { child, root });

        string json = ProjectSerializer.Serialize(project);
        Project restored = ProjectSerializer.Deserialize(json);
        Dictionary<string, CodeBlock> restoredBlocks = restored.CodeBlocks.ToDictionary(block => block.Uid, block => block);
        CodeBlock restoredRoot = restoredBlocks[root.Uid];
        List<CodeBlock> ordered = _service.GetChainFrom(restoredRoot, restoredBlocks);
        string generatedSource = string.Join("\n", ordered.Select(JavaBlockMap.ToJava));

        Assert.AreEqual(child.Uid, restoredRoot.ChildBlockUid);
        Assert.AreEqual(root.Uid, restoredBlocks[child.Uid].ParentBlockUid);
        Assert.AreEqual("run();\nSystem.out.println(value);", generatedSource);
    }

    [TestMethod]
    public void GetStatementRoot_ReturnsLeftmostBlockInHorizontalStatementChain()
    {
        CodeBlock left = new(0, 0, "var-1", 0, 0, CodeBlockType.Variable, "score", VariableBlockType.Int);
        CodeBlock middle = new(140, 0, "assign-1", 1, 0, CodeBlockType.Assignment, "=");
        CodeBlock right = new(280, 0, "op-1", 2, 0, CodeBlockType.Operator, "Operator");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [left.Uid] = left,
            [middle.Uid] = middle,
            [right.Uid] = right
        };

        _service.ConnectStatement(left, middle);
        _service.ConnectStatement(middle, right);

        CodeBlock root = _statementService.GetStatementRoot(right, blocks);

        Assert.AreEqual(left.Uid, root.Uid);
    }

    [TestMethod]
    public void GetStatementChainFrom_ReturnsOrderedHorizontalBlocksStartingAtLeftmostRoot()
    {
        CodeBlock left = new(0, 0, "var-1", 0, 0, CodeBlockType.Variable, "score", VariableBlockType.Int);
        CodeBlock middle = new(140, 0, "assign-1", 1, 0, CodeBlockType.Assignment, "=");
        CodeBlock right = new(280, 0, "op-1", 2, 0, CodeBlockType.Operator, "Operator");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [left.Uid] = left,
            [middle.Uid] = middle,
            [right.Uid] = right
        };

        _service.ConnectStatement(left, middle);
        _service.ConnectStatement(middle, right);

        List<CodeBlock> statementChain = _statementService.GetStatementChain(middle, blocks);

        CollectionAssert.AreEqual(new[] { left.Uid, middle.Uid, right.Uid }, statementChain.Select(block => block.Uid).ToArray());
    }

    [TestMethod]
    public void GetExecutionOrderedStatementRoots_ReturnsLeftmostRowsInVerticalExecutionOrder()
    {
        CodeBlock run = new(0, 0, "run-1", 0, 0, CodeBlockType.Run, "Run");
        CodeBlock print = new(0, 72, "print-1", 0, 1, CodeBlockType.Print, "Print");
        CodeBlock variable = new(140, 72, "var-1", 1, 1, CodeBlockType.Variable, "score", VariableBlockType.Int);
        CodeBlock whileBlock = new(0, 144, "while-1", 0, 2, CodeBlockType.While, "While");
        Dictionary<string, CodeBlock> blocks = new()
        {
            [run.Uid] = run,
            [print.Uid] = print,
            [variable.Uid] = variable,
            [whileBlock.Uid] = whileBlock
        };

        _service.Connect(run, print);
        _service.Connect(print, whileBlock);
        _service.ConnectStatement(print, variable);

        List<CodeBlock> roots = _statementService.GetExecutionOrderedStatementRoots(blocks);

        CollectionAssert.AreEqual(new[] { run.Uid, print.Uid, whileBlock.Uid }, roots.Select(block => block.Uid).ToArray());
    }

    [TestMethod]
    public void BuildStatementText_ReturnsJoinedTokensForAssignmentStatement()
    {
        CodeBlock variable = new(0, 0, "var-1", 0, 1, CodeBlockType.Variable, "score", VariableBlockType.Int);
        CodeBlock assignment = new(140, 0, "assign-1", 1, 1, CodeBlockType.Assignment, "=");
        CodeBlock rightVariable = new(280, 0, "var-2", 2, 1, CodeBlockType.Variable, "count", VariableBlockType.Int);
        CodeBlock op = new(420, 0, "op-1", 3, 1, CodeBlockType.Operator, "Operator", stringValue: "+");
        CodeBlock rightValue = new(560, 0, "var-3", 4, 1, CodeBlockType.Variable, "step", VariableBlockType.Int);
        Dictionary<string, CodeBlock> blocks = new()
        {
            [variable.Uid] = variable,
            [assignment.Uid] = assignment,
            [rightVariable.Uid] = rightVariable,
            [op.Uid] = op,
            [rightValue.Uid] = rightValue
        };

        _service.ConnectStatement(variable, assignment);
        _service.ConnectStatement(assignment, rightVariable);
        _service.ConnectStatement(rightVariable, op);
        _service.ConnectStatement(op, rightValue);

        string statementText = _statementService.BuildStatementText(op, blocks);

        Assert.AreEqual("score = count + step", statementText);
    }

    [TestMethod]
    public void BuildStatementText_ReturnsPrintFollowedByExpressionTokens()
    {
        CodeBlock print = new(0, 0, "print-1", 0, 1, CodeBlockType.Print, "Print");
        CodeBlock variable = new(140, 0, "var-1", 1, 1, CodeBlockType.Variable, "score", VariableBlockType.Int);
        CodeBlock op = new(280, 0, "op-1", 2, 1, CodeBlockType.Operator, "Operator", stringValue: "%");
        CodeBlock divisor = new(420, 0, "var-2", 3, 1, CodeBlockType.Variable, "two", VariableBlockType.Int);
        Dictionary<string, CodeBlock> blocks = new()
        {
            [print.Uid] = print,
            [variable.Uid] = variable,
            [op.Uid] = op,
            [divisor.Uid] = divisor
        };

        _service.ConnectStatement(print, variable);
        _service.ConnectStatement(variable, op);
        _service.ConnectStatement(op, divisor);

        string statementText = _statementService.BuildStatementText(variable, blocks);

        Assert.AreEqual("Print score % two", statementText);
    }
}


