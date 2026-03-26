using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.Quality;

[TestClass]
[DoNotParallelize]
public sealed class WorkspaceFeatureCoverageTests
{
    [TestMethod]
    [TestCategory("Properties Testing")]
    public void CodeBlock_MetadataProperties_AreStored()
    {
        CodeBlock block = new(
            10,
            20,
            "uid-1",
            gridColumn: 3,
            gridRow: 4,
            blockType: CodeBlockType.Variable,
            blockName: "score",
            variableType: VariableBlockType.Int);

        Assert.AreEqual(CodeBlockType.Variable, block.BlockType);
        Assert.AreEqual("score", block.BlockName);
        Assert.AreEqual(VariableBlockType.Int, block.VariableType);
        Assert.AreEqual(3, block.GridColumn);
        Assert.AreEqual(4, block.GridRow);
    }

    [TestMethod]
    [TestCategory("Properties Testing")]
    public void CodeBlock_UpdateBlockMetadata_OverwritesValues()
    {
        CodeBlock block = new(0, 0, "uid-2");

        block.UpdateBlockMetadata(CodeBlockType.Print, "Print", null);

        Assert.AreEqual(CodeBlockType.Print, block.BlockType);
        Assert.AreEqual("Print", block.BlockName);
        Assert.IsNull(block.VariableType);
    }

    [TestMethod]
    [TestCategory("Properties Testing")]
    public void CodeBlock_UpdateBlockMetadata_CanStoreVariableMetadata()
    {
        CodeBlock block = new(0, 0, "uid-3");

        block.UpdateBlockMetadata(CodeBlockType.Variable, "count", VariableBlockType.Int);

        Assert.AreEqual(CodeBlockType.Variable, block.BlockType);
        Assert.AreEqual("count", block.BlockName);
        Assert.AreEqual(VariableBlockType.Int, block.VariableType);
    }

    [TestMethod]
    [TestCategory("Scenario-Based Testing")]
    public void VariableScenario_ModelToJava_ProducesTypedDeclaration()
    {
        CodeBlock block = new(
            0,
            0,
            "uid-variable",
            blockType: CodeBlockType.Variable,
            blockName: "enabled",
            variableType: VariableBlockType.Bool);

        string java = JavaBlockMap.ToJava(block);

        Assert.AreEqual("boolean enabled = false;", java);
    }

    [TestMethod]
    [TestCategory("Functional Testing")]
    public void JavaMapping_StandardBlocks_ReturnExpectedSnippets()
    {
        CodeBlock run = new(0, 0, "uid-run", blockType: CodeBlockType.Run, blockName: "Run");
        CodeBlock equals = new(0, 0, "uid-eq", blockType: CodeBlockType.Equals, blockName: "==");

        Assert.AreEqual("run();", JavaBlockMap.ToJava(run));
        Assert.AreEqual("==", JavaBlockMap.ToJava(equals));
    }

    [TestMethod]
    [TestCategory("Functional Testing")]
    public void JavaMapping_StringVariable_ReturnsStringDeclaration()
    {
        CodeBlock variable = new(
            0,
            0,
            "uid-var-string",
            blockType: CodeBlockType.Variable,
            blockName: "name",
            variableType: VariableBlockType.String);

        Assert.AreEqual("String name = \"\";", JavaBlockMap.ToJava(variable));
    }

    [TestMethod]
    [TestCategory("Functional Testing")]
    public void JavaMapping_PrintBlock_ReturnsConsoleWriteLine()
    {
        CodeBlock print = new(0, 0, "uid-print", blockType: CodeBlockType.Print, blockName: "Print");

        Assert.AreEqual("System.out.println(value);", JavaBlockMap.ToJava(print));
    }

    [TestMethod]
    [TestCategory("Functional Testing")]
    public void JavaMapping_IntVariable_ReturnsIntDeclaration()
    {
        CodeBlock variable = new(
            0,
            0,
            "uid-var-int",
            blockType: CodeBlockType.Variable,
            blockName: "count",
            variableType: VariableBlockType.Int);

        Assert.AreEqual("int count = 0;", JavaBlockMap.ToJava(variable));
    }

