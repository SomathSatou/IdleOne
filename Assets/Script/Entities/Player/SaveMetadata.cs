using System;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Métadonnées rapides d'une sauvegarde, permettant d'afficher les infos
    /// d'un slot sans charger tout le profil.
    /// GDD §16.1 — Slots de Sauvegarde.
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        /// <summary>Nom du joueur.</summary>
        public string PlayerName;

        /// <summary>Numéro du run actuel.</summary>
        public int RunCount;

        /// <summary>Temps de jeu total en secondes.</summary>
        public float PlayTime;

        /// <summary>Date de la dernière sauvegarde (ISO 8601).</summary>
        public string LastSavedAt;

        /// <summary>Version du jeu au moment de la sauvegarde.</summary>
        public string GameVersion;

        public SaveMetadata() { }

        /// <summary>
        /// Crée les métadonnées à partir d'un profil joueur.
        /// </summary>
        public SaveMetadata(PlayerProfile profile, string gameVersion)
        {
            if (profile != null)
            {
                PlayerName = profile.PlayerName;
                RunCount = profile.RunCount;
                PlayTime = profile.PlayTime;
                LastSavedAt = profile.LastSavedAt;
            }
            GameVersion = gameVersion;
        }
    }
}
