using System;
using UnityEngine;

namespace Assets.Script.Creatures
{
    /// <summary>
    /// Représente une créature du jeu IdleOne avec ses 6 stats (hexagone),
    /// ses composants et ses stats dérivées.
    /// GDD §3.1 — Les 6 Stats, §3.2 — Composants, §4.1 — Formule de craft.
    /// </summary>
    [Serializable]
    public class Creature
    {
        // ==========================
        //  Identité & Composants
        // ==========================

        /// <summary>Nom affiché de la créature.</summary>
        public string Name;

        /// <summary>Type de base (bipède, quadrupède, volant, serpentin…).</summary>
        public string Skeleton;

        /// <summary>Forme influençant l'apparence et une stat principale.
        /// Valeurs possibles : Rond, Triangle, Carré, Ovale, Losange, Étoile.</summary>
        public string Shape;

        /// <summary>Couleur influençant l'affinité élémentaire et une stat secondaire.
        /// Valeurs possibles : Rouge, Bleu, Vert, Jaune, Violet, Orange, Doré, Arc-en-ciel.</summary>
        public string Color;

        /// <summary>Gen-0 = craftée, Gen-1+ = issue du breeding.
        /// Le potentiel augmente avec la génération.</summary>
        public int Generation;

        // ==========================
        //  6 Stats — Hexagone
        // ==========================

        /// <summary>Force (FOR / STR) — Dégâts physiques, capacité de charge.</summary>
        public int Strength;

        /// <summary>Agilité (AGI) — Vitesse, esquive.</summary>
        public int Agility;

        /// <summary>Intelligence (INT) — Dégâts magiques, efficacité spécialisée.</summary>
        public int Intelligence;

        /// <summary>Chance (CHA / LCK) — Drops rares, crits, mutations.</summary>
        public int Luck;

        /// <summary>Constitution (CON) — PV, endurance.</summary>
        public int Constitution;

        /// <summary>Volonté (VOL / WIL) — Résistance, bonus idle.</summary>
        public int Willpower;

        // ==========================
        //  Stats de base (pour calcul)
        // ==========================

        [HideInInspector] public int BaseStrength;
        [HideInInspector] public int BaseAgility;
        [HideInInspector] public int BaseIntelligence;
        [HideInInspector] public int BaseLuck;
        [HideInInspector] public int BaseConstitution;
        [HideInInspector] public int BaseWillpower;

        [HideInInspector] public int ShapeBonusStrength;
        [HideInInspector] public int ShapeBonusAgility;
        [HideInInspector] public int ShapeBonusIntelligence;
        [HideInInspector] public int ShapeBonusLuck;
        [HideInInspector] public int ShapeBonusConstitution;
        [HideInInspector] public int ShapeBonusWillpower;

        [HideInInspector] public int ColorBonusStrength;
        [HideInInspector] public int ColorBonusAgility;
        [HideInInspector] public int ColorBonusIntelligence;
        [HideInInspector] public int ColorBonusLuck;
        [HideInInspector] public int ColorBonusConstitution;
        [HideInInspector] public int ColorBonusWillpower;

        // ==========================
        //  Stats dérivées
        // ==========================

        /// <summary>PV max = Constitution × 10 (équilibrage à ajuster).</summary>
        public int MaxHealth => Constitution * 10;

        /// <summary>PV actuels.</summary>
        public int CurrentHealth;

        /// <summary>Somme totale des 6 stats (pour comparaison rapide).</summary>
        public int TotalStats => Strength + Agility + Intelligence + Luck + Constitution + Willpower;

        // ==========================
        //  Rôles en expédition (GDD §3.1)
        // ==========================

        /// <summary>Casse obstacles, cases combat.</summary>
        public bool CanBreakObstacles => Strength >= 5;

        /// <summary>Réduit le timer d'exploration par case.
        /// Multiplicateur : 1% de réduction par point d'AGI.</summary>
        public float ExplorationSpeedMultiplier => Mathf.Max(0.1f, 1f - (Agility * 0.01f));

        /// <summary>Révèle cases adjacentes (+vision).</summary>
        public int VisionRadius => 1 + (Intelligence / 10);

        /// <summary>Meilleurs drops, cases cachées bonus.</summary>
        public float DropQualityMultiplier => 1f + (Luck * 0.02f);

        /// <summary>Explore plusieurs cases d'affilée.</summary>
        public int MaxConsecutiveExplorations => 1 + (Constitution / 5);

        /// <summary>Continue d'explorer offline.</summary>
        public bool CanExploreOffline => Willpower >= 5;

        // ==========================
        //  Méthodes
        // ==========================

        public Creature()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Recalcule les stats finales à partir des composants.
        /// Formule GDD §4.1 : Stat[i] = base_squelette[i] + bonus_forme[i] + bonus_couleur[i]
        /// </summary>
        public void RecalculateStats()
        {
            Strength      = BaseStrength      + ShapeBonusStrength      + ColorBonusStrength;
            Agility       = BaseAgility       + ShapeBonusAgility       + ColorBonusAgility;
            Intelligence  = BaseIntelligence  + ShapeBonusIntelligence  + ColorBonusIntelligence;
            Luck          = BaseLuck          + ShapeBonusLuck          + ColorBonusLuck;
            Constitution  = BaseConstitution  + ShapeBonusConstitution  + ColorBonusConstitution;
            Willpower     = BaseWillpower     + ShapeBonusWillpower     + ColorBonusWillpower;

            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// Retourne les 6 stats dans l'ordre de l'hexagone radar chart :
        /// Force, Agilité, Intelligence, Chance, Constitution, Volonté.
        /// </summary>
        public int[] GetStatsArray()
        {
            return new int[] { Strength, Agility, Intelligence, Luck, Constitution, Willpower };
        }

        /// <summary>
        /// Applique des dégâts à la créature.
        /// </summary>
        public void TakeDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        }

        /// <summary>
        /// Soigne la créature jusqu'à son max.
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        /// <summary>
        /// Retourne true si la créature est encore en vie.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Réinitialise les PV au maximum.
        /// </summary>
        public void FullHeal()
        {
            CurrentHealth = MaxHealth;
        }
    }
}
