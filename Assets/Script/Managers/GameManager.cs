using System;
using UnityEngine;
using Assets.Script.Entities;

namespace Assets.Script.Managers
{
    /// <summary>
    /// Singleton central du jeu. Gère le profil joueur, la création de partie
    /// et le cycle de rebirth.
    /// GDD §14.4 — GameManager (singleton, rôle centralisateur).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ==========================
        //  Singleton
        // ==========================

        /// <summary>Instance unique du GameManager, accessible globalement.</summary>
        public static GameManager Instance { get; private set; }

        // ==========================
        //  Données
        // ==========================

        /// <summary>
        /// Profil du joueur actuellement chargé (ressources, inventaire, infos de session).
        /// GDD §14.1 — Structure du Profil.
        /// </summary>
        public PlayerProfile CurrentProfile { get; private set; }

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (CurrentProfile != null)
            {
                CurrentProfile.PlayTime += Time.deltaTime;
            }
        }

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Crée une nouvelle partie avec un profil vierge pour le Run 1.
        /// Initialise les ressources de départ et les unlocks par défaut.
        /// GDD §8 — Run 1 : 1 squelette, 3 formes (Rond, Triangle, Carré),
        /// 3 couleurs (Rouge, Bleu, Vert), 5 slots créatures.
        /// </summary>
        public void NewGame(string playerName)
        {
            CurrentProfile = new PlayerProfile(playerName);

            // Unlocks de départ — GDD §8 Contenu Disponible Run 1
            CurrentProfile.Inventory.UnlockedSkeletons.Add("Bipede");

            CurrentProfile.Inventory.UnlockedShapes.Add("Rond");
            CurrentProfile.Inventory.UnlockedShapes.Add("Triangle");
            CurrentProfile.Inventory.UnlockedShapes.Add("Carre");

            CurrentProfile.Inventory.UnlockedColors.Add("Rouge");
            CurrentProfile.Inventory.UnlockedColors.Add("Bleu");
            CurrentProfile.Inventory.UnlockedColors.Add("Vert");

            // Stock initial de formes et couleurs pour le premier craft
            CurrentProfile.Inventory.AddShapeStock("Rond", 3);
            CurrentProfile.Inventory.AddShapeStock("Triangle", 3);
            CurrentProfile.Inventory.AddShapeStock("Carre", 3);

            CurrentProfile.Inventory.AddColorStock("Rouge", 3);
            CurrentProfile.Inventory.AddColorStock("Bleu", 3);
            CurrentProfile.Inventory.AddColorStock("Vert", 3);

            Debug.Log($"[GameManager] Nouvelle partie créée pour '{playerName}' — Run 1");
        }

        /// <summary>
        /// Effectue un rebirth : reset les ressources et l'inventaire du run,
        /// incrémente le compteur de run, et calcule l'Essence gagnée.
        /// GDD §6.1 — Déclenchement du Rebirth,
        /// §6.2 — Calcul de l'Essence gagnée,
        /// §6.3 — Ce qu'on RESET, §6.4 — Ce qu'on GARDE.
        /// </summary>
        public void Rebirth()
        {
            if (CurrentProfile == null)
            {
                Debug.LogWarning("[GameManager] Rebirth impossible : aucun profil chargé.");
                return;
            }

            // Calcul de l'Essence gagnée — GDD §6.2
            float essenceEarned = CalculateEssenceEarned();
            CurrentProfile.Resources.Add(ResourceType.Essence, essenceEarned);

            Debug.Log($"[GameManager] Rebirth ! Essence gagnée : {essenceEarned:F1}. " +
                      $"Run {CurrentProfile.RunCount} → {CurrentProfile.RunCount + 1}");

            // Reset des ressources et de l'inventaire — GDD §6.3
            CurrentProfile.Resources.ResetForRebirth();
            CurrentProfile.Inventory.ResetForRebirth();

            // Incrémentation du run — GDD §6.1
            CurrentProfile.RunCount++;
        }

        /// <summary>
        /// Charge un profil existant dans le GameManager.
        /// Utilisé par le système de sauvegarde (Prompt 3).
        /// </summary>
        public void LoadProfile(PlayerProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("[GameManager] LoadProfile : profil null ignoré.");
                return;
            }

            CurrentProfile = profile;
            Debug.Log($"[GameManager] Profil chargé : '{profile.PlayerName}' — Run {profile.RunCount}");
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>
        /// Calcule l'Essence gagnée à la fin d'un run.
        /// Formule placeholder basée sur GDD §6.2 :
        /// Essence = f(zones_explorées, créatures_créées, gen_max, temps_de_run).
        /// TODO : Implémenter la formule complète quand les zones et expéditions seront en place.
        /// </summary>
        private float CalculateEssenceEarned()
        {
            if (CurrentProfile == null) return 0f;

            // Formule placeholder — GDD §6.2
            // Base : nombre de créatures créées × bonus de run
            int creatureCount = CurrentProfile.Inventory.OwnedCreatures.Count;
            int maxGeneration = 0;
            foreach (var creature in CurrentProfile.Inventory.OwnedCreatures)
            {
                if (creature.Generation > maxGeneration)
                    maxGeneration = creature.Generation;
            }

            // Essence = (nb_créatures × 0.5) + (gen_max × 2) + (run_count × 1)
            float essence = (creatureCount * 0.5f) + (maxGeneration * 2f) + (CurrentProfile.RunCount * 1f);

            return Mathf.Max(1f, essence);
        }
    }
}
