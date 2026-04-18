using System;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Represents a typed literal input block whose value can be edited directly in the workspace.
    /// </summary>
    /// <remarks>
    /// Recent input-block work completed in this chat by Asher Drybrough, A01412779.
    /// </remarks>
    public sealed class InputBlock
    {
        /// <summary>
        /// Initializes a new input block with a selected primitive type and editable text value.
        /// </summary>
        /// <param name="primitiveType">The primitive type represented by the input block.</param>
        /// <param name="valueText">The editable text shown for the input's literal value.</param>
        public InputBlock(VariableBlockType primitiveType = VariableBlockType.String, string valueText = "")
        {
            PrimitiveType = primitiveType;
            ValueText = valueText;
        }

        public VariableBlockType PrimitiveType { get; set; }

        public string ValueText { get; set; }

        /// <summary>
        /// Converts the stored text into the primitive value currently selected for the input block.
        /// </summary>
        /// <returns>The typed value represented by the current text and primitive type.</returns>
        public object GetTypedValue()
        {
            return PrimitiveType switch
            {
                VariableBlockType.String => ValueText,
                VariableBlockType.Int => int.TryParse(ValueText, out int intValue) ? intValue : 0,
                VariableBlockType.Bool => bool.TryParse(ValueText, out bool boolValue) && boolValue,
                _ => ValueText
            };
        }
    }
}
