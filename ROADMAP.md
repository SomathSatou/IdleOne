# Roadmap IdleOne — v0.1.0 → v1.0.0

Ce document trace les étapes d'implémentation progressive du jeu, du prototype minimal jusqu'à la version complète avec tous les systèmes documentés dans le [GDD](GDD.md).

---

## État Actuel (Baseline)

- **Modèle créature** : `Creature.cs` avec 6 stats, composants en `string` (Skeleton, Shape, Color), stats dérivées (PV, expédition).
- **Prototype UI** : `CreatureUI.cs` + `CreatureHealthTicker.cs` + `DemoCreatureSpawner.cs` — affichage PV, barre de santé, bouton "Nourrir", statut Actif/Incapacité.
- **Non implémenté** : craft, breeding, expéditions, économie, prestige, bâtiments, mutations.

---

## v0.1 — Fondations Créature & UI

### v0.1.0 — Baseline actuelle (terminé)

- Mock créature avec 6 stats en `string`, UI PV + statut Actif/Incapacité.

### v0.1.1 — Écran Breeding Minimal

Objectif : un écran fonctionnel avec 3 panneaux et un résultat de breeding.

- **UI** : 3 panneaux — Parent 1 (gauche), Parent 2 (droite), Résultat (centre).
- **Data** : mock de 2 créatures avec affichage nom + 6 stats textuelles.
- **Mécanique** : bouton "Breed" générant un enfant Gen-1 via formule simplifiée (moyenne des parents + variance aléatoire).
- **Visualisation** : stats affichées en texte (pas d'hexagone graphique).

### v0.1.2 — Affichage Stats & Refactor Modèle

- **Carte créature** : affichage nom, PV max, statut, génération, composants.
- **Refactor** : introduction des classes `Skeleton`, `Shape`, `Color` (remplacer les `string` du modèle actuel).

---

## v0.2 — Mécaniques de Créature (Craft & Breeding)

### v0.2.0 — Formules de Breeding Complètes

Objectif : implémenter les formules du GDD §4.

- **Breeding** : formule complète avec `variance_factor`, floor/ceil, `breeding_min/max_bonus`.
- **Régression** : un enfant peut être moins bon que ses parents.

### v0.2.1 — Craft Gen-0 Déterministe

- **Craft** : `Squelette + Forme + Couleur → Créature` (formule déterministe §4.1).
- **UI** : écran de sélection des composants pour le craft.

### v0.2.2 — Héritage & Mutations de Base

- **Héritage** : règles de transmission squelette/forme/couleur.
- **Mutations** : probabilité de base, formes/couleurs rares (Étoile, Doré, Arc-en-ciel).

---

## v0.3 — Visualisation & Data Créature

### v0.3.0 — Radar Chart Hexagone

Objectif : rendre les stats visuellement lisibles via un radar chart.

- **Hexagone** : affichage des 6 stats (Force, Agilité, Intelligence, Chance, Constitution, Volonté).
- **Prédiction** : zone probable en breeding (min/max des parents).

### v0.3.1 — Refactor Creature.cs

- **Intégration** : `Creature.cs` utilisant les classes composants de v0.2.
- **Stats dérivées** : PV, endurance d'expédition.

---

## v0.4 — Monde Top-Down & Spatialité

### v0.4.0 — Caméra, Contrôles & Transitions

Objectif : donner une spatialité au hub du joueur.

- **Caméra** : vue top-down orthographique ou légèrement isométrique.
- **Contrôles** : déplacement clavier (ZQSD / WASD) ou click-to-move. Sprint (Shift).
- **Transitions** : portes/zones entre village, ferme intérieure, bâtiments (Maison, Forge, Labo).

### v0.4.1 — Village & PNJ de Base

- **Zones** : place centrale, marché, zone de farming, entrée expéditions.
- **PNJ** : villageois (lore), marchands (jour), quêteurs. Routines : patrouille, horaires jour/nuit.

### v0.4.2 — Ferme de Créatures & Enclos

- **Enclos** : créatures actives visibles, comportements simples (errance, faim, sommeil).
- **Interactions** : approche → inspection rapide (nom, PV) ou nourrir.

---

## v0.5 — Expéditions & Exploration

### v0.5.0 — Grille & Fog of War

Objectif : envoyer une créature explorer une grille et rapporter des ressources.

- **Grille** : zones carrées avec fog of war (§5.1).
- **Cases** : Ressource, Combat, Événement, Boss, Vide, Cachée (§5.2).

### v0.5.1 — Mécanique d'Exploration

- **Sélection** : créature + case adjacente révélée.
- **Timer idle** : durée basée sur difficulté − AGI de la créature.
- **Récompenses** : ressources, déblocages, cases adjacentes révélées.

### v0.5.2 — Stats d'Expédition & Progression

- **Scaling** : vision (INT), vitesse (AGI), endurance (CON), offline (VOL), drops (CHA), combat (FOR).
- **Progression** : compléter une zone débloque la suivante. Scaling taille grille par run.

---

## v0.6 — Économie de Base & Bâtiments

### v0.6.0 — Ressources & Clic/Idle

Objectif : clic/idle, ressources de base.

- **Fragments** : monnaie de base (clic + idle).
- **Formes / Couleurs / Matériaux** : drops d'expédition.

### v0.6.1 — Bâtiments Run 1

- **Maison du Héros** : hub central, stockage, craft.
- **Pension / Ranch** : soigne, régénère endurance, augmente slots créatures.
- **Forge** : craft et amélioration de composants.
- **Laboratoire de Breeding** : centre breeding, prédiction hexagone.

### v0.6.2 — Upgrades & Timers

- **Upgrades** : coût matériaux, effets de niveau (§7.5.2).
- **Timers** : exploration, breeding, repos.

---

## v0.7 — Rebirth & Prestige (Essence)

### v0.7.0 — Système de Rebirth

Objectif : introduire la boucle de reset/progression permanente.

- **Déclenchement** : suggestion quand progression stagne.
- **Essence** : calcul basé sur zones explorées, créatures créées, gen max, temps de run.

### v0.7.1 — Reset / Garde

- **Reset** : créatures, ressources, zones, niveaux bâtiments.
- **Garde** : Essence cumulée, squelettes/formes/couleurs débloqués, achievements.

### v0.7.2 — Boutique Prestige

- **Upgrades** : Breeding Min+/Max+, Stats Craft+, Slot Créature+, Mutation Unlock, Idle Speed+, etc. (§6.5).
- **Nouveaux Squelettes** : déblocables avec Essence.

---

## v0.8 — Mutations & Prédiction Avancée

### v0.8.0 — Système de Mutations Complet

- **Unlock** : débloqué après 1er rebirth (achat prestige).
- **Rate** : ~5% par breeding (augmentable).
- **Formes/Couleurs rares** : Étoile, Doré, Arc-en-ciel.

### v0.8.1 — Prédiction de Breeding (UI)

- **Hexagone Parent A** (bleu), **Parent B** (rouge).
- **Zone probable enfant** (vert semi-transparent = fourchette min/max).

### v0.8.2 — Équilibrage & Affinages

- **Régression** : variance_factor, floor, plafonds.
- **Inbreeding** : malus si parents trop proches (lien avec arbre Envie).

---

## v0.9 — Ressources Avancées (Gold, Taboo, Wish)

### v0.9.0 — Gold & Commerce

Objectif : ajouter la monnaie de late-game.

- **Sources** : vente créatures Gen-2+, cases Trésor, Bourse (Run 3+).
- **Sinks** : accélérations, upgrades avancés, cosmétiques, donations Orgueil.

### v0.9.1 — Taboo & Corruption

- **Sources** : sacrifices au Temple, décisions sombres en expédition, recycling massif.
- **Sinks** : arbres de péchés, offrandes, vœux corrompus.
- **Narratif** : transformation visuelle de la base selon le niveau de Taboo.

### v0.9.2 — Wish & Meta-Game

- **Déblocage** : Run 3+, accumulé 500+ Taboo, milestones globales.
- **Types** : Créature, Objet (Artefacts), Sagesse, Renaissance.
- **Corrompus** : versions améliorées via Taboo supplémentaire.

---

## v0.10 — Les 7 Arbres de Péchés

### v0.10.0 — Colère (Wrath)

- 10 nœuds, tiers Fondation/Spécialisation/Maîtrise.
- Focus : dégâts, combat, expédition, forme exclusive Étoile Rouge.

### v0.10.1 — Cupidité (Greed) & Gourmandise (Gluttony)

- **Cupidité** : Gold, Fragments, intérêts, bâtiment exclusif Fontaine de Richesse.
- **Gourmandise** : cuisine, buffs nourriture, repas avancés, buffs permanents.

### v0.10.2 — Orgueil (Pride), Envie (Envy) & Paresse (Sloth)

- **Orgueil** : visiteurs prestigieux, renommée, transformation finale visuelle.
- **Envie** : optimisation breeding, clonage, designer génétique.
- **Paresse** : timers réduits, auto-craft, auto-breeding, offline boost.

### v0.10.3 — Luxure (Lust) & Synergies Globales

- **Luxure** : charme/CHA, événements nocturnes, Festivités globales.
- **Synergies** : effets cumulatifs entre arbres (ex: Colère + Cupidité = +Gold sur Boss).

---

## v1.0 — Version Complète

### v1.0.0 — Intégration & Boucle Fermée

- **Tous les systèmes** connectés : craft → breed → expédition → économie → prestige → mutations → 7 péchés → Gold/Taboo/Wish.
- **Progression narrative visuelle** : base Arche → Ferme → Usine → Complexe dystopique (§12.2).

### v1.0.1 — Équilibrage

- Coûts, timers, courbes de progression, diminishing returns.
- Soft caps, hard caps, intérêts plafonnés.

### v1.0.2 — Polish UI/UX

- Animations, feedback, transitions, bulles d'action.
- Refonte des écrans de gestion (Gestionnaire v2).

### v1.0.3 — Contenu Additionnel

- Achievements, statistiques globales, milestones cachées.
- Événements ponctuels (festival, météo).

### v1.0.4 — Packaging & Documentation

- Builds, déploiement, guide de démarrage.
- Procédures de test et benchmarks de performance.

---

## Dépendances Entre Versions

```text
v0.1.0 ──► v0.1.1 ──► v0.1.2
   │         │          │
   └─────────┴──────────┘
            │
            ▼
      v0.2.0 ──► v0.2.1 ──► v0.2.2
         │                       │
         └───────────────────────┘
                      │
                      ▼
               v0.3.0 ──► v0.3.1
                  │
                  ▼
           v0.4.0 ──► v0.4.1 ──► v0.4.2
              │                       │
              └───────────────────────┘
                           │
                           ▼
                    v0.5.0 ──► v0.5.1 ──► v0.5.2
                       │                       │
                       └───────────────────────┘
                                    │
                                    ▼
                             v0.6.0 ──► v0.6.1 ──► v0.6.2
                                │                       │
                                └───────────────────────┘
                                             │
                                             ▼
                                      v0.7.0 ──► v0.7.1 ──► v0.7.2
                                         │                       │
                                         └───────────────────────┘
                                                      │
                                                      ▼
                                               v0.8.0 ──► v0.8.1 ──► v0.8.2
                                                  │                       │
                                                  └───────────────────────┘
                                                               │
                                                               ▼
                                                        v0.9.0 ──► v0.9.1 ──► v0.9.2
                                                           │                       │
                                                           └───────────────────────┘
                                                                        │
                                                                        ▼
                                                                 v0.10.0 ──► v0.10.1
                                                                      │          │
                                                                      ▼          ▼
                                                                 v0.10.2 ──► v0.10.3
                                                                      │
                                                                      ▼
                                                               v1.0.0 → v1.0.1 → v1.0.2 → v1.0.3 → v1.0.4
```

---

Dernière mise à jour : Mai 2026
