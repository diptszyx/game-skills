using System;
using TMPro;
using UnityEngine;

namespace GameKit.UI
{
    [CreateAssetMenu(menuName = "GameKit/UI Theme")]
    public sealed class UiTheme : ScriptableObject
    {
        public TMP_FontAsset Font;
        public Material BodyMaterial;
        public Material TitleMaterial;
        public Color BodyColor = new Color32(30, 58, 138, 255);
        public bool CurvedTitle = true;
        public float TitleCurveRadius = 810f;
        public float TitleOffsetY = -54f;
        // Modal geometry is theme data so existing authored layouts survive migration.
        public float TitleFontSize = 46f;
        public Vector2 TitleSize = new Vector2(420f, 100f);
        public float BodyFontSize = 36f;
        public Vector2 BodySize = new Vector2(500f, 120f);
        public float BodyOffsetY = 40f;
        public float ButtonRowOffsetY = -150f;
        public float TwoButtonOffsetX = 145f;
        public float PanelPixelsPerUnitMultiplier = 1f;
        public Sprite PanelSprite;
        public Vector2 PanelSize = new Vector2(620f, 520f);
        public Sprite CloseSprite;
        public Vector2 CloseSize = new Vector2(84f, 84f);
        public Vector2 CloseOffset = new Vector2(262f, 262f);
        public Sprite ToggleOn;
        public Sprite ToggleOff;
        public UiButtonStyle Primary = new UiButtonStyle();
        public UiButtonStyle Secondary = new UiButtonStyle();
        public UiButtonStyle Neutral = new UiButtonStyle();
        public UiMotion Motion = new UiMotion();
        public Color ScrimColor = new Color(0f, 0.04f, 0.12f, 0.72f);
    }

    [Serializable]
    public sealed class UiMotion
    {
        public float OpenSeconds = 0.22f;
        public float ScrimFadeSeconds = 0.15f;
        public float OpenStartScale = 0.86f;
        public float OpenOvershoot = 1.04f;
        public float OvershootAt = 0.65f;
        public float CloseSeconds = 0.12f;
        public float CloseEndScale = 0.92f;
    }
}
