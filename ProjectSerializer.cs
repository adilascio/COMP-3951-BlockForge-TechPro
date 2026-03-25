using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace COMP_3951_BlockForge_TechPro
{
    public class ProjectSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
        };

        public static string Serialize(Project project)
        {
            return JsonSerializer.Serialize(project, Options);
        }

        public static Project Deserialize(string json)
        {
            var project = JsonSerializer.Deserialize<Project>(json);

            if (project == null)
            {
                throw new InvalidOperationException("Invalid JSON data for Project.");
            }

            return project;
        }
    }
}
