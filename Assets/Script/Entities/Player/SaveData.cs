using System;
using System.Collections.Generic;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Conteneur principal de sauvegarde. Encapsule le profil joueur,
    /// les métadonnées rapides, et les données sérialisées des Dictionary
    /// non supportés par JsonUtility.
    /// GDD §16.4 — Données Sauvegardées, §16.5 — Migration de Sauvegardes.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Métadonnées rapides pour l'affichage des slots.</summary>
        public SaveMetadata Metadata;

        /// <summary>Profil complet du joueur.</summary>
        public PlayerProfile Profile;

        /// <summary>
        /// Version du format de sauvegarde pour la migration future.
        /// GDD §16.5.
        /// </summary>
        public string Version;

        // ==========================
        //  Données sérialisées (contournement JsonUtility)
        // ==========================

        /// <summary>
        /// Stock de formes sérialisé (remplace PlayerInventory.ShapeStock
        /// qui est un Dictionary non supporté par JsonUtility).
        /// </summary>
        public List<SerializableKeyValue> SerializedShapeStock;

        /// <summary>
        /// Stock de couleurs sérialisé (remplace PlayerInventory.ColorStock
        /// qui est un Dictionary non supporté par JsonUtility).
        /// </summary>
        public List<SerializableKeyValue> SerializedColorStock;

        public SaveData() { }

        /// <summary>
        /// Crée un SaveData à partir d'un profil joueur.
        /// Extrait les Dictionary en listes sérialisables et construit les métadonnées.
        /// </summary>
        public static SaveData FromProfile(PlayerProfile profile, string gameVersion)
        {
            var data = new SaveData
            {
                Profile = profile,
                Version = gameVersion,
                Metadata = new SaveMetadata(profile, gameVersion)
            };

            // Convertir les Dictionary en listes sérialisables
            if (profile != null && profile.Inventory != null)
            {
                data.SerializedShapeStock = SerializableKeyValue.FromDictionary(profile.Inventory.ShapeStock);
                data.SerializedColorStock = SerializableKeyValue.FromDictionary(profile.Inventory.ColorStock);
            }
            else
            {
                data.SerializedShapeStock = new List<SerializableKeyValue>();
                data.SerializedColorStock = new List<SerializableKeyValue>();
            }

            return data;
        }

        /// <summary>
        /// Restaure les Dictionary du profil à partir des listes sérialisées.
        /// Doit être appelé après désérialisation JSON pour reconstruire les données
        /// que JsonUtility ne peut pas gérer nativement.
        /// </summary>
        public void RestoreDictionaries()
        {
            if (Profile != null && Profile.Inventory != null)
            {
                Profile.Inventory.ShapeStock = SerializableKeyValue.ToDictionary(SerializedShapeStock);
                Profile.Inventory.ColorStock = SerializableKeyValue.ToDictionary(SerializedColorStock);
            }
        }
    }
}
