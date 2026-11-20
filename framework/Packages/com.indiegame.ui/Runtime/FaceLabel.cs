using UnityEngine;

namespace GameKit.UI
{
    public static class FaceLabel
    {
        public const float FaceCentreFromTop = 0.41f;

        public static void CentreOnFace(RectTransform label, float buttonHeight)
            => CentreOnFace(label, buttonHeight, FaceCentreFromTop);

        public static void CentreOnFace(RectTransform label, float buttonHeight, float centreFromTop)
        {
            float labelHeight = label.rect.height;
            label.anchorMin = new Vector2(label.anchorMin.x, 1f);
            label.anchorMax = new Vector2(label.anchorMax.x, 1f);
            label.pivot = new Vector2(label.pivot.x, 0.5f);
            label.sizeDelta = new Vector2(label.sizeDelta.x, labelHeight);
            label.anchoredPosition = new Vector2(label.anchoredPosition.x, -buttonHeight * centreFromTop);
        }
    }
}
