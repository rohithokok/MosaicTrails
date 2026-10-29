using System;
using UnityEngine;
using UnityEngine.UI;

public static class PuzzleUIFactory
{
    public static readonly Color Ink = new Color(0.055f, 0.15f, 0.19f);
    public static readonly Color Deep = new Color(0.035f, 0.12f, 0.16f);
    public static readonly Color Cream = new Color(0.98f, 0.92f, 0.78f);
    public static readonly Color Paper = new Color(1f, 0.965f, 0.88f);
    public static readonly Color Teal = new Color(0.24f, 0.61f, 0.56f);
    public static readonly Color Amber = new Color(0.98f, 0.69f, 0.34f);
    public static readonly Color Coral = new Color(0.91f, 0.40f, 0.34f);

    private static Sprite _cardSprite;
    private static Sprite _circleSprite;
    private static Font _font;

    public static Sprite CardSprite
    {
        get
        {
            if (_cardSprite == null)
                _cardSprite = MakeRoundedSprite();
            return _cardSprite;
        }
    }

    public static Sprite CircleSprite
    {
        get
        {
            if (_circleSprite == null)
                _circleSprite = MakeCircleSprite();
            return _circleSprite;
        }
    }

    public static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, bool rounded = false)
    {
        RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        if (rounded)
        {
            image.sprite = CardSprite;
            image.type = Image.Type.Sliced;
        }
        return image;
    }

    public static Text CreateText(Transform parent, string name, string value, int fontSize, Color color,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size,
        FontStyle fontStyle = FontStyle.Normal)
    {
        RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, size);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label, Color color,
        Action clicked, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size,
        int fontSize = 31, Color? textColor = null)
    {
        Image background = CreatePanel(parent, name, color, anchorMin, anchorMax, anchoredPosition, size, true);
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => clicked?.Invoke());
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(color, Ink, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(color.r, color.g, color.b, 0.48f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        Text text = CreateText(background.transform, "Label", label, fontSize, textColor ?? Ink,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
        RectTransform textRect = (RectTransform)text.transform;
        textRect.offsetMin = new Vector2(20f, 8f);
        textRect.offsetMax = new Vector2(-20f, -8f);
        Shadow shadow = background.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.2f);
        shadow.effectDistance = new Vector2(0f, -5f);
        return button;
    }

    public static void AddBackdrop(Transform parent)
    {
        CreatePanel(parent, "Night Teal Backdrop", Deep, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image halo = CreatePanel(parent, "Soft Amber Halo", new Color(0.88f, 0.49f, 0.27f, 0.17f),
            new Vector2(0.78f, 0.82f), new Vector2(0.78f, 0.82f), Vector2.zero, new Vector2(560f, 560f));
        halo.sprite = CircleSprite;
        Image glow = CreatePanel(parent, "Soft Teal Glow", new Color(0.17f, 0.57f, 0.55f, 0.16f),
            new Vector2(0.16f, 0.22f), new Vector2(0.16f, 0.22f), Vector2.zero, new Vector2(500f, 500f));
        glow.sprite = CircleSprite;
    }

    private static Font Font
    {
        get
        {
            if (_font != null)
                return _font;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }

    private static Sprite MakeRoundedSprite()
    {
        const int size = 64;
        const float radius = 15f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Mosaic Trails Rounded UI Shape",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = x < radius ? radius : x >= size - radius ? size - radius - 1 : x;
                float cy = y < radius ? radius : y >= size - radius ? size - radius - 1 : y;
                float dx = x - cx;
                float dy = y - cy;
                pixels[y * size + x] = dx * dx + dy * dy <= radius * radius ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(18f, 18f, 18f, 18f));
    }

    private static Sprite MakeCircleSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Mosaic Trails Circle UI Shape",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.48f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - center;
                pixels[y * size + x] = p.sqrMagnitude <= radius * radius ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
