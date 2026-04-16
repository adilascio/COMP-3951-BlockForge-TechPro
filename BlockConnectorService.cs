using System;
using System.Collections.Generic;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// Applies the v1 parent/child connector rules for vertical block chains.
    /// </summary>
    public class BlockConnectorService
    {
        private readonly int _cellWidth;
        private readonly int _cellHeight;

        public BlockConnectorService(int cellWidth = 140, int cellHeight = 72)
        {
            _cellWidth = cellWidth;
            _cellHeight = cellHeight;
        }

        public bool CanConnect(CodeBlock parent, CodeBlock child)
        {
            if (parent.Uid == child.Uid)
            {
                return false;
            }

            if (!IsFlowBlock(parent) || !IsFlowBlock(child))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(parent.ChildBlockUid))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(child.ParentBlockUid))
            {
                return false;
            }

            return true;
        }

        public void Connect(CodeBlock parent, CodeBlock child)
        {
            if (!CanConnect(parent, child))
            {
                throw new InvalidOperationException("Blocks cannot be connected under the current connector rules.");
            }

            parent.ChildBlockUid = child.Uid;
            child.ParentBlockUid = parent.Uid;
            AlignChildToParent(parent, child);
        }

        public void Disconnect(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            if (!string.IsNullOrWhiteSpace(block.ParentBlockUid) && blocks.TryGetValue(block.ParentBlockUid, out CodeBlock? parent))
            {
                parent.ChildBlockUid = null;
            }

            if (!string.IsNullOrWhiteSpace(block.ChildBlockUid) && blocks.TryGetValue(block.ChildBlockUid, out CodeBlock? child))
            {
                child.ParentBlockUid = null;
            }

            block.ParentBlockUid = null;
            block.ChildBlockUid = null;
            DisconnectStatement(block, blocks);
        }

        public bool CanConnectStatement(CodeBlock leftBlock, CodeBlock rightBlock)
        {
            if (leftBlock.Uid == rightBlock.Uid)
            {
                return false;
            }

            if (!IsStatementBlock(leftBlock) || !IsStatementBlock(rightBlock))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(leftBlock.NextStatementBlockUid))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rightBlock.PreviousStatementBlockUid))
            {
                return false;
            }

            return true;
        }

        public void ConnectStatement(CodeBlock leftBlock, CodeBlock rightBlock)
        {
            if (!CanConnectStatement(leftBlock, rightBlock))
            {
                throw new InvalidOperationException("Blocks cannot be connected side-by-side under the current statement rules.");
            }

            leftBlock.NextStatementBlockUid = rightBlock.Uid;
            rightBlock.PreviousStatementBlockUid = leftBlock.Uid;
        }

        public void DisconnectStatement(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            if (!string.IsNullOrWhiteSpace(block.PreviousStatementBlockUid) &&
                blocks.TryGetValue(block.PreviousStatementBlockUid, out CodeBlock? previous))
            {
                previous.NextStatementBlockUid = null;
            }

            if (!string.IsNullOrWhiteSpace(block.NextStatementBlockUid) &&
                blocks.TryGetValue(block.NextStatementBlockUid, out CodeBlock? next))
            {
                next.PreviousStatementBlockUid = null;
            }

            block.PreviousStatementBlockUid = null;
            block.NextStatementBlockUid = null;
        }

        public void MoveChain(CodeBlock root, IDictionary<string, CodeBlock> blocks, int newGridColumn, int newGridRow)
        {
            root.UpdateGridPosition(newGridColumn, newGridRow);
            root.UpdatePosition(newGridColumn * _cellWidth, newGridRow * _cellHeight);

            CodeBlock current = root;
            int nextRow = newGridRow + 1;

            while (!string.IsNullOrWhiteSpace(current.ChildBlockUid) && blocks.TryGetValue(current.ChildBlockUid, out CodeBlock? child))
            {
                child.UpdateGridPosition(newGridColumn, nextRow);
                child.UpdatePosition(newGridColumn * _cellWidth, nextRow * _cellHeight);
                current = child;
                nextRow++;
            }
        }

        public void DeleteBlock(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            string? parentUid = block.ParentBlockUid;
            string? childUid = block.ChildBlockUid;
            string? previousStatementUid = block.PreviousStatementBlockUid;
            string? nextStatementUid = block.NextStatementBlockUid;

            Disconnect(block, blocks);
            blocks.Remove(block.Uid);

            if (!string.IsNullOrWhiteSpace(parentUid) && blocks.TryGetValue(parentUid, out CodeBlock? parent))
            {
                parent.ChildBlockUid = null;
            }

            if (!string.IsNullOrWhiteSpace(childUid) && blocks.TryGetValue(childUid, out CodeBlock? child))
            {
                child.ParentBlockUid = null;
            }

            if (!string.IsNullOrWhiteSpace(previousStatementUid) && blocks.TryGetValue(previousStatementUid, out CodeBlock? previous))
            {
                previous.NextStatementBlockUid = null;
            }

            if (!string.IsNullOrWhiteSpace(nextStatementUid) && blocks.TryGetValue(nextStatementUid, out CodeBlock? next))
            {
                next.PreviousStatementBlockUid = null;
            }
        }

        public List<CodeBlock> GetChainFrom(CodeBlock root, IDictionary<string, CodeBlock> blocks)
        {
            List<CodeBlock> chain = new() { root };
            CodeBlock current = root;

            while (!string.IsNullOrWhiteSpace(current.ChildBlockUid) && blocks.TryGetValue(current.ChildBlockUid, out CodeBlock? child))
            {
                chain.Add(child);
                current = child;
            }

            return chain;
        }

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

        public List<CodeBlock> GetStatementChainFrom(CodeBlock block, IDictionary<string, CodeBlock> blocks)
        {
            List<CodeBlock> statementChain = new();
            CodeBlock current = GetStatementRoot(block, blocks);
            statementChain.Add(current);

            while (!string.IsNullOrWhiteSpace(current.NextStatementBlockUid) &&
                   blocks.TryGetValue(current.NextStatementBlockUid, out CodeBlock? next))
            {
                statementChain.Add(next);
                current = next;
            }

            return statementChain;
        }

        private void AlignChildToParent(CodeBlock parent, CodeBlock child)
        {
            int childColumn = parent.GridColumn;
            int childRow = parent.GridRow + 1;
            child.UpdateGridPosition(childColumn, childRow);
            child.UpdatePosition(childColumn * _cellWidth, childRow * _cellHeight);
        }

        private static bool IsFlowBlock(CodeBlock block)
        {
            return block.BlockType switch
            {
                CodeBlockType.Run => true,
                CodeBlockType.Print => true,
                CodeBlockType.If => true,
                CodeBlockType.While => true,
                CodeBlockType.Variable => true,
                _ => false
            };
        }

        private static bool IsStatementBlock(CodeBlock block)
        {
            return block.BlockType switch
            {
                CodeBlockType.Run => false,
                CodeBlockType.If => true,
                CodeBlockType.While => true,
                CodeBlockType.Print => true,
                CodeBlockType.Variable => true,
                CodeBlockType.Assignment => true,
                CodeBlockType.Operator => true,
                CodeBlockType.Equals => true,
                CodeBlockType.LessThan => true,
                CodeBlockType.GreaterThan => true,
                CodeBlockType.Input => true,
                _ => false
            };
        }
    }
}
