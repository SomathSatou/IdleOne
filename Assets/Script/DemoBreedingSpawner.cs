using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.UI;

namespace Assets.Script
{
    /// <summary>
    /// Spawner autonome qui construit entièrement la scène de test breeding en runtime.
    /// Ajoutez ce script à un GameObject vide dans une scène vide et appuyez sur Play.
    /// GDD §4.5 — Page de test breeding : sélection parents, prédiction hexagonale, résultat.
    /// </summary>
    public class DemoBreedingSpawner : MonoBehaviour
    {
        [Header("Optional : assign existing canvas, or leave empty to auto-create")]
        [SerializeField] private Canvas targetCanvas;

        private void Start()
        {
            SetupCanvas();
            BuildUI();
            Debug.Log("[DemoBreedingSpawner] Breeding test UI built.");
        }

        private void SetupCanvas()
        {
            if (targetCanvas != null) return;

            var canvasGO = new GameObject("BreedingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasGO.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        private void BuildUI()
        {
            var root = targetCanvas.transform;

            // Background
            var bg = CreatePanel(root, "Background", Color.black);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;

            // Title
            var title = CreateText(root, "Title", "Breeding Test Lab", 36, TextAnchor.MiddleCenter);
            title.anchorMin = new Vector2(0f, 0.92f);
            title.anchorMax = new Vector2(1f, 1f);
            title.offsetMin = Vector2.zero;
            title.offsetMax = Vector2.zero;

            // === PARENT A ===
            var panelA = CreatePanel(root, "PanelParentA", new Color(0.1f, 0.1f, 0.2f, 0.8f));
            panelA.anchorMin = new Vector2(0.02f, 0.55f);
            panelA.anchorMax = new Vector2(0.32f, 0.90f);
            panelA.offsetMin = Vector2.zero;
            panelA.offsetMax = Vector2.zero;

            var nameA = CreateText(panelA, "NameA", "Parent A", 18, TextAnchor.UpperLeft);
            nameA.anchorMin = new Vector2(0.05f, 0.82f);
            nameA.anchorMax = new Vector2(0.95f, 0.96f);

            var chartA = CreateRadarChart(panelA, "ChartA", Color.blue, new Color(0.2f, 0.5f, 1f, 0.6f));
            chartA.anchorMin = new Vector2(0.05f, 0.35f);
            chartA.anchorMax = new Vector2(0.95f, 0.80f);
            chartA.offsetMin = Vector2.zero;
            chartA.offsetMax = Vector2.zero;

            var statsA = CreateText(panelA, "StatsA", "...", 13, TextAnchor.LowerLeft);
            statsA.anchorMin = new Vector2(0.05f, 0.02f);
            statsA.anchorMax = new Vector2(0.95f, 0.34f);

            // === PARENT B ===
            var panelB = CreatePanel(root, "PanelParentB", new Color(0.2f, 0.1f, 0.1f, 0.8f));
            panelB.anchorMin = new Vector2(0.36f, 0.55f);
            panelB.anchorMax = new Vector2(0.66f, 0.90f);
            panelB.offsetMin = Vector2.zero;
            panelB.offsetMax = Vector2.zero;

            var nameB = CreateText(panelB, "NameB", "Parent B", 18, TextAnchor.UpperLeft);
            nameB.anchorMin = new Vector2(0.05f, 0.82f);
            nameB.anchorMax = new Vector2(0.95f, 0.96f);

            var chartB = CreateRadarChart(panelB, "ChartB", Color.red, new Color(1f, 0.3f, 0.3f, 0.6f));
            chartB.anchorMin = new Vector2(0.05f, 0.35f);
            chartB.anchorMax = new Vector2(0.95f, 0.80f);
            chartB.offsetMin = Vector2.zero;
            chartB.offsetMax = Vector2.zero;

            var statsB = CreateText(panelB, "StatsB", "...", 13, TextAnchor.LowerLeft);
            statsB.anchorMin = new Vector2(0.05f, 0.02f);
            statsB.anchorMax = new Vector2(0.95f, 0.34f);

            // === PREDICTION ===
            var panelPred = CreatePanel(root, "PanelPrediction", new Color(0.1f, 0.2f, 0.1f, 0.8f));
            panelPred.anchorMin = new Vector2(0.70f, 0.55f);
            panelPred.anchorMax = new Vector2(0.98f, 0.90f);
            panelPred.offsetMin = Vector2.zero;
            panelPred.offsetMax = Vector2.zero;

            var namePred = CreateText(panelPred, "NamePred", "Prediction", 18, TextAnchor.UpperLeft);
            namePred.anchorMin = new Vector2(0.05f, 0.82f);
            namePred.anchorMax = new Vector2(0.95f, 0.96f);

            var chartPred = CreateRadarChart(panelPred, "ChartPred", Color.green, new Color(0.2f, 0.8f, 0.2f, 0.3f));
            chartPred.anchorMin = new Vector2(0.05f, 0.35f);
            chartPred.anchorMax = new Vector2(0.95f, 0.80f);
            chartPred.offsetMin = Vector2.zero;
            chartPred.offsetMax = Vector2.zero;

            var statsPred = CreateText(panelPred, "StatsPred", "...", 13, TextAnchor.LowerLeft);
            statsPred.anchorMin = new Vector2(0.05f, 0.02f);
            statsPred.anchorMax = new Vector2(0.95f, 0.34f);

            // === CONTROLS (mid bottom) ===
            var panelControls = CreatePanel(root, "PanelControls", new Color(0.15f, 0.15f, 0.15f, 0.9f));
            panelControls.anchorMin = new Vector2(0.02f, 0.35f);
            panelControls.anchorMax = new Vector2(0.66f, 0.52f);
            panelControls.offsetMin = Vector2.zero;
            panelControls.offsetMax = Vector2.zero;

            var btnRandom = CreateButton(panelControls, "BtnRandom", "Randomize Parents", new Vector2(0.02f, 0.55f), new Vector2(0.32f, 0.95f));
            var btnBreed = CreateButton(panelControls, "BtnBreed", "Breed !", new Vector2(0.35f, 0.55f), new Vector2(0.65f, 0.95f));
            var btnQuick = CreateButton(panelControls, "BtnQuick", "Quick x10", new Vector2(0.68f, 0.55f), new Vector2(0.98f, 0.95f));

            var varianceLabel = CreateText(panelControls, "VarianceLabel", "Variance:", 14, TextAnchor.MiddleLeft);
            varianceLabel.anchorMin = new Vector2(0.02f, 0.25f);
            varianceLabel.anchorMax = new Vector2(0.20f, 0.52f);

            var varianceInput = CreateInputField(panelControls, "VarianceInput", "0.25", new Vector2(0.22f, 0.25f), new Vector2(0.40f, 0.52f));

            var mutLabel = CreateText(panelControls, "MutLabel", "Mutation %:", 14, TextAnchor.MiddleLeft);
            mutLabel.anchorMin = new Vector2(0.45f, 0.25f);
            mutLabel.anchorMax = new Vector2(0.63f, 0.52f);

            var mutInput = CreateInputField(panelControls, "MutInput", "0.05", new Vector2(0.65f, 0.25f), new Vector2(0.83f, 0.52f));

            // Log text (bottom strip, below inputs)
            var logText = CreateText(panelControls, "LogText", "Ready.", 14, TextAnchor.MiddleCenter);
            logText.anchorMin = new Vector2(0.02f, 0.02f);
            logText.anchorMax = new Vector2(0.98f, 0.22f);

            // === CHILD RESULT ===
            var panelChild = CreatePanel(root, "PanelChild", new Color(0.2f, 0.15f, 0.25f, 0.9f));
            panelChild.anchorMin = new Vector2(0.70f, 0.10f);
            panelChild.anchorMax = new Vector2(0.98f, 0.52f);
            panelChild.offsetMin = Vector2.zero;
            panelChild.offsetMax = Vector2.zero;

            var nameChild = CreateText(panelChild, "NameChild", "Child (?)", 18, TextAnchor.UpperLeft);
            nameChild.anchorMin = new Vector2(0.05f, 0.82f);
            nameChild.anchorMax = new Vector2(0.95f, 0.96f);

            var chartChild = CreateRadarChart(panelChild, "ChartChild", Color.cyan, new Color(0.3f, 0.8f, 1f, 0.6f));
            chartChild.anchorMin = new Vector2(0.05f, 0.38f);
            chartChild.anchorMax = new Vector2(0.95f, 0.80f);
            chartChild.offsetMin = Vector2.zero;
            chartChild.offsetMax = Vector2.zero;

            var statsChild = CreateText(panelChild, "StatsChild", "---", 13, TextAnchor.LowerLeft);
            statsChild.anchorMin = new Vector2(0.05f, 0.22f);
            statsChild.anchorMax = new Vector2(0.95f, 0.37f);

            var compChild = CreateText(panelChild, "CompChild", "---", 13, TextAnchor.LowerLeft);
            compChild.anchorMin = new Vector2(0.05f, 0.08f);
            compChild.anchorMax = new Vector2(0.95f, 0.22f);

            var mutChild = CreateText(panelChild, "MutChild", "---", 13, TextAnchor.LowerLeft);
            mutChild.anchorMin = new Vector2(0.05f, 0.01f);
            mutChild.anchorMax = new Vector2(0.95f, 0.08f);

            // === BREEDING UI ===
            var uiGO = new GameObject("BreedingUI");
            uiGO.transform.SetParent(root, false);
            var breedingUI = uiGO.AddComponent<BreedingUI>();
            breedingUI.Initialize(
                nameA.GetComponent<TMP_Text>(), chartA.GetComponent<HexRadarChart>(), statsA.GetComponent<TMP_Text>(),
                nameB.GetComponent<TMP_Text>(), chartB.GetComponent<HexRadarChart>(), statsB.GetComponent<TMP_Text>(),
                chartPred.GetComponent<HexRadarChart>(), statsPred.GetComponent<TMP_Text>(),
                chartChild.GetComponent<HexRadarChart>(), nameChild.GetComponent<TMP_Text>(),
                statsChild.GetComponent<TMP_Text>(), compChild.GetComponent<TMP_Text>(), mutChild.GetComponent<TMP_Text>(),
                btnBreed.GetComponent<Button>(), btnRandom.GetComponent<Button>(), btnQuick.GetComponent<Button>(),
                varianceInput.GetComponent<TMP_InputField>(), mutInput.GetComponent<TMP_InputField>(),
                logText.GetComponent<TMP_Text>());
        }

        // ========== UI Factory ==========

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = ToTMP(alignment);
            tmp.color = Color.white;
            tmp.richText = true;
            return go.GetComponent<RectTransform>();
        }

        private TextAlignmentOptions ToTMP(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                default: return TextAlignmentOptions.Center;
            }
        }

        private RectTransform CreateRadarChart(Transform parent, string name, Color outline, Color fill)
        {
            var go = new GameObject(name, typeof(HexRadarChart));
            go.transform.SetParent(parent, false);
            var chart = go.GetComponent<HexRadarChart>();
            chart.SetColors(fill, outline);
            chart.maxStatValue = 20f;
            chart.radius = 80f;
            return go.GetComponent<RectTransform>();
        }

        private RectTransform CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.7f, 1f);

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.5f, 0.7f, 0.9f);
            btn.colors = colors;

            var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform CreateInputField(Transform parent, string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var textGO = new GameObject("Text", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var input = go.GetComponent<TMP_InputField>();
            input.textComponent = tmp;
            input.text = defaultText;
            input.contentType = TMP_InputField.ContentType.DecimalNumber;

            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4, 2);
            textRt.offsetMax = new Vector2(-4, -2);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }
    }
}
