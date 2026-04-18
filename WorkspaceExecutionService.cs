using System;
using System.Collections.Generic;
using System.Linq;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Executes the current workspace in-memory using the existing vertical flow and horizontal statement links.
    /// </summary>
    /// <remarks>
    /// Recent execution/interpreter work completed in this chat by Asher Drybrough, A01412779.
    /// </remarks>
    public sealed class WorkspaceExecutionService
    {
        private readonly StatementChainService _statementChainService = new();

        /// <summary>
        /// Executes the current workspace by following the Run block's vertical chain and evaluating each horizontal statement row.
        /// </summary>
        /// <param name="blocks">The workspace blocks keyed by UID.</param>
        /// <returns>The captured output, diagnostics, and runtime variable state for the execution pass.</returns>
        public WorkspaceExecutionResult Execute(IDictionary<string, CodeBlock> blocks)
        {
            WorkspaceExecutionResult result = new();
            if (blocks.Count == 0)
            {
                result.Diagnostics.Add("Execution aborted: workspace is empty.");
                return result;
            }

            InitializeVariableState(blocks, result);

            CodeBlock? runBlock = blocks.Values
                .Where(block => block.BlockType == CodeBlockType.Run)
                .OrderBy(block => block.GridRow)
                .ThenBy(block => block.GridColumn)
                .FirstOrDefault();

            if (runBlock == null)
            {
                result.Diagnostics.Add("Execution aborted: no Run block found.");
                return result;
            }

            HashSet<string> visited = new();
            CodeBlock? current = runBlock;

            while (current != null)
            {
                if (!visited.Add(current.Uid))
                {
                    result.Diagnostics.Add("Execution aborted: a loop was detected in the vertical flow.");
                    break;
                }

                if (current.BlockType != CodeBlockType.Run)
                {
                    ExecuteStatement(current, blocks, result);
                }

                current = string.IsNullOrWhiteSpace(current.ChildBlockUid) ||
                          !blocks.TryGetValue(current.ChildBlockUid, out CodeBlock? child)
                    ? null
                    : child;
            }

            return result;
        }

        private void InitializeVariableState(IDictionary<string, CodeBlock> blocks, WorkspaceExecutionResult result)
        {
            foreach (CodeBlock block in blocks.Values
                         .Where(block => block.BlockType == CodeBlockType.Variable && !string.IsNullOrWhiteSpace(block.BlockName))
                         .OrderBy(block => block.GridRow)
                         .ThenBy(block => block.GridColumn))
            {
                if (result.VariableState.ContainsKey(block.BlockName!))
                {
                    continue;
                }

                result.VariableState[block.BlockName!] = GetVariableValue(block);
            }
        }

        private void ExecuteStatement(CodeBlock statementBlock, IDictionary<string, CodeBlock> blocks, WorkspaceExecutionResult result)
        {
            List<CodeBlock> chain = _statementChainService.GetStatementChain(statementBlock, blocks);
            if (chain.Count == 0)
            {
                return;
            }

            CodeBlock root = chain[0];
            if (root.BlockType == CodeBlockType.Print)
            {
                if (chain.Count == 1)
                {
                    result.OutputLines.Add(string.Empty);
                    return;
                }

                object value = EvaluateExpression(chain.Skip(1).ToList(), result.VariableState);
                result.OutputLines.Add(FormatValue(value));
                return;
            }

            if (root.BlockType == CodeBlockType.Variable &&
                chain.Count >= 3 &&
                chain[1].BlockType == CodeBlockType.Assignment)
            {
                string variableName = root.BlockName ?? "variable";
                object value = EvaluateExpression(chain.Skip(2).ToList(), result.VariableState);
                object coercedValue = CoerceToVariableType(value, root.VariableType);

                result.VariableState[variableName] = coercedValue;
                UpdateMatchingVariableBlocks(blocks, variableName, root.VariableType, coercedValue);
                return;
            }

            if (root.BlockType == CodeBlockType.Variable && chain.Count == 1)
            {
                string variableName = root.BlockName ?? "variable";
                result.VariableState[variableName] = GetVariableValue(root);
                return;
            }

            if (IsStandaloneExpressionStatement(chain))
            {
                _ = EvaluateExpression(chain, result.VariableState);
                return;
            }

            result.Diagnostics.Add($"Unsupported statement: {_statementChainService.BuildStatementText(root, blocks)}");
        }

        private static bool IsStandaloneExpressionStatement(IReadOnlyList<CodeBlock> chain)
        {
            if (chain.Count < 3)
            {
                return false;
            }

            if (!IsOperandBlock(chain[0].BlockType))
            {
                return false;
            }

            for (int index = 1; index < chain.Count; index += 2)
            {
                if (index >= chain.Count || !IsOperatorBlock(chain[index].BlockType))
                {
                    return false;
                }

                if (index + 1 >= chain.Count || !IsOperandBlock(chain[index + 1].BlockType))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsOperandBlock(CodeBlockType blockType)
        {
            return blockType == CodeBlockType.Variable || blockType == CodeBlockType.Input;
        }

        private static bool IsOperatorBlock(CodeBlockType blockType)
        {
            return blockType == CodeBlockType.Operator ||
                   blockType == CodeBlockType.Equals ||
                   blockType == CodeBlockType.LessThan ||
                   blockType == CodeBlockType.GreaterThan;
        }

        private static void UpdateMatchingVariableBlocks(
            IDictionary<string, CodeBlock> blocks,
            string variableName,
            VariableBlockType? variableType,
            object value)
        {
            foreach (CodeBlock block in blocks.Values.Where(block => block.BlockType == CodeBlockType.Variable && block.BlockName == variableName))
            {
                block.StringValue = null;
                block.IntValue = null;
                block.BoolValue = null;

                switch (variableType)
                {
                    case VariableBlockType.String:
                        block.StringValue = Convert.ToString(value) ?? string.Empty;
                        break;
                    case VariableBlockType.Int:
                        block.IntValue = Convert.ToInt32(value);
                        break;
                    case VariableBlockType.Bool:
                        block.BoolValue = Convert.ToBoolean(value);
                        break;
                }
            }
        }

        private object EvaluateExpression(IReadOnlyList<CodeBlock> tokens, IDictionary<string, object> variableState)
        {
            if (tokens.Count == 0)
            {
                throw new InvalidOperationException("Cannot evaluate an empty expression.");
            }

            object value = ResolveOperand(tokens[0], variableState);

            for (int index = 1; index < tokens.Count; index += 2)
            {
                if (index + 1 >= tokens.Count)
                {
                    throw new InvalidOperationException("Expression ended with an operator and is incomplete.");
                }

                string op = GetOperatorToken(tokens[index]);
                object right = ResolveOperand(tokens[index + 1], variableState);
                value = ApplyOperator(value, op, right);
            }

            return value;
        }

        private static object ResolveOperand(CodeBlock block, IDictionary<string, object> variableState)
        {
            return block.BlockType switch
            {
                CodeBlockType.Variable => ResolveVariable(block, variableState),
                CodeBlockType.Input => GetInputValue(block),
                _ => throw new InvalidOperationException($"Unsupported operand block: {block.BlockType}.")
            };
        }

        private static object ResolveVariable(CodeBlock block, IDictionary<string, object> variableState)
        {
            string variableName = block.BlockName ?? throw new InvalidOperationException("Variable block is missing a name.");
            if (variableState.TryGetValue(variableName, out object? value))
            {
                return value;
            }

            object initialValue = GetVariableValue(block);
            variableState[variableName] = initialValue;
            return initialValue;
        }

        private static object GetVariableValue(CodeBlock block)
        {
            return block.VariableType switch
            {
                VariableBlockType.String => block.StringValue ?? string.Empty,
                VariableBlockType.Int => block.IntValue ?? 0,
                VariableBlockType.Bool => block.BoolValue ?? false,
                _ => block.StringValue ?? block.BlockName ?? string.Empty
            };
        }

        private static object GetInputValue(CodeBlock block)
        {
            return block.VariableType switch
            {
                VariableBlockType.String => block.StringValue ?? string.Empty,
                VariableBlockType.Int => block.IntValue ?? 0,
                VariableBlockType.Bool => block.BoolValue ?? false,
                _ => block.StringValue ?? string.Empty
            };
        }

        private static string GetOperatorToken(CodeBlock block)
        {
            return block.BlockType switch
            {
                CodeBlockType.Operator => string.IsNullOrWhiteSpace(block.StringValue) ? "+" : block.StringValue!,
                CodeBlockType.Equals => "==",
                CodeBlockType.LessThan => string.IsNullOrWhiteSpace(block.StringValue) ? "<" : block.StringValue!,
                CodeBlockType.GreaterThan => string.IsNullOrWhiteSpace(block.StringValue) ? ">" : block.StringValue!,
                _ => throw new InvalidOperationException($"Unsupported operator block: {block.BlockType}.")
            };
        }

        private static object ApplyOperator(object left, string op, object right)
        {
            return op switch
            {
                "+" => ApplyAddition(left, right),
                "-" => Convert.ToInt32(left) - Convert.ToInt32(right),
                "*" => Convert.ToInt32(left) * Convert.ToInt32(right),
                "/" => ApplyDivision(left, right),
                "%" => ApplyModulo(left, right),
                "==" => Equals(left, right),
                "<" => CompareNumeric(left, right) < 0,
                "<=" => CompareNumeric(left, right) <= 0,
                ">" => CompareNumeric(left, right) > 0,
                ">=" => CompareNumeric(left, right) >= 0,
                _ => throw new InvalidOperationException($"Unsupported operator '{op}'.")
            };
        }

        private static int CompareNumeric(object left, object right)
        {
            return Convert.ToInt32(left).CompareTo(Convert.ToInt32(right));
        }

        private static object ApplyAddition(object left, object right)
        {
            if (left is string || right is string)
            {
                return $"{FormatValue(left)}{FormatValue(right)}";
            }

            return Convert.ToInt32(left) + Convert.ToInt32(right);
        }

        private static object ApplyDivision(object left, object right)
        {
            int divisor = Convert.ToInt32(right);
            if (divisor == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            return Convert.ToInt32(left) / divisor;
        }

        private static object ApplyModulo(object left, object right)
        {
            int divisor = Convert.ToInt32(right);
            if (divisor == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            return Convert.ToInt32(left) % divisor;
        }

        private static object CoerceToVariableType(object value, VariableBlockType? variableType)
        {
            return variableType switch
            {
                VariableBlockType.String => Convert.ToString(value) ?? string.Empty,
                VariableBlockType.Int => Convert.ToInt32(value),
                VariableBlockType.Bool => Convert.ToBoolean(value),
                _ => value
            };
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                bool boolValue => boolValue ? "true" : "false",
                _ => Convert.ToString(value) ?? string.Empty
            };
        }
    }

    /// <summary>
    /// Stores the output and runtime variable state produced by a lightweight workspace execution pass.
    /// </summary>
    /// <remarks>
    /// Recent execution/interpreter work completed in this chat by Asher Drybrough, A01412779.
    /// </remarks>
    public sealed class WorkspaceExecutionResult
    {
        public List<string> OutputLines { get; } = new();
        public List<string> Diagnostics { get; } = new();
        public Dictionary<string, object> VariableState { get; } = new(StringComparer.Ordinal);
    }
}
