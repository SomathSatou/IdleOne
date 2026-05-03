using System;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Profil complet du joueur contenant ses informations de session,
    /// ses ressources et son inventaire.
    /// GDD §14.1 — Structure du Profil.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        // ==========================
        //  Identité
        // ==========================

        /// <summary>Nom du joueur choisi à la création de la partie.</summary>
        public string PlayerName;

        /// <summary>
        /// Numéro du run actuel (commence à 1).
        /// Incrémenté à chaque rebirth. GDD §6 — Rebirth & Prestige.
        /// </summary>
        public int RunCount = 1;

        // ==========================
        //  Temps de jeu
        // ==========================

        /// <summary>Temps de jeu total en secondes (cumulé sur tous les runs).</summary>
        public float PlayTime;

        /// <summary>
        /// Date de création du profil (ISO 8601 string pour compatibilité JsonUtility).
        /// </summary>
        public string CreatedAt;

        /// <summary>
        /// Date de la dernière sauvegarde (ISO 8601 string pour compatibilité JsonUtility).
        /// </summary>
        public string LastSavedAt;

        // ==========================
        //  Sous-systèmes
        // ==========================

        /// <summary>
        /// Ressources du joueur (Fragments, Essence, Gold, Taboo, Wishes, Materials).
        /// GDD §7.1 — Ressources de craft créatures.
        /// </summary>
        public PlayerResources Resources = new PlayerResources();

        /// <summary>
        /// Inventaire du joueur (créatures, unlocks, stocks).
        /// GDD §3.2 — Composants, §6.5 — Boutique de Prestige.
        /// </summary>
        public PlayerInventory Inventory = new PlayerInventory();

        // ==========================
        //  Méthodes
        // ==========================

        /// <summary>
        /// Crée un nouveau profil avec les valeurs par défaut pour le Run 1.
        /// </summary>
        public PlayerProfile() { }

        /// <summary>
        /// Crée un nouveau profil avec le nom du joueur et initialise les timestamps.
        /// </summary>
        public PlayerProfile(string playerName)
        {
            PlayerName = playerName;
            RunCount = 1;
            PlayTime = 0f;
            CreatedAt = DateTime.UtcNow.ToString("o");
            LastSavedAt = CreatedAt;
            Resources = new PlayerResources();
            Inventory = new PlayerInventory();
        }
    }
}
