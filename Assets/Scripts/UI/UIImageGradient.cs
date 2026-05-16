using UnityEngine;
using UnityEngine.UI;

namespace BaslangicSeviye.SayiTahminOyunu.UI
{
    /// <summary>
    /// UGUI Graphic vertex renklerini kullanarak basit gradient efekti uygular.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class UIImageGradient : BaseMeshEffect
    {
        [SerializeField] private Color topColor = new Color32(30, 41, 59, 255);
        [SerializeField] private Color bottomColor = new Color32(15, 23, 42, 255);
        [SerializeField] private bool horizontal;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            UIVertex vertex = default;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                min = Vector2.Min(min, vertex.position);
                max = Vector2.Max(max, vertex.position);
            }

            float range = horizontal ? max.x - min.x : max.y - min.y;
            if (Mathf.Approximately(range, 0f))
            {
                return;
            }

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                float t = horizontal
                    ? Mathf.InverseLerp(min.x, max.x, vertex.position.x)
                    : Mathf.InverseLerp(min.y, max.y, vertex.position.y);
                vertex.color *= Color.Lerp(bottomColor, topColor, t);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}
