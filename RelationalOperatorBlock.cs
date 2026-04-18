namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Represents a relational operator block whose symbol is chosen from a small allowed set.
    /// </summary>
    /// <remarks>
    /// Recent relational-operator block work completed in this chat by Asher Drybrough, A01412779.
    /// </remarks>
    public sealed class RelationalOperatorBlock
    {
        public static readonly string[] LessThanOperators = ["<", "<="];
        public static readonly string[] GreaterThanOperators = [">", ">="];

        /// <summary>
        /// Initializes a new relational operator block with a constrained operator set and current selection.
        /// </summary>
        /// <param name="supportedOperators">The allowed symbols that can be selected for this block.</param>
        /// <param name="selectedOperator">The currently selected symbol.</param>
        public RelationalOperatorBlock(string[] supportedOperators, string selectedOperator)
        {
            SupportedOperators = supportedOperators;
            SelectedOperator = string.IsNullOrWhiteSpace(selectedOperator) ? supportedOperators[0] : selectedOperator;
        }

        public string[] SupportedOperators { get; }

        public string SelectedOperator { get; set; }
    }
}
