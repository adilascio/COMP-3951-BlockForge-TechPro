using System;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge VariableBlock
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    public class VariableBlock
    {
        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the variable type.
        /// </summary>
        public VariableBlockType VariableType { get; set; }

        /// <summary>
        /// Gets or sets the string value when the variable type is <see cref="VariableBlockType.String"/>.
        /// </summary>
        public string? StringValue { get; set; }

        /// <summary>
        /// Gets or sets the integer value when the variable type is <see cref="VariableBlockType.Int"/>.
        /// </summary>
        public int? IntValue { get; set; }

        /// <summary>
        /// Gets or sets the Boolean value when the variable type is <see cref="VariableBlockType.Bool"/>.
        /// </summary>
        public bool? BoolValue { get; set; }

        /// <summary>
        /// Gets the currently active typed value for the variable block.
        /// </summary>
        public object Value =>
            VariableType switch
            {
                VariableBlockType.String => StringValue ?? string.Empty,
                VariableBlockType.Int => IntValue ?? 0,
                VariableBlockType.Bool => BoolValue ?? false,
                _ => throw new InvalidOperationException("Unsupported variable type.")
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableBlock"/> class.
        /// </summary>
        public VariableBlock()
        {
            VariableName = string.Empty;
            VariableType = VariableBlockType.String;
            StringValue = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableBlock"/> class with a validated name and type.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="variableType">The variable type.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="variableName"/> is null, empty, or whitespace.
        /// </exception>
        private VariableBlock(string variableName, VariableBlockType variableType)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentException("Variable name cannot be empty.", nameof(variableName));
            }

            VariableName = variableName;
            VariableType = variableType;
        }

        /// <summary>
        /// Creates a string variable block.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="value">The initial string value.</param>
        /// <returns>A configured string variable block.</returns>
        public static VariableBlock CreateString(string variableName, string value = "")
        {
            return new VariableBlock(variableName, VariableBlockType.String)
            {
                StringValue = value
            };
        }

        /// <summary>
        /// Creates an integer variable block.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="value">The initial integer value.</param>
        /// <returns>A configured integer variable block.</returns>
        public static VariableBlock CreateInt(string variableName, int value = 0)
        {
            return new VariableBlock(variableName, VariableBlockType.Int)
            {
                IntValue = value
            };
        }

        /// <summary>
        /// Creates a Boolean variable block.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="value">The initial Boolean value.</param>
        /// <returns>A configured Boolean variable block.</returns>
        public static VariableBlock CreateBool(string variableName, bool value = false)
        {
            return new VariableBlock(variableName, VariableBlockType.Bool)
            {
                BoolValue = value
            };
        }

        /// <summary>
        /// Updates the string value of the variable block.
        /// </summary>
        /// <param name="value">The new string value.</param>
        public void UpdateStringValue(string value)
        {
            EnsureType(VariableBlockType.String);
            StringValue = value;
        }

        /// <summary>
        /// Updates the integer value of the variable block.
        /// </summary>
        /// <param name="value">The new integer value.</param>
        public void UpdateIntValue(int value)
        {
            EnsureType(VariableBlockType.Int);
            IntValue = value;
        }

        /// <summary>
        /// Updates the Boolean value of the variable block.
        /// </summary>
        /// <param name="value">The new Boolean value.</param>
        public void UpdateBoolValue(bool value)
        {
            EnsureType(VariableBlockType.Bool);
            BoolValue = value;
        }

        /// <summary>
        /// Ensures that the variable block is being updated with the correct type.
        /// </summary>
        /// <param name="expectedType">The expected variable type.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the expected type does not match the block's current type.
        /// </exception>
        private void EnsureType(VariableBlockType expectedType)
        {
            if (VariableType != expectedType)
            {
                throw new InvalidOperationException($"Variable block type is {VariableType} and cannot store {expectedType} values.");
            }
        }
    }
}
