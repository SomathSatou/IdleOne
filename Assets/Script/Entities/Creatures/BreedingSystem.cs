using System;
using UnityEngine;

namespace Assets.Script.Creatures
{
    /// <summary>
    /// Système de breeding selon GDD §4.2 — Phase 2 Breeding (Gen-1+).
    /// Calcule les stats d'un enfant à partir de deux parents avec variance,
    /// bonus de génération, mutations, et héritage des composants.
    /// </summary>
    public static class BreedingSystem
    {
        public const float DefaultVarianceFactor = 0.25f;
        public const float DefaultMutationChance = 0.05f;
        public const int MaxStatFloor = 1;

        /// <summary>
        /// Résultat d'une opération de breeding avec détails de prédiction.
        /// </summary>
        public class BreedingResult
        {
            public Creature Child;
            public float[] MinStats;
            public float[] MaxStats;
            public bool Mutated;
            public string MutationDescription;
        }

        /// <summary>
        /// Données de prédiction pour l'UI (fourchettes min/max par stat).
        /// </summary>
        public class BreedingPrediction
        {
            public float[] MinStats; // 6 stats
            public float[] MaxStats; // 6 stats
            public string PredictedSkeleton;
            public string PredictedShape;
            public string PredictedColor;
        }

        /// <summary>
        /// Calcule la prédiction de breeding (fourchettes sans aléatoire).
        /// </summary>
        public static BreedingPrediction Predict(Creature parentA, Creature parentB,
            float varianceFactor = DefaultVarianceFactor,
            float breedingMinBonus = 0f, float breedingMaxBonus = 0f)
        {
            var statsA = parentA.GetStatsArray();
            var statsB = parentB.GetStatsArray();

            var minStats = new float[6];
            var maxStats = new float[6];

            for (int i = 0; i < 6; i++)
            {
                float baseMin = Mathf.Min(statsA[i], statsB[i]);
                float baseMax = Mathf.Max(statsA[i], statsB[i]);
                float variance = (baseMax - baseMin) * varianceFactor;

                minStats[i] = Mathf.Max(MaxStatFloor, baseMin - variance + breedingMinBonus);
                maxStats[i] = baseMax + variance + breedingMaxBonus;
            }

            return new BreedingPrediction
            {
                MinStats = minStats,
                MaxStats = maxStats,
                PredictedSkeleton = PredictSkeleton(parentA, parentB),
                PredictedShape = PredictShape(parentA, parentB),
                PredictedColor = PredictColor(parentA, parentB)
            };
        }

        /// <summary>
        /// Exécute le breeding aléatoire et retourne l'enfant.
        /// </summary>
        public static BreedingResult Breed(Creature parentA, Creature parentB,
            float varianceFactor = DefaultVarianceFactor,
            float breedingMinBonus = 0f, float breedingMaxBonus = 0f,
            float mutationChance = DefaultMutationChance,
            float generationBonus = 0.5f)
        {
            var prediction = Predict(parentA, parentB, varianceFactor, breedingMinBonus, breedingMaxBonus);
            var child = new Creature
            {
                Name = $"Gen-{Mathf.Max(parentA.Generation, parentB.Generation) + 1} Child",
                Generation = Mathf.Max(parentA.Generation, parentB.Generation) + 1,
                Skeleton = ResolveSkeleton(parentA, parentB),
                Shape = ResolveShape(parentA, parentB),
                Color = ResolveColor(parentA, parentB)
            };

            bool mutated = UnityEngine.Random.value < mutationChance;
            string mutationDesc = null;

            float[] childStats = new float[6];
            for (int i = 0; i < 6; i++)
            {
                float roll = UnityEngine.Random.Range(prediction.MinStats[i], prediction.MaxStats[i]);
                childStats[i] = roll + generationBonus;
            }

            if (mutated)
            {
                childStats = ApplyMutation(childStats, out mutationDesc);
                child.Shape = mutationDesc.Contains("Shape") ? "Etoile" : child.Shape;
                child.Color = mutationDesc.Contains("Color") ? "Arc-en-ciel" : child.Color;
            }

            child.Strength = childStats[0];
            child.Agility = childStats[1];
            child.Intelligence = childStats[2];
            child.Luck = childStats[3];
            child.Constitution = childStats[4];
            child.Willpower = childStats[5];

            child.CurrentHealth = child.MaxHealth;

            return new BreedingResult
            {
                Child = child,
                MinStats = prediction.MinStats,
                MaxStats = prediction.MaxStats,
                Mutated = mutated,
                MutationDescription = mutationDesc
            };
        }

        // ========== Héritage Composants ==========

        private static string PredictSkeleton(Creature a, Creature b)
        {
            return $"{a.Skeleton} or {b.Skeleton}";
        }

        private static string ResolveSkeleton(Creature a, Creature b)
        {
            return UnityEngine.Random.value < 0.5f ? a.Skeleton : b.Skeleton;
        }

        private static string PredictShape(Creature a, Creature b)
        {
            // Simplification : dominance aléatoire pour la prédiction
            return $"{a.Shape} vs {b.Shape}";
        }

        private static string ResolveShape(Creature a, Creature b)
        {
            // Simplification : 50/50 pour le prototype
            return UnityEngine.Random.value < 0.5f ? a.Shape : b.Shape;
        }

        private static string PredictColor(Creature a, Creature b)
        {
            // Mélange simple : si parents ont des couleurs mixables
            string mixed = TryMixColor(a.Color, b.Color);
            if (mixed != null)
                return $"{a.Color}+{b.Color} → {mixed}";
            return $"{a.Color} or {b.Color}";
        }

        private static string ResolveColor(Creature a, Creature b)
        {
            string mixed = TryMixColor(a.Color, b.Color);
            if (mixed != null && UnityEngine.Random.value < 0.3f)
                return mixed;
            return UnityEngine.Random.value < 0.5f ? a.Color : b.Color;
        }

        private static string TryMixColor(string c1, string c2)
        {
            var mix = (c1, c2);
            if (mix == ("Rouge", "Bleu") || mix == ("Bleu", "Rouge")) return "Violet";
            if (mix == ("Rouge", "Jaune") || mix == ("Jaune", "Rouge")) return "Orange";
            if (mix == ("Bleu", "Vert") || mix == ("Vert", "Bleu")) return "Cyan"; // ou autre
            return null;
        }

        // ========== Mutations ==========

        private static float[] ApplyMutation(float[] stats, out string description)
        {
            description = "";
            int mutationType = UnityEngine.Random.Range(0, 3);
            var result = (float[])stats.Clone();

            switch (mutationType)
            {
                case 0:
                    // Stat boost aléatoire
                    int statIdx = UnityEngine.Random.Range(0, 6);
                    result[statIdx] += UnityEngine.Random.Range(2f, 5f);
                    description = $"Mutation: +{result[statIdx] - stats[statIdx]:F1} {(CreatureStat)statIdx}";
                    break;
                case 1:
                    // Shape mutation → Etoile
                    description = "Mutation: Shape → Etoile";
                    break;
                case 2:
                    // Color mutation → Arc-en-ciel
                    description = "Mutation: Color → Arc-en-ciel";
                    break;
            }

            return result;
        }
    }

    public enum CreatureStat
    {
        Strength = 0,
        Agility = 1,
        Intelligence = 2,
        Luck = 3,
        Constitution = 4,
        Willpower = 5
    }
}
