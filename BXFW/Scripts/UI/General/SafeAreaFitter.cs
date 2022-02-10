using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Resizes a <see cref="RectTransform"/> to fit <see cref="Screen.safeArea"/>.</summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private void Awake()
    {
        var rTransform = GetComponent<RectTransform>();
        var safeArea = Screen.safeArea;

        var anchorMin = safeArea.position;
        var anchorMax = anchorMin + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rTransform.anchorMin = anchorMin;
        rTransform.anchorMax = anchorMax;
    }
}
