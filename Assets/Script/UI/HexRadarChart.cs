using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Script.UI
{
    /// <summary>
    /// Dessine un radar chart hexagonal (6 stats) sur un Canvas.
    /// Supporte l'affichage d'une créature (plein) et d'une zone de prédiction (semi-transparent).
    /// GDD §4.5 — Hexagone Parent A (bleu), Parent B (rouge), Prédiction enfant (vert).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class HexRadarChart : Graphic
    {
        [Header("Configuration")]
        public float maxStatValue = 20f;
        public float radius = 100f;
        [SerializeField] private float lineThickness = 2f;

        [Header("Couleurs")]
        [SerializeField] private Color shapeFillColor = new Color(0.2f, 0.5f, 1f, 0.6f);
        [SerializeField] private Color shapeOutlineColor = new Color(0.2f, 0.5f, 1f, 1f);
        [SerializeField] private Color predictionFillColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        [SerializeField] private Color predictionOutlineColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);

        public void SetColors(Color fill, Color outline)
        {
            shapeFillColor = fill;
            shapeOutlineColor = outline;
            SetVerticesDirty();
        }

        [Header("Labels")]
        [SerializeField] private string[] statLabels = new string[]
        {
            "FOR", "AGI", "INT", "CHA", "CON", "VOL"
        };

        // Données à afficher
        private float[] _currentStats;
        private float[] _predictionMin;
        private float[] _predictionMax;
        private bool _showPrediction;

        // Vertices internes
        private readonly List<UIVertex> _verts = new List<UIVertex>();
        private readonly List<int> _indices = new List<int>();

        /// <summary>Définit les stats actuelles à dessiner (plein).</summary>
        public void SetStats(float[] stats)
        {
            _currentStats = stats != null && stats.Length >= 6 ? stats : null;
            SetVerticesDirty();
        }

        /// <summary>Définit la zone de prédiction min/max (verte semi-transparente).</summary>
        public void SetPrediction(float[] minStats, float[] maxStats)
        {
            _predictionMin = minStats != null && minStats.Length >= 6 ? minStats : null;
            _predictionMax = maxStats != null && maxStats.Length >= 6 ? maxStats : null;
            _showPrediction = _predictionMin != null && _predictionMax != null;
            SetVerticesDirty();
        }

        /// <summary>Efface la prédiction.</summary>
        public void ClearPrediction()
        {
            _showPrediction = false;
            _predictionMin = null;
            _predictionMax = null;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            _verts.Clear();
            _indices.Clear();

            Vector2 center = rectTransform.rect.center;

            // Grille de fond (concentrique)
            DrawGrid(center, vh);

            // Prédiction (derrière)
            if (_showPrediction)
                DrawPredictionShape(center, vh);

            // Stats actuelles (devant)
            if (_currentStats != null)
                DrawStatsShape(center, vh);
        }

        private void DrawGrid(Vector2 center, VertexHelper vh)
        {
            int rings = 4;
            for (int r = 1; r <= rings; r++)
            {
                float t = r / (float)rings;
                float ringRadius = radius * t;
                for (int i = 0; i < 6; i++)
                {
                    float angle = (i * 60f - 90f) * Mathf.Deg2Rad;
                    Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius;

                    float nextAngle = ((i + 1) * 60f - 90f) * Mathf.Deg2Rad;
                    Vector2 nextP = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * ringRadius;

                    AddLine(p, nextP, gridColor, 1f, vh);

                    // Rayons
                    if (r == rings)
                    {
                        Vector2 inner = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (ringRadius * 0.05f);
                        AddLine(center, p, gridColor, 1f, vh);
                    }
                }
            }
        }

        private void DrawStatsShape(Vector2 center, VertexHelper vh)
        {
            if (_currentStats == null) return;

            var points = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float t = Mathf.Clamp01(_currentStats[i] / maxStatValue);
                float angle = (i * 60f - 90f) * Mathf.Deg2Rad;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * t);
            }

            // Fill
            int startIdx = vh.currentVertCount;
            AddVertex(center, shapeFillColor, vh);
            for (int i = 0; i < 6; i++)
                AddVertex(points[i], shapeFillColor, vh);

            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                vh.AddTriangle(startIdx, startIdx + 1 + i, startIdx + 1 + next);
            }

            // Outline
            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                AddLine(points[i], points[next], shapeOutlineColor, lineThickness, vh);
            }
        }

        private void DrawPredictionShape(Vector2 center, VertexHelper vh)
        {
            if (_predictionMin == null || _predictionMax == null) return;

            var minPoints = new Vector2[6];
            var maxPoints = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float tMin = Mathf.Clamp01(_predictionMin[i] / maxStatValue);
                float tMax = Mathf.Clamp01(_predictionMax[i] / maxStatValue);
                float angle = (i * 60f - 90f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                minPoints[i] = center + dir * (radius * tMin);
                maxPoints[i] = center + dir * (radius * tMax);
            }

            // Fill entre min et max (forme "donut" sectorisé)
            int startIdx = vh.currentVertCount;
            for (int i = 0; i < 6; i++)
            {
                AddVertex(minPoints[i], predictionFillColor, vh);
                AddVertex(maxPoints[i], predictionFillColor, vh);
            }

            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                int minIdx = startIdx + i * 2;
                int maxIdx = startIdx + i * 2 + 1;
                int nextMin = startIdx + next * 2;
                int nextMax = startIdx + next * 2 + 1;

                vh.AddTriangle(minIdx, maxIdx, nextMax);
                vh.AddTriangle(minIdx, nextMax, nextMin);
            }

            // Outline min + max
            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                AddLine(minPoints[i], minPoints[next], predictionOutlineColor, lineThickness, vh);
                AddLine(maxPoints[i], maxPoints[next], predictionOutlineColor, lineThickness, vh);
            }
        }

        private void AddLine(Vector2 a, Vector2 b, Color color, float thickness, VertexHelper vh)
        {
            Vector2 dir = (b - a).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            int idx = vh.currentVertCount;
            AddVertex(a + perp, color, vh);
            AddVertex(a - perp, color, vh);
            AddVertex(b + perp, color, vh);
            AddVertex(b - perp, color, vh);

            vh.AddTriangle(idx, idx + 2, idx + 1);
            vh.AddTriangle(idx + 1, idx + 2, idx + 3);
        }

        private void AddVertex(Vector2 pos, Color color, VertexHelper vh)
        {
            UIVertex v = UIVertex.simpleVert;
            v.position = pos;
            v.color = color;
            vh.AddVert(v);
        }
    }
}
