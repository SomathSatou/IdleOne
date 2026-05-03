using System;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Types de ressources disponibles dans le jeu.
    /// GDD §7.1 — Ressources de craft créatures.
    /// </summary>
    public enum ResourceType
    {
        Fragments,
        Essence,
        Gold,
        Taboo,
        Wishes,
        Materials
    }

    /// <summary>
    /// Centralise toutes les ressources du joueur.
    /// GDD §7.1 — Ressources, §6.2 — Essence (prestige), §7.2.2 — Taboo, §7.2.3 — Wish.
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        // ==========================
        //  Ressources — Run (reset au rebirth)
        // ==========================

        /// <summary>Monnaie de base obtenue par clic et idle. Reset au rebirth. GDD §7.1.</summary>
        public float Fragments;

        /// <summary>Monnaie de commerce late-game. Reset au rebirth sauf upgrades. GDD §7.2.1.</summary>
        public float Gold;

        /// <summary>Matériaux bruts pour upgrades de bâtiments. Reset au rebirth. GDD §7.1.</summary>
        public int Materials;

        // ==========================
        //  Ressources — Persistantes (gardées au rebirth)
        // ==========================

        /// <summary>Monnaie de prestige gagnée au rebirth. Persiste entre les runs. GDD §6.2.</summary>
        public float Essence;

        /// <summary>Ressource morale/corruption. Persiste entre les runs. GDD §7.2.2.</summary>
        public float Taboo;

        /// <summary>Ressource meta-game gagnée via milestones. Persiste entre les runs. GDD §7.2.3.</summary>
        public int Wishes;

        // ==========================
        //  Events
        // ==========================

        /// <summary>
        /// Événement déclenché quand une ressource change de valeur.
        /// Paramètres : (ResourceType, ancienneValeur, nouvelleValeur).
        /// L'UI peut s'abonner pour se rafraîchir automatiquement.
        /// </summary>
        [NonSerialized]
        public Action<ResourceType, float, float> OnResourceChanged;

        // ==========================
        //  Méthodes publiques
        // ==========================

        /// <summary>
        /// Ajoute une quantité à la ressource spécifiée.
        /// GDD §7.1 — Les ressources sont accumulées via clic, idle, expéditions et rebirth.
        /// </summary>
        public void Add(ResourceType type, float amount)
        {
            if (amount <= 0) return;

            float oldValue = GetValue(type);
            SetValue(type, oldValue + amount);
            float newValue = GetValue(type);

            OnResourceChanged?.Invoke(type, oldValue, newValue);
        }

        /// <summary>
        /// Dépense une quantité de la ressource spécifiée.
        /// Retourne true si la dépense a réussi, false si le joueur n'a pas assez.
        /// GDD §7.3 — Coûts de craft, breeding, upgrades.
        /// </summary>
        public bool Spend(ResourceType type, float amount)
        {
            if (amount <= 0) return false;
            if (!CanAfford(type, amount)) return false;

            float oldValue = GetValue(type);
            SetValue(type, oldValue - amount);
            float newValue = GetValue(type);

            OnResourceChanged?.Invoke(type, oldValue, newValue);
            return true;
        }

        /// <summary>
        /// Vérifie si le joueur possède assez de la ressource spécifiée.
        /// GDD §7.3 — Vérification avant toute action coûteuse.
        /// </summary>
        public bool CanAfford(ResourceType type, float amount)
        {
            return GetValue(type) >= amount;
        }

        /// <summary>
        /// Réinitialise les ressources pour un nouveau run (rebirth).
        /// Reset : Fragments, Gold, Materials.
        /// Gardé : Essence, Taboo, Wishes.
        /// GDD §6.3 — Ce qu'on RESET, §6.4 — Ce qu'on GARDE.
        /// </summary>
        public void ResetForRebirth()
        {
            float oldFragments = Fragments;
            float oldGold = Gold;
            int oldMaterials = Materials;

            Fragments = 0f;
            Gold = 0f;
            Materials = 0;

            OnResourceChanged?.Invoke(ResourceType.Fragments, oldFragments, 0f);
            OnResourceChanged?.Invoke(ResourceType.Gold, oldGold, 0f);
            OnResourceChanged?.Invoke(ResourceType.Materials, oldMaterials, 0f);
        }

        // ==========================
        //  Méthodes internes
        // ==========================

        /// <summary>
        /// Retourne la valeur actuelle d'une ressource par son type.
        /// </summary>
        public float GetValue(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Fragments: return Fragments;
                case ResourceType.Essence:   return Essence;
                case ResourceType.Gold:      return Gold;
                case ResourceType.Taboo:     return Taboo;
                case ResourceType.Wishes:    return Wishes;
                case ResourceType.Materials: return Materials;
                default: return 0f;
            }
        }

        private void SetValue(ResourceType type, float value)
        {
            switch (type)
            {
                case ResourceType.Fragments: Fragments = value; break;
                case ResourceType.Essence:   Essence = value; break;
                case ResourceType.Gold:      Gold = value; break;
                case ResourceType.Taboo:     Taboo = value; break;
                case ResourceType.Wishes:    Wishes = (int)value; break;
                case ResourceType.Materials: Materials = (int)value; break;
            }
        }
    }
}
