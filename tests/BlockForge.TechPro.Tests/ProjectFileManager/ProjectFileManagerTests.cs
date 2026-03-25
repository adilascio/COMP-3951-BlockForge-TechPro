using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.ProjectFileManagerTests;

[TestClass]
public class ProjectFileManagerTests
{
    [TestMethod]
    public void ProjectFile_IntegrationPipeline()
    {
        PayloadTransformer transformer = new PayloadTransformer(5);

        ProjectFileManager filemanager = new ProjectFileManager(transformer);

        List<CodeBlock> blocks = new List<CodeBlock>();
        blocks.Add(new CodeBlock(150, 300, "UID-1"));
        blocks.Add(new CodeBlock(300, 600, "UID-2"));
        blocks.Add(new CodeBlock(450, 900, "UID-3"));

        Project project = new Project("test_project", blocks);

        string filepath = project.ProjectName + ".bfg";

        try
        {
            filemanager.SaveFile(project);

            Project loaded = filemanager.LoadFile(filepath);

            Assert.AreEqual(project.ProjectName, loaded.ProjectName);
            Assert.AreEqual(project.Version, loaded.Version);
            Assert.HasCount(project.CodeBlocks.Count, loaded.CodeBlocks);
            Assert.AreEqual(project.CodeBlocks[0].Uid, loaded.CodeBlocks[0].Uid);
            Assert.AreEqual(project.CodeBlocks[0].PosX, loaded.CodeBlocks[0].PosX, 1e-6);
            Assert.AreEqual(project.CodeBlocks[0].PosY, loaded.CodeBlocks[0].PosY, 1e-6);
            Assert.AreEqual(project.CodeBlocks[2].Uid, loaded.CodeBlocks[2].Uid);

        }
        finally
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }
    }
}
