namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Represents an operator block whose selected symbol is chosen from a predefined set.
    /// </summary>
    public sealed class OperatorBlock
    {
        public static readonly string[] SupportedOperators = ["+", "-", "*", "/", "%"];

        public OperatorBlock(string selectedOperator = "+")
        {
            SelectedOperator = string.IsNullOrWhiteSpace(selectedOperator) ? "+" : selectedOperator;
        }

        public string SelectedOperator { get; set; }
    }
}
