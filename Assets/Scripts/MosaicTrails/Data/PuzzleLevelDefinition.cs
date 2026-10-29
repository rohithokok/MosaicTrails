using UnityEngine;

public enum PuzzleArtworkTheme
{
    LanternGarden,
    CoralCove,
    CloudMarket
}

[CreateAssetMenu(fileName = "PuzzleLevel", menuName = "Mosaic Trails/Puzzle Level")]
public sealed class PuzzleLevelDefinition : ScriptableObject
{
    public string levelTitle = "Lantern Garden";
    public string difficulty = "Gentle";
    [Min(2)] public int columns = 3;
    [Min(2)] public int rows = 3;
    public PuzzleArtworkTheme artworkTheme;
    [TextArea] public string hintText = "Look for the picture edges that continue each other.";

    public int PieceCount => Mathf.Max(2, columns) * Mathf.Max(2, rows);
}
