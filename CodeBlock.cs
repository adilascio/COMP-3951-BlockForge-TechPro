using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// BlockForge CodeBlock 
/// Author: Angus Grewal
/// Date: Mar 4 2026
/// Source: Self-written, with AI coaching. All code submitted is human written, based on ChatGPT guidance.
/// </summary>
namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Data Transfer Object that represents a "Block" from the UI with starting coordinates and a UID.
    /// Will expend to include different subtypes, inbound and outbound links to other code blocks and specific internal data such as basic primitives.
    /// </summary>
    public class CodeBlock
    {
        /// <summary>
        /// Gets or sets the horizontal pixel position for the block.
        /// </summary>
        public double PosX { get; set; }

        /// <summary>
        /// Gets or sets the vertical pixel position for the block.
        /// </summary>
        public double PosY { get; set; }

        /// <summary>
        /// Gets or sets the occupied grid column for the block.
        /// </summary>
        public int GridColumn { get; set; }

        /// <summary>
        /// Gets or sets the occupied grid row for the block.
        /// </summary>
        public int GridRow { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the block.
        /// </summary>
        public string Uid { get; set; }
        public CodeBlockType BlockType { get; set; }
        public string? BlockName { get; set; }
        public VariableBlockType? VariableType { get; set; }

        /// <summary>
        /// Constructor for a CodeBlock.
        /// </summary>
        /// <param name="posX">starting X coordinate to place from.</param>
        /// <param name="posY">starting Y coordinate to place from.</param>
        /// <param name="uid">unique identifier for an instance of CodeBlock. Uniqueness will be enforced elsewhere, for now the CodeBlockValidator will log duplicates.</param>
        /// <param name="gridColumn">The occupied grid column.</param>
        /// <param name="gridRow">The occupied grid row.</param>
        public CodeBlock(
            double posX,
            double posY,
            String uid,
            int gridColumn = 0,
            int gridRow = 0,
            CodeBlockType blockType = CodeBlockType.Unknown,
            string? blockName = null,
            VariableBlockType? variableType = null)
        {
            this.PosX = posX;
            this.PosY = posY;
            this.Uid = uid;
            this.GridColumn = gridColumn;
            this.GridRow = gridRow;
            this.BlockType = blockType;
            this.BlockName = blockName;
            this.VariableType = variableType;
        }

        /// <summary>
        /// Simple method that updates a CodeBlock's position properties,
        /// eg when dragged from the UI layer and resaved.
        /// </summary>
        /// <param name="posX">new X coordinate.</param>
        /// <param name="posY">new Y coordinate.</param>
        public void UpdatePosition(double posX, double posY)
        {
            this.PosX = posX;
            this.PosY = posY;
        }

        /// <summary>
        /// Updates the stored grid position for the block.
        /// </summary>
        /// <param name="gridColumn">occupied grid column.</param>
        /// <param name="gridRow">occupied grid row.</param>
        public void UpdateGridPosition(int gridColumn, int gridRow)
        {
            this.GridColumn = gridColumn;
            this.GridRow = gridRow;
        }

        /// <summary>
        /// Updates the metadata associated with the block.
        /// </summary>
        /// <param name="blockType">The block type to store.</param>
        /// <param name="blockName">The display name of the block.</param>
        /// <param name="variableType">The variable type when the block represents a variable.</param>
        public void UpdateBlockMetadata(CodeBlockType blockType, string? blockName = null, VariableBlockType? variableType = null)
        {
            this.BlockType = blockType;
            this.BlockName = blockName;
            this.VariableType = variableType;
        }
    }
}
