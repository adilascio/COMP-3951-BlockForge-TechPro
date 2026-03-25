using COMP_3951_BlockForge_TechPro;


/// <summary>
/// BlockForge CodeBlockTests 
/// Author: Angus Grewal
/// Date: Mar 4 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace CodeBlockTests;
[TestClass]
public sealed class CodeBlockTests
{
    [TestMethod]
    public void CodeBlock_Construction()
    {
        var block = new CodeBlock(150, 300, "UID-1");

        Assert.AreEqual(150, block.PosX, 1e-6);
        Assert.AreEqual(300, block.PosY, 1e-6);
        Assert.AreEqual("UID-1", block.Uid);
    }

    [TestMethod]
    public void CodeBlock_UpdateXandYProperties()
    {
        var block = new CodeBlock(150, 300, "UID-1");

        block.UpdatePosition(block.PosX * 2, block.PosY * 2);

        Assert.AreEqual(300, block.PosX, 1e-6);
        Assert.AreEqual(600, block.PosY, 1e-6);
        Assert.AreEqual("UID-1", block.Uid);
    }
}
