using System;
using System.Drawing;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge GridSnapService
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    public class GridSnapService
    {
        /// <summary>
        /// Gets the width of each grid cell in pixels.
        /// </summary>
        public int CellWidth { get; }

        /// <summary>
        /// Gets the height of each grid cell in pixels.
        /// </summary>
        public int CellHeight { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSnapService"/> class.
        /// </summary>
        /// <param name="cellWidth">The width of each grid cell in pixels.</param>
        /// <param name="cellHeight">The height of each grid cell in pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="cellWidth"/> or <paramref name="cellHeight"/> is less than or equal to zero.
        /// </exception>
        public GridSnapService(int cellWidth, int cellHeight)
        {
            if (cellWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
            }

            if (cellHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
            }

            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }

        /// <summary>
        /// Converts a raw pixel location into the nearest grid position.
        /// </summary>
        /// <param name="rawLocation">The raw pixel location to evaluate.</param>
        /// <returns>The nearest grid position for the supplied location.</returns>
        public GridPosition GetGridPosition(Point rawLocation)
        {
            int column = (int)Math.Round(rawLocation.X / (double)CellWidth, MidpointRounding.AwayFromZero);
            int row = (int)Math.Round(rawLocation.Y / (double)CellHeight, MidpointRounding.AwayFromZero);

            return new GridPosition(column, row);
        }

        /// <summary>
        /// Converts a grid position into a snapped pixel location.
        /// </summary>
        /// <param name="gridPosition">The grid position to convert.</param>
        /// <returns>The snapped pixel location for the grid position.</returns>
        public Point GetSnappedLocation(GridPosition gridPosition)
        {
            return new Point(gridPosition.Column * CellWidth, gridPosition.Row * CellHeight);
        }

        /// <summary>
        /// Snaps a raw location to the nearest valid grid position within workspace bounds.
        /// </summary>
        /// <param name="rawLocation">The raw pixel location to snap.</param>
        /// <param name="blockSize">The size of the block being placed.</param>
        /// <param name="workspaceSize">The size of the workspace bounds.</param>
        /// <returns>
        /// A <see cref="SnappedPlacement"/> containing the snapped location and occupied grid position.
        /// </returns>
        public SnappedPlacement Snap(Point rawLocation, Size blockSize, Size workspaceSize)
        {
            int maxColumn = Math.Max(0, (workspaceSize.Width - blockSize.Width) / CellWidth);
            int maxRow = Math.Max(0, (workspaceSize.Height - blockSize.Height) / CellHeight);

            GridPosition rawGridPosition = GetGridPosition(rawLocation);
            GridPosition clampedGridPosition = new(
                Clamp(rawGridPosition.Column, 0, maxColumn),
                Clamp(rawGridPosition.Row, 0, maxRow));

            return new SnappedPlacement(GetSnappedLocation(clampedGridPosition), clampedGridPosition);
        }

        /// <summary>
        /// Restricts a value to a given minimum and maximum range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <returns>The clamped value.</returns>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