    [TestMethod]
    [TestCategory("Functional Testing")]
    public void JavaMapping_BoolVariable_ReturnsBooleanDeclaration()
    {
        CodeBlock variable = new(
            0,
            0,
            "uid-var-bool",
            blockType: CodeBlockType.Variable,
            blockName: "enabled",
            variableType: VariableBlockType.Bool);

        Assert.AreEqual("boolean enabled = false;", JavaBlockMap.ToJava(variable));
    }

    [TestMethod]
    [TestCategory("Edge Case Testing")]
    public void JavaMapping_VariableWithoutType_Throws()
    {
        CodeBlock block = new(
            0,
            0,
            "uid-variable-edge",
            blockType: CodeBlockType.Variable,
            blockName: "value",
            variableType: null);

        Assert.Throws<InvalidOperationException>(() => JavaBlockMap.ToJava(block));
    }

    [TestMethod]
    [TestCategory("Edge Case Testing")]
    public void JavaMapping_UnknownBlockType_Throws()
    {
        CodeBlock block = new(0, 0, "uid-unknown", blockType: CodeBlockType.Unknown, blockName: "Unknown");

        Assert.Throws<InvalidOperationException>(() => JavaBlockMap.ToJava(block));
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DragVisualColor_IsLighterThanOriginal()
    {
        Color original = Color.FromArgb(100, 50, 20);
        Color dragColor = (Color)InvokePrivateStatic("GetDragVisualColor", [original])!;

        Assert.IsTrue(dragColor.R > original.R);
        Assert.IsTrue(dragColor.G > original.G);
        Assert.IsTrue(dragColor.B > original.B);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DragVisualColor_UsesHalfBlendToWhite()
    {
        Color original = Color.FromArgb(80, 120, 160);
        Color dragColor = (Color)InvokePrivateStatic("GetDragVisualColor", [original])!;

        Assert.AreEqual((80 + 255) / 2, dragColor.R);
        Assert.AreEqual((120 + 255) / 2, dragColor.G);
        Assert.AreEqual((160 + 255) / 2, dragColor.B);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteZoneImage_HasTransparentBackground()
    {
        Bitmap image = (Bitmap)InvokePrivateStatic("CreateDeleteZoneImage", null)!;
        Color cornerPixel = image.GetPixel(0, 0);

        Assert.AreEqual(0, cornerPixel.A);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteZoneImage_HasVisibleIconPixels()
    {
        Bitmap image = (Bitmap)InvokePrivateStatic("CreateDeleteZoneImage", null)!;
        int centerX = image.Width / 2;
        Color iconPixel = image.GetPixel(centerX, 10);

        Assert.IsTrue(iconPixel.A > 0);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteZoneRegion_ExcludesOuterCorner()
    {
        Region region = (Region)InvokePrivateStatic("CreateDeleteZoneRegion", null)!;

        Assert.IsFalse(region.IsVisible(0, 0));
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteZoneRegion_HasNonEmptyBounds()
    {
        Region region = (Region)InvokePrivateStatic("CreateDeleteZoneRegion", null)!;
        using Bitmap bitmap = new(28, 28);
        using Graphics graphics = Graphics.FromImage(bitmap);
        RectangleF bounds = region.GetBounds(graphics);

        Assert.IsTrue(bounds.Width > 0);
        Assert.IsTrue(bounds.Height > 0);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void BlockClick_DoesNotLog_WhenViewToggleIsOff()
    {
        using Form1 form = new();
        ToolStripMenuItem toggle = GetPrivateField<ToolStripMenuItem>(form, "toggleBlockTypeLoggingToolStripMenuItem");
        ListBox console = GetPrivateField<ListBox>(form, "_consoleListBox");
        Panel block = new() { Tag = "Print" };

        int before = console.Items.Count;
        toggle.Checked = false;

        InvokePrivateInstance(form, "Block_Click", [block, EventArgs.Empty]);

        Assert.AreEqual(before, console.Items.Count);
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void BlockClick_LogsVariableType_WhenViewToggleIsOn()
    {
        using Form1 form = new();
        ToolStripMenuItem toggle = GetPrivateField<ToolStripMenuItem>(form, "toggleBlockTypeLoggingToolStripMenuItem");
        ListBox console = GetPrivateField<ListBox>(form, "_consoleListBox");
        Panel block = new() { Tag = VariableBlock.CreateString("name") };

        toggle.Checked = true;
        InvokePrivateInstance(form, "Block_Click", [block, EventArgs.Empty]);

        string lastMessage = console.Items[^1]!.ToString()!;
        StringAssert.Contains(lastMessage, "Variable (String)");
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteDrop_RemovesWorkspaceBlock_WhenOverlappingTrash()
    {
        using Form1 form = new();
        PictureBox deleteZone = GetPrivateField<PictureBox>(form, "_deleteDropZone");
        GroupBox workspace = GetPrivateField<GroupBox>(form, "groupBoxWorkSpace");
        Dictionary<Panel, CodeBlock> workspaceBlocks = GetPrivateField<Dictionary<Panel, CodeBlock>>(form, "_workspaceBlocks");
        Panel block = new()
        {
            Size = new Size(20, 20),
            Location = deleteZone.Location,
            Tag = "Print"
        };

        workspace.Controls.Add(block);
        workspaceBlocks[block] = new CodeBlock(0, 0, "uid-delete", blockType: CodeBlockType.Print, blockName: "Print");

        bool deleted = (bool)InvokePrivateInstance(form, "TryDeleteBlockOnDrop", [block])!;

        Assert.IsTrue(deleted);
        Assert.IsFalse(workspace.Controls.Contains(block));
        Assert.IsFalse(workspaceBlocks.ContainsKey(block));
    }

    [TestMethod]
    [TestCategory("User Interaction Testing")]
    public void DeleteDrop_DoesNotRemoveWorkspaceBlock_WhenNotOverlappingTrash()
    {
        using Form1 form = new();
        GroupBox workspace = GetPrivateField<GroupBox>(form, "groupBoxWorkSpace");
        Dictionary<Panel, CodeBlock> workspaceBlocks = GetPrivateField<Dictionary<Panel, CodeBlock>>(form, "_workspaceBlocks");
        Panel block = new()
        {
            Size = new Size(20, 20),
            Location = new Point(5, 5),
            Tag = "Print"
        };

        workspace.Controls.Add(block);
        workspaceBlocks[block] = new CodeBlock(0, 0, "uid-keep", blockType: CodeBlockType.Print, blockName: "Print");

        bool deleted = (bool)InvokePrivateInstance(form, "TryDeleteBlockOnDrop", [block])!;

        Assert.IsFalse(deleted);
        Assert.IsTrue(workspace.Controls.Contains(block));
        Assert.IsTrue(workspaceBlocks.ContainsKey(block));
    }

    [TestMethod]
    [TestCategory("System Events Testing")]
    public void ApplySplitLayout_ClampsSplitterDistanceWithinBounds()
    {
        SplitContainer split = new()
        {
            Orientation = Orientation.Vertical,
            Width = 420,
            Height = 260
        };

        InvokePrivateStatic("ApplySplitLayout", [split, 120, 240, 999]);

        Assert.IsTrue(split.SplitterDistance >= split.Panel1MinSize);
        Assert.IsTrue(split.SplitterDistance <= split.Width - split.Panel2MinSize);
    }

    [TestMethod]
    [TestCategory("System Events Testing")]
    public void ApplySplitLayout_HorizontalClampsToMinimum()
    {
        SplitContainer split = new()
        {
            Orientation = Orientation.Horizontal,
            Width = 320,
            Height = 220
        };

        InvokePrivateStatic("ApplySplitLayout", [split, 120, 80, -50]);

        Assert.AreEqual(split.Panel1MinSize, split.SplitterDistance);
    }

    [TestMethod]
    [TestCategory("System Events Testing")]
    public void LayoutSplitContainerBelowMenu_PositionsSplitBelowMenuBar()
    {
        using Form1 form = new();
        SplitContainer horizontalSplit = GetPrivateField<SplitContainer>(form, "_horizontalSplit");
        MenuStrip menu = GetPrivateField<MenuStrip>(form, "menuStrip1");

        InvokePrivateInstance(form, "LayoutSplitContainerBelowMenu", null);

        Assert.AreEqual(menu.Bottom, horizontalSplit.Top);
    }

    [TestMethod]
    [TestCategory("Scenario-Based Testing")]
    public void ResolveBlockMetadata_EqualsBlockTypeResolved()
    {
        object result = InvokePrivateStatic("ResolveBlockMetadata", ["=="])!;
        Type tupleType = result.GetType();
        CodeBlockType blockType = (CodeBlockType)tupleType.GetField("Item1")!.GetValue(result)!;
        string blockName = (string)tupleType.GetField("Item2")!.GetValue(result)!;

        Assert.AreEqual(CodeBlockType.Equals, blockType);
        Assert.AreEqual("==", blockName);
    }

    [TestMethod]
    [TestCategory("Scenario-Based Testing")]
    public void ResolveBlockMetadata_VariableBlockResolved()
    {
        VariableBlock variable = VariableBlock.CreateInt("score", 7);
        object result = InvokePrivateStatic("ResolveBlockMetadata", [variable])!;
        Type tupleType = result.GetType();
        CodeBlockType blockType = (CodeBlockType)tupleType.GetField("Item1")!.GetValue(result)!;
        string blockName = (string)tupleType.GetField("Item2")!.GetValue(result)!;
        VariableBlockType? variableType = (VariableBlockType?)tupleType.GetField("Item3")!.GetValue(result)!;

        Assert.AreEqual(CodeBlockType.Variable, blockType);
        Assert.AreEqual("score", blockName);
        Assert.AreEqual(VariableBlockType.Int, variableType);
    }

    [TestMethod]
    [TestCategory("Scenario-Based Testing")]
    public void ResolveBlockMetadata_UnknownLabelResolvedAsUnknown()
    {
        object result = InvokePrivateStatic("ResolveBlockMetadata", ["CustomBlock"])!;
        Type tupleType = result.GetType();
        CodeBlockType blockType = (CodeBlockType)tupleType.GetField("Item1")!.GetValue(result)!;
        string blockName = (string)tupleType.GetField("Item2")!.GetValue(result)!;

        Assert.AreEqual(CodeBlockType.Unknown, blockType);
        Assert.AreEqual("CustomBlock", blockName);
    }

    [TestMethod]
    [TestCategory("Scenario-Based Testing")]
    public void GetBlockDisplayText_VariableBlockIncludesNameAndType()
    {
        string text = (string)InvokePrivateStatic("GetBlockDisplayText", [VariableBlock.CreateBool("enabled")])!;

        Assert.AreEqual("enabled : Bool", text);
    }

    private static object? InvokePrivateStatic(string methodName, object?[]? args)
    {
        MethodInfo? method = typeof(Form1).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, $"Unable to find Form1.{methodName}");
        return method.Invoke(null, args);
    }

    private static object? InvokePrivateInstance(object instance, string methodName, object?[]? args)
    {
        MethodInfo? method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, $"Unable to find {instance.GetType().Name}.{methodName}");
        return method.Invoke(instance, args);
    }

    private static T GetPrivateField<T>(object instance, string fieldName) where T : class
    {
        FieldInfo? field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, $"Unable to find field {fieldName}");
        object? value = field.GetValue(instance);
        Assert.IsNotNull(value, $"Field {fieldName} was null");
        return (T)value;
    }
}
