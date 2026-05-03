using System;
using System.Collections.Generic;
using Assets.Script.Creatures;

namespace Assets.Script.Entities
{
    /// <summary>
    /// Inventaire du joueur : créatures possédées, créatures actives,
    /// types débloqués et stocks de composants consommables.
    /// GDD §3.2 — Composants, §6.5 — Boutique de Prestige (slots, unlocks),
    /// §7.1 — Formes et Couleurs comme ressources consommables.
    /// </summary>
    [Serializable]
    public class PlayerInventory
    {
        // ==========================
        //  Créatures
        // ==========================

        /// <summary>Toutes les créatures possédées par le joueur. GDD §3 — Système de Créatures.</summary>
        public List<Creature> OwnedCreatures = new List<Creature>();

        /// <summary>
        /// Créatures actuellement déployées (slots limités).
        /// GDD §6.5 — Slot Créature+ (augmentable via prestige).
        /// </summary>
        public List<Creature> ActiveCreatures = new List<Creature>();

        /// <summary>
        /// Nombre maximum de créatures actives simultanées.
        /// Par défaut 5, augmentable via la boutique de prestige (§6.5 — Slot Créature+).
        /// </summary>
        public int MaxActiveSlots = 5;

        // ==========================
        //  Types débloqués (persistent au rebirth)
        // ==========================

        /// <summary>
        /// Types de squelettes débloqués. Non consommables, choisis au moment du craft.
        /// GDD §3.2 — Squelette = type de base, §6.5 — Nouveau Squelette (achat prestige).
        /// </summary>
        public List<string> UnlockedSkeletons = new List<string>();

        /// <summary>
        /// Formes disponibles pour le craft. Débloquées via prestige ou expéditions.
        /// GDD §3.3 — Affinités Formes, §6.5 — Pack Formes.
        /// </summary>
        public List<string> UnlockedShapes = new List<string>();

        /// <summary>
        /// Couleurs disponibles pour le craft. Débloquées via prestige ou expéditions.
        /// GDD §3.4 — Affinités Couleurs, §6.5 — Pack Couleurs.
        /// </summary>
        public List<string> UnlockedColors = new List<string>();

        // ==========================
        //  Stocks consommables (reset au rebirth)
        // ==========================

        /// <summary>
        /// Stock de formes consommables utilisées pour le craft Gen-0.
        /// GDD §7.1 — Formes (items) obtenues via expéditions et drops.
        /// </summary>
        public Dictionary<string, int> ShapeStock = new Dictionary<string, int>();

        /// <summary>
        /// Stock de couleurs consommables utilisées pour le craft Gen-0.
        /// GDD §7.1 — Couleurs (items) obtenues via expéditions et drops.
        /// </summary>
        public Dictionary<string, int> ColorStock = new Dictionary<string, int>();

        // ==========================
        //  Méthodes — Créatures
        // ==========================

        /// <summary>
        /// Ajoute une créature à la collection du joueur.
        /// GDD §4.1 — Craft Gen-0, §4.2 — Breeding Gen-1+.
        /// </summary>
        public void AddCreature(Creature creature)
        {
            if (creature == null) return;
            OwnedCreatures.Add(creature);
        }

        /// <summary>
        /// Retire une créature de la collection du joueur.
        /// La retire aussi des créatures actives si elle y était.
        /// GDD §7.5.1 — Temple des Sacrifices (recyclage).
        /// </summary>
        public void RemoveCreature(Creature creature)
        {
            if (creature == null) return;
            OwnedCreatures.Remove(creature);
            ActiveCreatures.Remove(creature);
        }

        /// <summary>
        /// Active une créature (la déploie dans un slot actif).
        /// Retourne false si la créature n'est pas possédée, déjà active, ou si les slots sont pleins.
        /// GDD §6.5 — Slot Créature+ (MaxActiveSlots augmentable via prestige).
        /// </summary>
        public bool SetActive(Creature creature)
        {
            if (creature == null) return false;
            if (!OwnedCreatures.Contains(creature)) return false;
            if (ActiveCreatures.Contains(creature)) return false;
            if (ActiveCreatures.Count >= MaxActiveSlots) return false;

            ActiveCreatures.Add(creature);
            return true;
        }

        /// <summary>
        /// Désactive une créature (la retire des slots actifs).
        /// </summary>
        public bool SetInactive(Creature creature)
        {
            if (creature == null) return false;
            return ActiveCreatures.Remove(creature);
        }

        // ==========================
        //  Méthodes — Stocks
        // ==========================

        /// <summary>
        /// Ajoute des formes consommables au stock.
        /// GDD §7.1 — Formes obtenues via expéditions.
        /// </summary>
        public void AddShapeStock(string shape, int amount)
        {
            if (string.IsNullOrEmpty(shape) || amount <= 0) return;
            if (ShapeStock.ContainsKey(shape))
                ShapeStock[shape] += amount;
            else
                ShapeStock[shape] = amount;
        }

        /// <summary>
        /// Ajoute des couleurs consommables au stock.
        /// GDD §7.1 — Couleurs obtenues via expéditions.
        /// </summary>
        public void AddColorStock(string color, int amount)
        {
            if (string.IsNullOrEmpty(color) || amount <= 0) return;
            if (ColorStock.ContainsKey(color))
                ColorStock[color] += amount;
            else
                ColorStock[color] = amount;
        }

        // ==========================
        //  Rebirth
        // ==========================

        /// <summary>
        /// Réinitialise l'inventaire pour un nouveau run (rebirth).
        /// Reset : créatures, stocks de formes/couleurs.
        /// Gardé : types débloqués (squelettes, formes, couleurs), MaxActiveSlots.
        /// GDD §6.3 — Ce qu'on RESET (toutes les créatures, formes, couleurs, matériaux),
        /// §6.4 — Ce qu'on GARDE (squelettes/formes/couleurs débloqués).
        /// </summary>
        public void ResetForRebirth()
        {
            OwnedCreatures.Clear();
            ActiveCreatures.Clear();
            ShapeStock.Clear();
            ColorStock.Clear();
            // UnlockedSkeletons, UnlockedShapes, UnlockedColors et MaxActiveSlots persistent
        }
    }
}
