using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.ProjectTests;

[TestClass]
public class ProjectTests
{
    [TestMethod]
    public void Project_Construction()
    {
        var block1 = new CodeBlock(150, 300, "UID-1");
        var block2 = new CodeBlock(350, 600, "UID-2");
        var block3 = new CodeBlock(450, 900, "UID-3");

        var blocks = new List<CodeBlock> { block1,  block2, block3 };

        var project = new Project("test_project",  blocks);

        Assert.AreEqual("test_project", project.ProjectName);
        Assert.AreEqual(1, project.Version);
        Assert.AreEqual(blocks, project.CodeBlocks);
    }

    [TestMethod]
    public void Project_UpdateBlocks()
    {
        var block1 = new CodeBlock(150, 300, "UID-1");
        var block2 = new CodeBlock(350, 600, "UID-2");
        var block3 = new CodeBlock(450, 900, "UID-3");

        var blocks1 = new List<CodeBlock> { block1, block2, block3 };

        var block4 = new CodeBlock(150, 300, "UID-1");
        var block5 = new CodeBlock(350, 600, "UID-2");
        var block6 = new CodeBlock(450, 900, "UID-3");

        var blocks2 = new List<CodeBlock> { block4, block5, block6 };

        var project = new Project("test_project", blocks1);
        project.UpdateBlocks(blocks2);
        Assert.AreEqual(blocks2, project.CodeBlocks);
    }
}
