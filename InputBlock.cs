using System;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Represents a typed literal input block whose value can be edited directly in the workspace.
    /// </summary>
    public sealed class InputBlock
    {
        public InputBlock(VariableBlockType primitiveType = VariableBlockType.String, string valueText = "")
        {
            PrimitiveType = primitiveType;
            ValueText = valueText;
        }

        public VariableBlockType PrimitiveType { get; set; }

        public string ValueText { get; set; }

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
