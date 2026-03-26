using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.CodeGeneration;

[TestClass]
public sealed class JavaBlockMapTests
{
    [TestMethod]
    public void ToJava_IfBlock_ReturnsIfTemplate()
    {
        CodeBlock block = new(0, 0, "if-1", blockType: CodeBlockType.If, blockName: "If");

        string java = JavaBlockMap.ToJava(block);

        Assert.AreEqual("if (condition) {\n}", java);
    }

    [TestMethod]
    public void ToJava_PrintBlock_ReturnsPrintTemplate()
    {
        CodeBlock block = new(0, 0, "print-1", blockType: CodeBlockType.Print, blockName: "Print");

        string java = JavaBlockMap.ToJava(block);

        Assert.AreEqual("System.out.println(value);", java);
    }

    [TestMethod]
    public void ToJava_StringVariable_ReturnsStringDeclaration()
    {
        CodeBlock block = new(
            0,
            0,
            "var-1",
            blockType: CodeBlockType.Variable,
            blockName: "username",
            variableType: VariableBlockType.String);

        string java = JavaBlockMap.ToJava(block);

        Assert.AreEqual("String username = \"\";", java);
    }

    [TestMethod]
    public void ToJava_UnknownType_Throws()
    {
        CodeBlock block = new(0, 0, "unknown-1");

        Assert.Throws<InvalidOperationException>(() => JavaBlockMap.ToJava(block));
    }
}
