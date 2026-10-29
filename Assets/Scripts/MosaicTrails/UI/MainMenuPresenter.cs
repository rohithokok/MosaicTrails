using UnityEngine;
using UnityEngine.UI;

public static class MainMenuPresenter
{
    public static void Build(Transform root, PuzzleLevelDefinition[] levels, int unlockedThrough,
        System.Action<int> onChooseLevel)
    {
        bool wideLayout = Screen.width > Screen.height;
        PuzzleUIFactory.AddBackdrop(root);
        PuzzleUIFactory.CreateText(root, "Eyebrow", "A LITTLE PUZZLE JOURNEY", 22, PuzzleUIFactory.Amber,
            TextAnchor.MiddleCenter, new Vector2(0.5f, wideLayout ? 0.94f : 0.91f),
            new Vector2(0.5f, wideLayout ? 0.94f : 0.91f), Vector2.zero,
            new Vector2(720f, 52f), FontStyle.Bold);
        float titleY = wideLayout ? 0.81f : 0.79f;
        PuzzleUIFactory.CreateText(root, "Game Title", "MOSAIC\nTRAILS", wideLayout ? 70 : 94, PuzzleUIFactory.Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, titleY), new Vector2(0.5f, titleY), Vector2.zero,
            new Vector2(840f, wideLayout ? 185f : 245f), FontStyle.Bold);
        float subtitleY = wideLayout ? 0.66f : 0.69f;
        PuzzleUIFactory.CreateText(root, "Game Subtitle", "Gather the little pieces. Find the bigger picture.", wideLayout ? 22 : 27,
            new Color(0.77f, 0.87f, 0.81f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, subtitleY), new Vector2(0.5f, subtitleY), Vector2.zero,
            new Vector2(900f, 66f));

        PuzzleUIFactory.CreateText(root, "Map Heading", "CHOOSE A POSTCARD", 21, PuzzleUIFactory.Amber,
            TextAnchor.MiddleLeft, new Vector2(0.1f, wideLayout ? 0.575f : 0.625f),
            new Vector2(0.9f, wideLayout ? 0.575f : 0.625f), Vector2.zero,
            new Vector2(0f, 45f), FontStyle.Bold);

        float[] yPositions = wideLayout
            ? new[] { 0.48f, 0.32f, 0.16f }
            : new[] { 0.535f, 0.395f, 0.255f };
        float cardHeight = wideLayout ? 142f : 206f;
        for (int i = 0; i < levels.Length; i++)
        {
            PuzzleLevelDefinition level = levels[i];
            if (level == null)
                continue;
            bool unlocked = i <= unlockedThrough;
            int capturedIndex = i;
            string title = "POSTCARD  " + (i + 1).ToString("00") + "\n" + level.levelTitle.ToUpperInvariant() +
                           "     •     " + level.PieceCount + " PIECES     •     " + level.difficulty.ToUpperInvariant();
            if (!unlocked)
                title += "\nCOMPLETE THE PREVIOUS POSTCARD TO UNLOCK";

            Button card = PuzzleUIFactory.CreateButton(root, "Level Card " + (i + 1), title,
                unlocked ? PuzzleUIFactory.Paper : new Color(0.46f, 0.55f, 0.52f),
                () => onChooseLevel?.Invoke(capturedIndex),
                new Vector2(0.08f, yPositions[i]), new Vector2(0.92f, yPositions[i]), Vector2.zero,
                new Vector2(0f, cardHeight), wideLayout ? 19 : 24,
                unlocked ? PuzzleUIFactory.Ink : new Color(0.88f, 0.90f, 0.84f));
            card.interactable = unlocked;
        }

        PuzzleUIFactory.CreateText(root, "Footer Note", "MADE FOR SLOW MOMENTS  ·  NO RUSH, JUST REVEAL", 16,
            new Color(0.62f, 0.75f, 0.70f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, wideLayout ? 0.045f : 0.075f),
            new Vector2(0.5f, wideLayout ? 0.045f : 0.075f), Vector2.zero,
            new Vector2(900f, 44f), FontStyle.Bold);
    }
}
