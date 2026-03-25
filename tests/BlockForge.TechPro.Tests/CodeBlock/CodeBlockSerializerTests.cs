using COMP_3951_BlockForge_TechPro;


/// <summary>
/// BlockForge CodeBlockSerializerTests 
/// Author: Angus Grewal
/// Date: Mar 4 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace CodeBlockTests;
[TestClass]
public sealed class CodeBlockSerializerTests
{
    [TestMethod]
    public void Serialize_SingleCodeBlock()
    {
        var block = new CodeBlock(150, 300, "UID-1");

        string json = CodeBlockSerializer.Serialize(block);
        var load = CodeBlockSerializer.DeserializeSingle(json);

        Assert.IsNotNull(load);
        Assert.AreEqual(block.Uid, load.Uid);
        Assert.AreEqual(block.PosX, load.PosX, 1e-6);
        Assert.AreEqual(block.PosY, load.PosY, 1e-6);
    }

    [TestMethod]
    public void Serialize_ListOfCodeBlocks()
    {
        var block1 = new CodeBlock(150, 300, "UID-1");
        var block2 = new CodeBlock(300, 600, "UID-2");
        var block3 = new CodeBlock(450, 900, "UID-3");

        var blocks = new List<CodeBlock> { block1, block2, block3 };

        string json = CodeBlockSerializer.Serialize(blocks);
        var load = CodeBlockSerializer.DeserializeList(json);

        Assert.IsNotNull(load);
        Assert.AreEqual(3, load.Count);
        Assert.AreEqual("UID-1", load[0].Uid);
        Assert.AreEqual("UID-2", load[1].Uid);
        Assert.AreEqual("UID-3", load[2].Uid);
    }

    [TestMethod]
    public void Deserialize_Empty()
    {
        var load = CodeBlockSerializer.DeserializeList("[]");

        Assert.IsNotNull(load);
        Assert.AreEqual(0, load.Count);
    }
}
