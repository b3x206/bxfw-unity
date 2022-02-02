using UnityEngine;

public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}

/// <summary>
/// Extensions for class <see cref="RectTransform"/>.
/// </summary>
public static class RectTransformExtensions
{
    public static void SetAnchor(this RectTransform source, AnchorPresets align, 
        bool setPosition = false, float offsetX = 0f, float offsetY = 0f)
    {
        if (setPosition)
        {
            source.anchoredPosition = new Vector3(offsetX, offsetY, 0);
        }

        switch (align)
        {
            case AnchorPresets.TopLeft:
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
            case AnchorPresets.TopCenter:
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
            case AnchorPresets.TopRight:
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;

            case AnchorPresets.MiddleLeft:
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
            case AnchorPresets.MiddleCenter:
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
            case AnchorPresets.MiddleRight:
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;

            case AnchorPresets.BottomLeft:
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
            case AnchorPresets.BottomCenter:
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
            case AnchorPresets.BottomRight:
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;

            case AnchorPresets.HorStretchTop:
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
            case AnchorPresets.HorStretchMiddle:
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
            case AnchorPresets.HorStretchBottom:
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;

            case AnchorPresets.VertStretchLeft:
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
            case AnchorPresets.VertStretchCenter:
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
            case AnchorPresets.VertStretchRight:
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;

            case AnchorPresets.StretchAll:
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    source.offsetMin = Vector2.zero;
                    source.offsetMax = Vector2.zero;
                    break;
        }
    }

    public static void SetPivot(this RectTransform source, PivotPresets preset)
    {
        switch (preset)
        {
            case PivotPresets.TopLeft:
                    source.pivot = new Vector2(0, 1);
                    break;
            case PivotPresets.TopCenter:
                    source.pivot = new Vector2(0.5f, 1);
                    break;
            case PivotPresets.TopRight:
                    source.pivot = new Vector2(1, 1);
                    break;

            case PivotPresets.MiddleLeft:
                    source.pivot = new Vector2(0, 0.5f);
                    break;
            case PivotPresets.MiddleCenter:
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
            case PivotPresets.MiddleRight:
                    source.pivot = new Vector2(1, 0.5f);
                    break;

            case PivotPresets.BottomLeft:
                    source.pivot = new Vector2(0, 0);
                    break;
            case PivotPresets.BottomCenter:
                    source.pivot = new Vector2(0.5f, 0);
                    break;
            case PivotPresets.BottomRight:
                    source.pivot = new Vector2(1, 0);
                    break;
        }
    }
}