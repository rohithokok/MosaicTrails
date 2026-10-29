using UnityEngine;
using UnityEngine.UI;

public static class CompletionPresenter
{
    public static RectTransform Build(Transform parent, PuzzleLevelDefinition level, bool finalLevel,
        System.Action next, System.Action restart, System.Action menu)
    {
        Image shade = PuzzleUIFactory.CreatePanel(parent, "Completion Shade", new Color(0.015f, 0.07f, 0.09f, 0.78f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = shade.gameObject.AddComponent<CanvasGroup>();
        RectTransform panel = PuzzleUIFactory.CreateRect(shade.transform, "Completion Card",
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(850f, 790f));
        Image card = panel.gameObject.AddComponent<Image>();
        card.sprite = PuzzleUIFactory.CardSprite;
        card.type = Image.Type.Sliced;
        card.color = PuzzleUIFactory.Paper;
        Shadow shadow = panel.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
        shadow.effectDistance = new Vector2(0f, -14f);

        Image seal = PuzzleUIFactory.CreatePanel(panel, "Postcard Seal", PuzzleUIFactory.Coral,
            new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f), Vector2.zero,
            new Vector2(74f, 74f));
        seal.sprite = PuzzleUIFactory.CircleSprite;
        PuzzleUIFactory.CreateText(panel, "Seal Letter", "M", 42, PuzzleUIFactory.Paper, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(74f, 74f), FontStyle.Bold);
        PuzzleUIFactory.CreateText(panel, "Completion Title", "PICTURE COMPLETE", 39, PuzzleUIFactory.Ink,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.67f), new Vector2(0.5f, 0.67f), Vector2.zero,
            new Vector2(760f, 90f), FontStyle.Bold);
        PuzzleUIFactory.CreateText(panel, "Completion Detail", level.levelTitle + " is back in one piece.\nTake a little look around.",
            25, new Color(0.24f, 0.37f, 0.36f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), Vector2.zero,
            new Vector2(700f, 110f));

        string nextLabel = finalLevel ? "BACK TO POSTCARDS" : "NEXT POSTCARD";
        PuzzleUIFactory.CreateButton(panel, "Next Button", nextLabel, PuzzleUIFactory.Amber, next,
            new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero,
            new Vector2(600f, 96f), 24);
        PuzzleUIFactory.CreateButton(panel, "Replay Button", "PLAY THIS ONE AGAIN", new Color(0.88f, 0.92f, 0.83f), restart,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero,
            new Vector2(600f, 86f), 21);
        PuzzleUIFactory.CreateButton(panel, "Map Button", "RETURN TO MAP", new Color(0.88f, 0.92f, 0.83f), menu,
            new Vector2(0.5f, 0.095f), new Vector2(0.5f, 0.095f), Vector2.zero,
            new Vector2(600f, 86f), 21);

        group.alpha = 0f;
        panel.localScale = Vector3.one * 0.88f;
        return panel;
    }
}
