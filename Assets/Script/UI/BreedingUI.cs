using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Script.Creatures;
using System.Collections.Generic;

namespace Assets.Script.UI
{
    /// <summary>
    /// UI complète de test pour le système de breeding.
    /// GDD §4.5 — Sélection Parent A/B, prédiction hexagonale, résultat enfant.
    /// </summary>
    public class BreedingUI : MonoBehaviour
    {
        [Header("Parents")]
        [SerializeField] private TMP_Text parentAName;
        [SerializeField] private HexRadarChart parentAChart;
        [SerializeField] private TMP_Text parentAStats;

        [SerializeField] private TMP_Text parentBName;
        [SerializeField] private HexRadarChart parentBChart;
        [SerializeField] private TMP_Text parentBStats;

        [Header("Prédiction")]
        [SerializeField] private HexRadarChart predictionChart;
        [SerializeField] private TMP_Text predictionText;

        [Header("Résultat")]
        [SerializeField] private HexRadarChart childChart;
        [SerializeField] private TMP_Text childName;
        [SerializeField] private TMP_Text childStats;
        [SerializeField] private TMP_Text childComponents;
        [SerializeField] private TMP_Text mutationText;

        [Header("Contrôles")]
        [SerializeField] private Button breedButton;
        [SerializeField] private Button randomizeButton;
        [SerializeField] private Button quickBreedButton;
        [SerializeField] private TMP_InputField varianceInput;
        [SerializeField] private TMP_InputField mutationChanceInput;
        [SerializeField] private TMP_Text logText;

        private Creature _parentA;
        private Creature _parentB;
        private BreedingSystem.BreedingPrediction _currentPrediction;

        /// <summary>
        /// Injection manuelle des références UI (utilisé quand la scène est construite programmatically).
        /// </summary>
        public void Initialize(
            TMP_Text pAName, HexRadarChart pAChart, TMP_Text pAStats,
            TMP_Text pBName, HexRadarChart pBChart, TMP_Text pBStats,
            HexRadarChart predChart, TMP_Text predText,
            HexRadarChart cChart, TMP_Text cName, TMP_Text cStats, TMP_Text cComp, TMP_Text cMut,
            Button btnBreed, Button btnRandom, Button btnQuick,
            TMP_InputField varInput, TMP_InputField mutInput, TMP_Text log)
        {
            parentAName = pAName; parentAChart = pAChart; parentAStats = pAStats;
            parentBName = pBName; parentBChart = pBChart; parentBStats = pBStats;
            predictionChart = predChart; predictionText = predText;
            childChart = cChart; childName = cName; childStats = cStats;
            childComponents = cComp; mutationText = cMut;
            breedButton = btnBreed; randomizeButton = btnRandom; quickBreedButton = btnQuick;
            varianceInput = varInput; mutationChanceInput = mutInput; logText = log;
        }

        private void Start()
        {
            if (breedButton != null)
                breedButton.onClick.AddListener(OnBreedClicked);
            if (randomizeButton != null)
                randomizeButton.onClick.AddListener(OnRandomizeClicked);
            if (quickBreedButton != null)
                quickBreedButton.onClick.AddListener(OnQuickBreedClicked);

            OnRandomizeClicked();
        }

        private void OnDestroy()
        {
            if (breedButton != null)
                breedButton.onClick.RemoveListener(OnBreedClicked);
            if (randomizeButton != null)
                randomizeButton.onClick.RemoveListener(OnRandomizeClicked);
            if (quickBreedButton != null)
                quickBreedButton.onClick.RemoveListener(OnQuickBreedClicked);
        }

        /// <summary>Génère deux parents aléatoires et affiche la prédiction.</summary>
        private void OnRandomizeClicked()
        {
            _parentA = GenerateRandomCreature("Parent A", 0);
            _parentB = GenerateRandomCreature("Parent B", 0);
            RefreshParents();
            ComputePrediction();
            ClearResult();
            Log("Parents randomisés.");
        }

        /// <summary>Exécute un breeding et affiche l'enfant.</summary>
        private void OnBreedClicked()
        {
            if (_parentA == null || _parentB == null) return;

            float variance = ParseFloat(varianceInput, BreedingSystem.DefaultVarianceFactor);
            float mutationChance = ParseFloat(mutationChanceInput, BreedingSystem.DefaultMutationChance);

            var result = BreedingSystem.Breed(_parentA, _parentB,
                varianceFactor: variance,
                mutationChance: mutationChance);

            ShowResult(result);
            Log($"Breed réussi ! Enfant Gen-{result.Child.Generation}, Mutated={result.Mutated}");
        }

