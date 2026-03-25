using System;
using System.Collections.Generic;
using System.Text;

namespace COMP_3951_BlockForge_TechPro
{
    public class ProjectFileManager
    {
        private PayloadTransformer _transformer;

        public ProjectFileManager(PayloadTransformer transformer)
        {
            _transformer = transformer;
        }

        public void SaveFile(Project project)
        {
            List<string> errors = CodeBlockValidator.Validate(project.CodeBlocks);
            if (errors.Count > 0)
            {
                string message = "Validation failed:\n" + string.Join("\n", errors);
                throw new InvalidOperationException(message);
            }

            if (project.ProjectName == null)
            {
                throw new InvalidDataException("Project name cannot be null when saving.");
            }

            string json = ProjectSerializer.Serialize(project);
            string scrambled = _transformer.Scramble(json);
            byte[] encoded = Encoding.UTF8.GetBytes(scrambled);

            File.WriteAllBytes(project.ProjectName + ".bfg", encoded);
        }

        public Project LoadFile(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);
            string scrambled = Encoding.UTF8.GetString(data);
            string json = _transformer.Unscramble(scrambled);

            Project project = ProjectSerializer.Deserialize(json);

            if (project.ProjectName == null)
            {
                throw new ArgumentNullException("Project name was null when loading.");
            }

            List<string> errors = CodeBlockValidator.Validate(project.CodeBlocks);

            if (errors.Count > 0)
            {
                string message = "Validation failed:\n" + string.Join("\n", errors);
                throw new InvalidOperationException(message);
            }

            return project;
        }
    }
}
