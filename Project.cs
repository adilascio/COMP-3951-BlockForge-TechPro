using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// BlockForge Project 
/// Author: Angus Grewal
/// Date: Mar 25 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Data Transfer Object that represents a Project File as a persisting object, containing metadata for project name and file format version.
    /// Also contains a list of all CodeBlocks present within the project.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// File format version metadata.
        /// </summary>
        public int Version { get; set; } = 1;
        /// <summary>
        /// Name of Project. May be empty at first but must be assigned when saving/loading as it will become the filename.
        /// </summary>
        public string? ProjectName { get; set; }
        /// <summary>
        /// The list of CodeBlocks associated with the Project.
        /// </summary>
        public List<CodeBlock> CodeBlocks { get; set; } = new List<CodeBlock>();

        /// <summary>
        /// Constructor for a Project.
        /// </summary>
        /// <param name="projectName">The name of the Project, which will also become the filename without extension.</param>
        /// <param name="codeBlocks">The CodeBlocks to include into the Project when saved.</param>
        public Project(string projectName, List<CodeBlock> codeBlocks)
        {
            this.ProjectName = projectName;
            this.CodeBlocks = codeBlocks;
        }

        /// <summary>
        /// Updates the list of CodeBlocks in the Project with the new list.
        /// </summary>
        /// <param name="blocks">list of new blocks to update.</param>
        public void UpdateBlocks(List<CodeBlock> blocks)
        {
            this.CodeBlocks = blocks;
        }
    }
}
