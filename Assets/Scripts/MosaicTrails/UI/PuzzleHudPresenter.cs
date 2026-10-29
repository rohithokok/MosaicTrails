using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PuzzleHudPresenter
{
    private readonly Text _hintStatus;
    private readonly Text _progressText;
    public readonly RectTransform BoardRect;

    private PuzzleHudPresenter(RectTransform boardRect, Text progressText, Text hintStatus)
    {
        BoardRect = boardRect;
        _progressText = progressText;
        _hintStatus = hintStatus;
    }

    public static PuzzleHudPresenter Build(Transform root, PuzzleLevelDefinition level,
        Action hintPressed, Action restartPressed, Action menuPressed)
    {
        PuzzleUIFactory.AddBackdrop(root);
        PuzzleUIFactory.CreateText(root, "Journey Label", "MOSAIC TRAILS     /     " + level.difficulty.ToUpperInvariant(),
            18, PuzzleUIFactory.Amber, TextAnchor.MiddleLeft,
            new Vector2(0.08f, 0.94f), new Vector2(0.92f, 0.94f), Vector2.zero,
            new Vector2(0f, 42f), FontStyle.Bold);
        PuzzleUIFactory.CreateText(root, "Level Title", level.levelTitle, 46, PuzzleUIFactory.Cream,
            TextAnchor.MiddleLeft, new Vector2(0.08f, 0.885f), new Vector2(0.92f, 0.885f), Vector2.zero,
            new Vector2(0f, 74f), FontStyle.Bold);

        Button back = PuzzleUIFactory.CreateButton(root, "Map Button", "MAP", new Color(0.16f, 0.32f, 0.33f),
            menuPressed, new Vector2(0.08f, 0.815f), new Vector2(0.34f, 0.815f), Vector2.zero,
            new Vector2(0f, 73f), 21, PuzzleUIFactory.Cream);
        Button restart = PuzzleUIFactory.CreateButton(root, "Restart Button", "START OVER", new Color(0.16f, 0.32f, 0.33f),
            restartPressed, new Vector2(0.65f, 0.815f), new Vector2(0.92f, 0.815f), Vector2.zero,
            new Vector2(0f, 73f), 20, PuzzleUIFactory.Cream);

        Image boardCard = PuzzleUIFactory.CreatePanel(root, "Open Play Space", new Color(0.10f, 0.24f, 0.27f, 0.76f),
            new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.77f), Vector2.zero, Vector2.zero, true);
        RectTransform boardRect = (RectTransform)boardCard.transform;
        boardRect.offsetMin = new Vector2(0f, 0f);
        boardRect.offsetMax = new Vector2(0f, 0f);

        Text progress = PuzzleUIFactory.CreateText(root, "Join Progress", "0 / " + level.PieceCount + " PIECES IN CONNECTED GROUPS",
            19, new Color(0.78f, 0.88f, 0.82f), TextAnchor.MiddleLeft,
            new Vector2(0.08f, 0.145f), new Vector2(0.65f, 0.145f), Vector2.zero,
            new Vector2(0f, 48f), FontStyle.Bold);
        PuzzleUIFactory.CreateButton(root, "Hint Button", "HINT", PuzzleUIFactory.Amber,
            hintPressed, new Vector2(0.69f, 0.115f), new Vector2(0.92f, 0.115f), Vector2.zero,
            new Vector2(0f, 89f), 23);
        Text hintStatus = PuzzleUIFactory.CreateText(root, "Hint Status", "", 18, PuzzleUIFactory.Cream,
            TextAnchor.MiddleCenter, new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.07f), Vector2.zero,
            new Vector2(0f, 45f));

        return new PuzzleHudPresenter(boardRect, progress, hintStatus);
    }

    public void SetProgress(int joined, int total)
    {
        _progressText.text = joined + " / " + total + " PIECES IN CONNECTED GROUPS";
    }

    public void SetHintStatus(string message)
    {
        _hintStatus.text = message;
    }
}
