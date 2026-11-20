using TMPro;
using UnityEngine;

namespace GameKit.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class CurvedText : MonoBehaviour
    {
        [SerializeField] private float _radius = 810f;
        private TMP_Text _text;
        private bool _applying;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            Apply();
        }

        private void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        private void OnTextChanged(Object changed) { if (changed == _text) Apply(); }

        private void Apply()
        {
            if (_applying || _text == null) return;
            _applying = true;
            _text.ForceMeshUpdate();
            TMP_TextInfo info = _text.textInfo;
            float radius = Mathf.Max(0.001f, Mathf.Abs(_radius));
            float centerX = _text.bounds.center.x;
            float halfWidth = _text.bounds.extents.x;
            float lift = radius * (1f - Mathf.Cos(Mathf.Asin(Mathf.Clamp(halfWidth / radius, -1f, 1f)))) * .5f;
            for (int index = 0; index < info.characterCount; index++)
            {
                TMP_CharacterInfo character = info.characterInfo[index];
                if (!character.isVisible) continue;
                Vector3[] vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                int first = character.vertexIndex;
                Vector3 middle = (vertices[first] + vertices[first + 2]) * .5f;
                float angle = Mathf.Asin(Mathf.Clamp((middle.x - centerX) / radius, -1f, 1f));
                Vector3 offset = new Vector3(0f, lift - radius * (1f - Mathf.Cos(angle)), 0f);
                Quaternion rotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
                for (int corner = 0; corner < 4; corner++) vertices[first + corner] = rotation * (vertices[first + corner] - middle) + middle + offset;
            }
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            _applying = false;
        }
    }
}
