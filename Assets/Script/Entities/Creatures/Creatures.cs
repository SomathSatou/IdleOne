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
        // :S: TODO string to class 
        public string Skeleton;

        /// <summary>Forme influençant l'apparence et une stat principale.
        /// Valeurs possibles : Rond, Triangle, Carré, Ovale, Losange, Étoile.</summary>
        // :S: TODO string to class 
        public string Shape;

        /// <summary>Couleur influençant l'affinité élémentaire et une stat secondaire.
        /// Valeurs possibles : Rouge, Bleu, Vert, Jaune, Violet, Orange, Doré, Arc-en-ciel.</summary>
        // :S: TODO string to class 
        public string Color;

        /// <summary>Gen-0 = craftée, Gen-1+ = issue du breeding.
        /// Le potentiel augmente avec la génération.</summary>
        public int Generation;

        // ==========================
        //  6 Stats — Hexagone
        // ==========================

        /// <summary>Force (FOR / STR) — Dégâts physiques, capacité de charge.</summary>
        public float Strength;

        /// <summary>Agilité (AGI) — Vitesse, esquive.</summary>
        public float Agility;

        /// <summary>Intelligence (INT) — Dégâts magiques, efficacité spécialisée.</summary>
        public float Intelligence;

        /// <summary>Chance (CHA / LCK) — Drops rares, crits, mutations.</summary>
        public float Luck;

        /// <summary>Constitution (CON) — PV, endurance.</summary>
        public float Constitution;

        /// <summary>Volonté (VOL / WIL) — Résistance, bonus idle.</summary>
        public float Willpower;

        // ==========================
        //  Stats de base (pour calcul)
        // ==========================

        [HideInInspector] public float BaseStrength;
        [HideInInspector] public float BaseAgility;
        [HideInInspector] public float BaseIntelligence;
        [HideInInspector] public float BaseLuck;
        [HideInInspector] public float BaseConstitution;
        [HideInInspector] public float BaseWillpower;

        [HideInInspector] public float ShapeBonusStrength;
        [HideInInspector] public float ShapeBonusAgility;
        [HideInInspector] public float ShapeBonusIntelligence;
        [HideInInspector] public float ShapeBonusLuck;
        [HideInInspector] public float ShapeBonusConstitution;
        [HideInInspector] public float ShapeBonusWillpower;

        [HideInInspector] public float ColorBonusStrength;
        [HideInInspector] public float ColorBonusAgility;
        [HideInInspector] public float ColorBonusIntelligence;
        [HideInInspector] public float ColorBonusLuck;
        [HideInInspector] public float ColorBonusConstitution;
        [HideInInspector] public float ColorBonusWillpower;

        // ==========================
        //  Stats dérivées
        // ==========================

        /// <summary>PV max = Constitution × 10 (équilibrage à ajuster).</summary>
        public float MaxHealth => Constitution * 10;

        /// <summary>PV actuels.</summary>
        public float CurrentHealth;

        /// <summary>Somme totale des 6 stats (pour comparaison rapide).</summary>
        public float TotalStats => Strength + Agility + Intelligence + Luck + Constitution + Willpower;

        // ==========================
        //  Rôles en expédition (GDD §3.1)
        // ==========================

        /// <summary>Casse obstacles, cases combat.</summary>
        public bool CanBreakObstacles => Strength >= 5;

        /// <summary>Réduit le timer d'exploration par case.
        /// Multiplicateur : 1% de réduction par point d'AGI.</summary>
        public float ExplorationSpeedMultiplier => Mathf.Max(0.1f, 1f - (Agility * 0.01f));

        /// <summary>Révèle cases adjacentes (+vision).</summary>
        public float VisionRadius => 1 + (Intelligence / 10);

        /// <summary>Meilleurs drops, cases cachées bonus.</summary>
        public float DropQualityMultiplier => 1f + (Luck * 0.02f);

        /// <summary>Explore plusieurs cases d'affilée.</summary>
        // TODO check a better way than casting to int
        public int MaxConsecutiveExplorations => 1 + ((int)Constitution / 5);

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
        public float[] GetStatsArray()
        {
            return new float[] { Strength, Agility, Intelligence, Luck, Constitution, Willpower };
        }

        /// <summary>
        /// Applique des dégâts à la créature.
        /// </summary>
        public void TakeDamage(float damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        }

        /// <summary>
        /// Soigne la créature jusqu'à son max.
        /// </summary>
        public void Heal(float amount)
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
