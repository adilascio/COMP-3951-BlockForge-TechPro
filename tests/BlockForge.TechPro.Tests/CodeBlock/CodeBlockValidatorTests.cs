using COMP_3951_BlockForge_TechPro;


/// <summary>
/// BlockForge CodeBlockValidatorTests 
/// Author: Angus Grewal
/// Date: Mar 4 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace CodeBlockTests;
[TestClass]
public sealed class CodeBlockValidatorTests
{
    [TestMethod]
    public void Validate_DuplicateUIDCodeBlock_Fails()
    {
        var block1 = new CodeBlock(150, 300, "UID-1");
        var block2 = new CodeBlock(300, 600, "UID-1");

        var blocks = new List<CodeBlock> { block1, block2 };

        var errors = CodeBlockValidator.Validate(blocks);

        Assert.IsGreaterThan(0, errors.Count);
    }

    [TestMethod]
    public void Validate_MissingUIDCodeBlock_Fails()
    {
        var block1 = new CodeBlock(150, 300, "");
        var block2 = new CodeBlock(300, 600, "UID-1");

        var blocks = new List<CodeBlock> { block1, block2 };

        var errors = CodeBlockValidator.Validate(blocks);

        Assert.IsGreaterThan(0, errors.Count);
    }

    [TestMethod]
    public void Validate_UniqueUIDs_Passes()
    {
        var block1 = new CodeBlock(150, 300, "UID-1");
        var block2 = new CodeBlock(300, 600, "UID-2");

        var blocks = new List<CodeBlock> { block1, block2 };

        var errors = CodeBlockValidator.Validate(blocks);

        Assert.AreEqual(0, errors.Count);
    }
}
