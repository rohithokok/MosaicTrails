using UnityEngine;

public static class PuzzleConnectionRules
{
    public static bool AreNeighbors(PuzzlePieceView a, PuzzlePieceView b)
    {
        return Mathf.Abs(a.Column - b.Column) + Mathf.Abs(a.Row - b.Row) == 1;
    }

    public static Vector2 ExpectedOffset(PuzzlePieceView a, PuzzlePieceView b, float tileWidth, float tileHeight)
    {
        return new Vector2((b.Column - a.Column) * tileWidth, (a.Row - b.Row) * tileHeight);
    }

    public static bool TryGetCorrection(
        PuzzlePieceView movingPiece,
        PuzzlePieceView stationaryPiece,
        float tileWidth,
        float tileHeight,
        float toleranceFraction,
        out Vector2 correction,
        out float errorSquared)
    {
        correction = Vector2.zero;
        errorSquared = float.MaxValue;

        if (!AreNeighbors(movingPiece, stationaryPiece))
            return false;

        Vector2 expected = ExpectedOffset(movingPiece, stationaryPiece, tileWidth, tileHeight);
        Vector2 actual = stationaryPiece.AnchoredPosition - movingPiece.AnchoredPosition;
        Vector2 error = actual - expected;
        float toleranceX = tileWidth * toleranceFraction;
        float toleranceY = tileHeight * toleranceFraction;

        // Both the edge gap and perpendicular edge alignment must fit within 18% of a tile.
        if (Mathf.Abs(error.x) > toleranceX || Mathf.Abs(error.y) > toleranceY)
            return false;

        correction = error;
        errorSquared = error.sqrMagnitude;
        return true;
    }
}
