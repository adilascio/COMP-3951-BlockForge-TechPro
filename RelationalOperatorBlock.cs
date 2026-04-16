namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Represents a relational operator block whose symbol is chosen from a small allowed set.
    /// </summary>
    public sealed class RelationalOperatorBlock
    {
        public static readonly string[] LessThanOperators = ["<", "<="];
        public static readonly string[] GreaterThanOperators = [">", ">="];

        public RelationalOperatorBlock(string[] supportedOperators, string selectedOperator)
        {
            SupportedOperators = supportedOperators;
            SelectedOperator = string.IsNullOrWhiteSpace(selectedOperator) ? supportedOperators[0] : selectedOperator;
        }

        public string[] SupportedOperators { get; }

        public string SelectedOperator { get; set; }
    }
}
