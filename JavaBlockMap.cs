using System;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Maps workspace blocks into Java syntax templates.
    /// </summary>
    public static class JavaBlockMap
    {
        public static string ToJava(CodeBlock block)
        {
            return block.BlockType switch
            {
                CodeBlockType.If => "if (condition) {\n}",
                CodeBlockType.While => "while (condition) {\n}",
                CodeBlockType.Run => "run();",
                CodeBlockType.Print => "System.out.println(value);",
                CodeBlockType.Equals => "==",
                CodeBlockType.Variable => MapVariable(block),
                _ => throw new InvalidOperationException($"Unsupported block type: {block.BlockType}")
            };
        }

        private static string MapVariable(CodeBlock block)
        {
            string variableName = string.IsNullOrWhiteSpace(block.BlockName) ? "variableName" : block.BlockName;

            return block.VariableType switch
            {
                VariableBlockType.String => $"String {variableName} = \"\";",
                VariableBlockType.Int => $"int {variableName} = 0;",
                VariableBlockType.Bool => $"boolean {variableName} = false;",
                _ => throw new InvalidOperationException("Variable blocks require a valid VariableType.")
            };
        }
    }
}
