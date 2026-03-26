using System.Drawing;

namespace COMP_3951_BlockForge_TechPro
{
    /// <summary>
    /// BlockForge SnappedPlacement
    /// Author: Andre Di Lascio
    /// Date: Mar 25 2026
    /// Source: Written with the help of AI.
    /// </summary>
    /// <param name="Location">The snapped pixel location of the block.</param>
    /// <param name="GridPosition">The grid position occupied by the block.</param>
    public readonly record struct SnappedPlacement(Point Location, GridPosition GridPosition);
}
