using System.Drawing;
using COMP_3951_BlockForge_TechPro;

namespace BlockForge.TechPro.Tests.SnapGrid;

/// <summary>
/// BlockForge GridSnapServiceTests
/// Author: Andre Di Lascio
/// Date: Mar 24 2026
/// Source: Written with the help of AI.
/// </summary>
[TestClass]
public sealed class GridSnapServiceTests
{
    private readonly GridSnapService _service = new(40, 40);

    [TestMethod]
    public void GetGridPosition_RawCoordinates_MapToExpectedGridCell()
    {
        GridPosition result = _service.GetGridPosition(new Point(78, 81));

        Assert.AreEqual(new GridPosition(2, 2), result);
    }

    [TestMethod]
    public void GetSnappedLocation_GridPosition_ReturnsSnappedPixelCoordinates()
    {
        Point result = _service.GetSnappedLocation(new GridPosition(3, 4));

        Assert.AreEqual(new Point(120, 160), result);
    }

    [TestMethod]
    public void Snap_ValidPoint_ReturnsMatchingGridAndPixelPosition()
    {
        SnappedPlacement result = _service.Snap(
            new Point(78, 81),
            new Size(70, 60),
            new Size(400, 300));

        Assert.AreEqual(new GridPosition(2, 2), result.GridPosition);
        Assert.AreEqual(new Point(80, 80), result.Location);
    }

    [TestMethod]
    public void Snap_BlockMovedToNewLocation_UpdatesStoredGridPosition()
    {
        var block = new CodeBlock(0, 0, "block-1", 0, 0);
        SnappedPlacement result = _service.Snap(
            new Point(121, 39),
            new Size(70, 60),
            new Size(400, 300));

        block.UpdatePosition(result.Location.X, result.Location.Y);
        block.UpdateGridPosition(result.GridPosition.Column, result.GridPosition.Row);

        Assert.AreEqual(120d, block.PosX);
        Assert.AreEqual(40d, block.PosY);
        Assert.AreEqual(3, block.GridColumn);
        Assert.AreEqual(1, block.GridRow);
    }

    [TestMethod]
    public void Snap_OutOfBoundsDrop_ClampsToLastValidCell()
    {
        SnappedPlacement result = _service.Snap(
            new Point(999, 999),
            new Size(70, 60),
            new Size(400, 300));

        Assert.AreEqual(new GridPosition(8, 6), result.GridPosition);
        Assert.AreEqual(new Point(320, 240), result.Location);
    }

    [TestMethod]
    public void Snap_NegativeCoordinates_ClampToOrigin()
    {
        SnappedPlacement result = _service.Snap(
            new Point(-15, -20),
            new Size(70, 60),
            new Size(400, 300));

        Assert.AreEqual(new GridPosition(0, 0), result.GridPosition);
        Assert.AreEqual(new Point(0, 0), result.Location);
    }

    [DataTestMethod]
    [DataRow(40, 40, 1, 1)]
    [DataRow(39, 39, 1, 1)]
    [DataRow(20, 20, 1, 1)]
    [DataRow(19, 19, 0, 0)]
    public void GetGridPosition_BoundaryConditions_OnGridLinesBehaveAsExpected(
        int x,
        int y,
        int expectedColumn,
        int expectedRow)
    {
        GridPosition result = _service.GetGridPosition(new Point(x, y));

        Assert.AreEqual(new GridPosition(expectedColumn, expectedRow), result);
    }

    [TestMethod]
    public void Serialize_BlockWithSnappedState_PreservesSnappedPositionAndGrid()
    {
        var block = new CodeBlock(80, 120, "block-1", 2, 3);

        string json = CodeBlockSerializer.Serialize(block);

        StringAssert.Contains(json, "\"PosX\": 80");
        StringAssert.Contains(json, "\"PosY\": 120");
        StringAssert.Contains(json, "\"GridColumn\": 2");
        StringAssert.Contains(json, "\"GridRow\": 3");
    }

    [DataTestMethod]
    [DataRow(0, 40)]
    [DataRow(40, 0)]
    [DataRow(-1, 40)]
    [DataRow(40, -1)]
    public void Constructor_InvalidCellDimensions_ThrowsArgumentOutOfRangeException(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridSnapService(width, height));
    }
}
