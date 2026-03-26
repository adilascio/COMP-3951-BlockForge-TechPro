using System.Text.Json;
using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.VariableBlocks;

/// <summary>
/// BlockForge VariableBlockTests
/// Author: Andre Di Lascio
/// Date: Mar 24 2026
/// Source: Written with the help of AI.
/// </summary>
[TestClass]
public sealed class VariableBlockTests
{
    [TestMethod]
    public void CreateString_CreatesVariableBlock_WithExpectedNameTypeAndValue()
    {
        VariableBlock block = VariableBlock.CreateString("playerName", "Alex");

        Assert.AreEqual("playerName", block.VariableName);
        Assert.AreEqual(VariableBlockType.String, block.VariableType);
        Assert.AreEqual("Alex", block.Value);
    }

    [TestMethod]
    public void CreateInt_CreatesVariableBlock_WithExpectedNameTypeAndValue()
    {
        VariableBlock block = VariableBlock.CreateInt("score", 42);

        Assert.AreEqual("score", block.VariableName);
        Assert.AreEqual(VariableBlockType.Int, block.VariableType);
        Assert.AreEqual(42, block.Value);
    }

    [TestMethod]
    public void CreateBool_CreatesVariableBlock_WithExpectedNameTypeAndValue()
    {
        VariableBlock block = VariableBlock.CreateBool("isVisible", true);

        Assert.AreEqual("isVisible", block.VariableName);
        Assert.AreEqual(VariableBlockType.Bool, block.VariableType);
        Assert.AreEqual(true, block.Value);
    }

    [TestMethod]
    public void CreateWithoutExplicitValue_UsesTypeAppropriateDefaults()
    {
        VariableBlock stringBlock = VariableBlock.CreateString("name");
        VariableBlock intBlock = VariableBlock.CreateInt("count");
        VariableBlock boolBlock = VariableBlock.CreateBool("enabled");

        Assert.AreEqual(string.Empty, stringBlock.Value);
        Assert.AreEqual(0, intBlock.Value);
        Assert.AreEqual(false, boolBlock.Value);
    }

    [TestMethod]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VariableBlock.CreateString(" "));
    }

    [TestMethod]
    public void UpdateValue_WrongType_ThrowsInvalidOperationException()
    {
        VariableBlock block = VariableBlock.CreateInt("score", 1);

        Assert.Throws<InvalidOperationException>(() => block.UpdateBoolValue(true));
    }

    [TestMethod]
    public void StringValue_EmptyString_IsAllowed()
    {
        VariableBlock block = VariableBlock.CreateString("playerName", "Alex");

        block.UpdateStringValue(string.Empty);

        Assert.AreEqual(string.Empty, block.Value);
    }

    [TestMethod]
    public void IntValue_NegativeInteger_IsPreserved()
    {
        VariableBlock block = VariableBlock.CreateInt("offset", -12);

        Assert.AreEqual(-12, block.Value);
    }

    [TestMethod]
    public void BoolValue_False_IsPreserved()
    {
        VariableBlock block = VariableBlock.CreateBool("isEnabled", false);

        Assert.AreEqual(false, block.Value);
    }

    [TestMethod]
    public void Serialization_PreservesVariableTypeAndValue()
    {
        VariableBlock block = VariableBlock.CreateBool("isReady", true);

        string json = JsonSerializer.Serialize(block);
        VariableBlock? restored = JsonSerializer.Deserialize<VariableBlock>(json);

        Assert.IsNotNull(restored);
        Assert.AreEqual("isReady", restored.VariableName);
        Assert.AreEqual(VariableBlockType.Bool, restored.VariableType);
        Assert.AreEqual(true, restored.Value);
    }
}