        /// <summary>10 breedings rapides pour tester la variance.</summary>
        private void OnQuickBreedClicked()
        {
            if (_parentA == null || _parentB == null) return;

            float variance = ParseFloat(varianceInput, BreedingSystem.DefaultVarianceFactor);
            float mutationChance = ParseFloat(mutationChanceInput, BreedingSystem.DefaultMutationChance);

            float bestTotal = -999f;
            BreedingSystem.BreedingResult best = null;
            var totals = new List<float>();

            for (int i = 0; i < 10; i++)
            {
                var result = BreedingSystem.Breed(_parentA, _parentB,
                    varianceFactor: variance,
                    mutationChance: mutationChance);
                totals.Add(result.Child.TotalStats);
                if (result.Child.TotalStats > bestTotal)
                {
                    bestTotal = result.Child.TotalStats;
                    best = result;
                }
            }

            ShowResult(best);
            Log($"10 breeds — meilleur enfant : Total={bestTotal:F1} | Moyenne={Average(totals):F1}");
        }

        private void RefreshParents()
        {
            if (_parentA != null)
            {
                parentAName.text = $"{_parentA.Name} (Gen-{_parentA.Generation})";
                parentAChart.SetStats(_parentA.GetStatsArray());
                parentAStats.text = FormatStats(_parentA);
            }

            if (_parentB != null)
            {
                parentBName.text = $"{_parentB.Name} (Gen-{_parentB.Generation})";
                parentBChart.SetStats(_parentB.GetStatsArray());
                parentBStats.text = FormatStats(_parentB);
            }
        }

        private void ComputePrediction()
        {
            if (_parentA == null || _parentB == null) return;

            float variance = ParseFloat(varianceInput, BreedingSystem.DefaultVarianceFactor);
            _currentPrediction = BreedingSystem.Predict(_parentA, _parentB, variance);

            predictionChart.SetStats(null);
            predictionChart.SetPrediction(_currentPrediction.MinStats, _currentPrediction.MaxStats);

            predictionText.text =
                $"<b>Prédiction</b>\n" +
                $"Squelette : {_currentPrediction.PredictedSkeleton}\n" +
                $"Forme : {_currentPrediction.PredictedShape}\n" +
                $"Couleur : {_currentPrediction.PredictedColor}\n" +
                $"\n<b>Fourchettes</b>\n" +
                FormatMinMax(_currentPrediction.MinStats, _currentPrediction.MaxStats);
        }

        private void ShowResult(BreedingSystem.BreedingResult result)
        {
            if (result == null) return;

            childName.text = $"{result.Child.Name} (Gen-{result.Child.Generation})";
            childChart.SetStats(result.Child.GetStatsArray());
            childChart.SetPrediction(result.MinStats, result.MaxStats);
            childStats.text = FormatStats(result.Child);
            childComponents.text = $"Squelette: {result.Child.Skeleton}\nForme: {result.Child.Shape}\nCouleur: {result.Child.Color}";
            mutationText.text = result.Mutated
                ? $"<color=red><b>MUTATION !</b></color>\n{result.MutationDescription}"
                : "Pas de mutation";
        }

        private void ClearResult()
        {
            childName.text = "Enfant (?)";
            childChart.SetStats(null);
            childChart.ClearPrediction();
            childStats.text = "---";
            childComponents.text = "---";
            mutationText.text = "---";
        }

        private void Log(string msg)
        {
            if (logText != null)
                logText.text = msg;
            Debug.Log($"[BreedingUI] {msg}");
        }

        // ========== Helpers ==========

        private Creature GenerateRandomCreature(string name, int generation)
        {
            var c = new Creature
            {
                Name = name,
                Generation = generation,
                Skeleton = PickRandom(new[] { "Bipede", "Quadrupede", "Volant", "Serpentin" }),
                Shape = PickRandom(new[] { "Rond", "Triangle", "Carre", "Ovale", "Losange" }),
                Color = PickRandom(new[] { "Rouge", "Bleu", "Vert", "Jaune", "Violet", "Orange" })
            };

            c.Strength = RandomStat();
            c.Agility = RandomStat();
            c.Intelligence = RandomStat();
            c.Luck = RandomStat();
            c.Constitution = RandomStat();
            c.Willpower = RandomStat();
            c.CurrentHealth = c.MaxHealth;
            return c;
        }

        private float RandomStat() => Random.Range(3f, 12f);

        private string PickRandom(string[] arr) => arr[Random.Range(0, arr.Length)];

        private string FormatStats(Creature c)
        {
            return $"FOR {c.Strength:F1} | AGI {c.Agility:F1} | INT {c.Intelligence:F1}\n" +
                   $"CHA {c.Luck:F1} | CON {c.Constitution:F1} | VOL {c.Willpower:F1}\n" +
                   $"Total: {c.TotalStats:F1}";
        }

        private string FormatMinMax(float[] min, float[] max)
        {
            string[] names = { "FOR", "AGI", "INT", "CHA", "CON", "VOL" };
            var lines = new System.Text.StringBuilder();
            for (int i = 0; i < 6; i++)
                lines.AppendLine($"{names[i]} : {min[i]:F1} — {max[i]:F1}");
            return lines.ToString();
        }

        private float ParseFloat(TMP_InputField field, float defaultValue)
        {
            if (field == null) return defaultValue;
            return float.TryParse(field.text, out float v) ? v : defaultValue;
        }

        private float Average(List<float> values)
        {
            float sum = 0f;
            foreach (var v in values) sum += v;
            return values.Count > 0 ? sum / values.Count : 0f;
        }
    }
}
