using System;
using System.Collections.Generic;
using System.Linq;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Provides traversal helpers for horizontally connected statement rows and their execution order.
    /// </summary>
    public sealed class StatementChainService
    {
        public CodeBlock GetStatementRoot(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            CodeBlock current = block;

            while (!string.IsNullOrWhiteSpace(current.PreviousStatementBlockUid) &&
                   blocks.TryGetValue(current.PreviousStatementBlockUid, out CodeBlock? previous))
            {
                current = previous;
            }

            return current;
        }

        public List<CodeBlock> GetStatementChain(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            List<CodeBlock> chain = new();
            CodeBlock current = GetStatementRoot(block, blocks);
            chain.Add(current);

            while (!string.IsNullOrWhiteSpace(current.NextStatementBlockUid) &&
                   blocks.TryGetValue(current.NextStatementBlockUid, out CodeBlock? next))
            {
                chain.Add(next);
                current = next;
            }

            return chain;
        }

        public List<CodeBlock> GetExecutionOrderedStatementRoots(IDictionary<string, CodeBlock> blocks)
        {
            List<CodeBlock> orderedRoots = new();
            HashSet<string> seenRoots = new();

            IEnumerable<CodeBlock> verticalRoots = blocks.Values
                .Where(block => string.IsNullOrWhiteSpace(block.ParentBlockUid))
                .OrderBy(block => block.GridRow)
                .ThenBy(block => block.GridColumn);

            foreach (CodeBlock verticalRoot in verticalRoots)
            {
                CodeBlock current = verticalRoot;

                while (true)
                {
                    CodeBlock statementRoot = GetStatementRoot(current, blocks);
                    if (seenRoots.Add(statementRoot.Uid))
                    {
                        orderedRoots.Add(statementRoot);
                    }

                    if (string.IsNullOrWhiteSpace(current.ChildBlockUid) ||
                        !blocks.TryGetValue(current.ChildBlockUid, out CodeBlock? child))
                    {
                        break;
                    }

                    current = child;
                }
            }

            foreach (CodeBlock statementRoot in blocks.Values
                         .Where(block => string.IsNullOrWhiteSpace(block.PreviousStatementBlockUid))
                         .OrderBy(block => block.GridRow)
                         .ThenBy(block => block.GridColumn))
            {
                if (seenRoots.Add(statementRoot.Uid))
                {
                    orderedRoots.Add(statementRoot);
                }
            }

            return orderedRoots;
        }

        public string BuildStatementText(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            List<CodeBlock> chain = GetStatementChain(block, blocks);
            if (chain.Count == 0)
            {
                return string.Empty;
            }

            if (chain[0].BlockType == CodeBlockType.Print)
            {
                string expression = string.Join(" ", chain.Skip(1).Select(GetInlineToken)).Trim();
                return string.IsNullOrWhiteSpace(expression)
                    ? "Print"
                    : $"Print {expression}";
            }

            return string.Join(" ", chain.Select(GetInlineToken)).Trim();
        }

        private static string GetInlineToken(CodeBlock block)
        {
            return block.BlockType switch
            {
                CodeBlockType.Variable => block.BlockName ?? "variable",
                CodeBlockType.Input => block.StringValue ?? string.Empty,
                CodeBlockType.Assignment => "=",
                CodeBlockType.Operator => string.IsNullOrWhiteSpace(block.StringValue) ? "+" : block.StringValue,
                CodeBlockType.Equals => "==",
                CodeBlockType.Print => "Print",
                CodeBlockType.If => "If",
                CodeBlockType.While => "While",
                CodeBlockType.Run => "Run",
                _ => block.BlockName ?? block.BlockType.ToString()
            };
        }
    }
}
