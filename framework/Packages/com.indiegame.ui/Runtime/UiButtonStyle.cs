using System;
using UnityEngine;

namespace GameKit.UI
{
    [Serializable]
    public sealed class UiButtonStyle
    {
        public Sprite Enabled;
        public Sprite Locked;
        public Material LabelMaterial;
        public Color LabelColor = Color.white;
        public float FontSize = 34f;
        public float FaceCentreFromTop = 0.41f;
        public Vector2 DefaultSize = new Vector2(246f, 88f);
        public Vector2 LabelPadding = new Vector2(24f, 16f);
    }
}
