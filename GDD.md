# Game Design Document — Idle Breeding Game
## Projet Akoia × Prozengan

**Version :** 0.1 — Conception initiale
**Date :** 26/04/2026
**Auteurs :** TS, Akoia, Prozengan

---

## Table des matières

1. [Vision du Projet](#1-vision-du-projet)
2. [Core Loop](#2-core-loop)
3. [Système de Créatures](#3-système-de-créatures)
4. [Craft & Breeding](#4-craft--breeding)
5. [Expéditions & Zones](#5-expéditions--zones)
6. [Rebirth & Prestige](#6-rebirth--prestige)
7. [Économie & Ressources](#7-économie--ressources)
8. [Progression par Run](#8-progression-par-run)
9. [Stack Technique](#9-stack-technique)
10. [Place de l'IA](#10-place-de-lia)
11. [Questions Ouvertes](#11-questions-ouvertes)

---

## 1. Vision du Projet

### Pitch
Un **jeu idle/incremental de breeding de créatures**, mêlant les mécaniques de Koletera II et Trimm, avec une inspiration Pokémon, dans un univers original.

### Références
- **Pokémon** — breeding, collection, types
- **Shapez** — formes/couleurs comme blocs de construction visuelle
- **Cookie Clicker / PokéClicker** — idle/click mécanique
- **Koletera II / Trimm** — systèmes propres au groupe

### Pilliers de Design
1. **Craft déterministe** pour apprendre, **breed aléatoire** pour optimiser
2. **Progression tangible** : chaque rebirth rend le joueur fondamentalement plus fort
3. **Exploration** : grilles de zones à découvrir case par case
4. **Visualisation claire** : hexagone de stats, prédiction de breeding

---

## 2. Core Loop

```
[Clic/Idle] → Ressources (Formes, Couleurs, Matériaux)
     │
     ├── CRAFT → Créature Gen-0
     │              │
     │              ├── DÉPLOIEMENT → Expédition (grille) → + Ressources
     │              │                                        + Squelettes rares
     │              │                                        + Nouvelles Couleurs
     │              │
     │              └── BREED avec une autre créature
     │                         │
     │                         ▼
     │                  Créature Gen-N (meilleure)
     │                         │
     │                         ├── DÉPLOIEMENT → Meilleurs rewards
     │                         └── BREED → Gen-N+1 ...
     │
     └── UPGRADE bâtiments / taux idle

     ═══════════════════════════════
     REBIRTH : reset → Essence → upgrades permanents
     ═══════════════════════════════
```

### Boucle minute par minute
1. Le joueur **clique** ou **attend** → accumule des ressources
2. Il **craft** ou **breed** une créature
3. Il **envoie** la créature explorer une zone (grille)
4. L'expédition rapporte des **rewards** (ressources, déblocages)
5. Il **améliore** ses bâtiments et ses créatures
6. Retour en 1 avec plus de puissance

---

## 3. Système de Créatures

### 3.1 Les 6 Stats (Hexagone)

Chaque créature possède exactement 6 stats, affichées en radar chart hexagonal.

```
              Volonté
             /       \
       Force           Chance
        |       ⬡       |
  Constitution        Intelligence
             \       /
              Agilité
```

| Stat | Abrév. | Rôle principal | Rôle en expédition |
|---|---|---|---|
| **Force (FOR)** | STR | Dégâts physiques, capacité de charge | Casse obstacles, cases combat |
| **Agilité (AGI)** | AGI | Vitesse, esquive | Réduit le timer d'exploration par case |
| **Intelligence (INT)** | INT | Dégâts magiques, efficacité spécialisée | Révèle cases adjacentes (+vision) |
| **Chance (CHA)** | LCK | Drops rares, crits, mutations | Meilleurs drops, cases cachées bonus |
| **Constitution (CON)** | CON | PV, endurance | Explore plusieurs cases d'affilée |
| **Volonté (VOL)** | WIL | Résistance, bonus idle | Continue d'explorer offline |

### 3.2 Composants d'une Créature

| Composant | Rôle |
|---|---|
| **Squelette** | Type de base (bipède, quadrupède, volant, serpentin…). Détermine les stats de base et l'apparence. |
| **Forme** | Modifie l'apparence et une stat principale (rond → +CON, triangle → +FOR, etc.) |
| **Couleur** | Modifie l'affinité élémentaire et une stat secondaire (rouge → Feu/+FOR, bleu → Eau/+INT, etc.) |
| **Génération** | Gen-0 = crafté, Gen-1+ = issu du breeding. Potentiel augmente avec la génération. |

### 3.3 Affinités Formes → Stats

| Forme | Stat boostée | Description |
|---|---|---|
| Rond | +CON | Solide, endurant |
| Triangle | +FOR | Pointu, offensif |
| Carré | +VOL | Stable, résistant |
| Ovale | +AGI | Fluide, rapide |
| Losange | +INT | Complexe, intelligent |
| Étoile | +CHA | Rare, chanceux (mutation uniquement) |

### 3.4 Affinités Couleurs → Stats / Éléments

| Couleur | Élément | Stat boostée |
|---|---|---|
| Rouge | Feu | +FOR |
| Bleu | Eau | +INT |
| Vert | Nature | +CON |
| Jaune | Foudre | +AGI |
| Violet | Ombre | +CHA |
| Orange | Terre | +VOL |
| Doré | Lumière | +CHA (mutation) |
| Arc-en-ciel | Chaos | +ALL (mutation rare) |

---

## 4. Craft & Breeding

### 4.1 Phase 1 — Craft (Gen-0)

```
Créature Gen-0 = Squelette + Forme + Couleur
```

**Résultat 100% déterministe.** Le joueur sait exactement ce qu'il obtient.

**Formule :**
```
Stat[i] = base_squelette[i] + bonus_forme[i] + bonus_couleur[i]
```

### 4.2 Phase 2 — Breeding (Gen-1+)

Deux créatures parentes → un enfant avec traits hérités + potentiel de mutation.

**Formule :**
```
base_min   = min(parentA[stat], parentB[stat])
base_max   = max(parentA[stat], parentB[stat])
variance   = (base_max - base_min) * variance_factor   // variance_factor = 0.25 par défaut
floor      = base_min - variance + breeding_min_bonus (prestige)
ceil       = base_max + variance + breeding_max_bonus (prestige)
Stat_enfant = random(floor, ceil) + bonus_generation + mutation_roll
```

> **Règle de régression :** Un enfant **peut** avoir des stats inférieures à ses parents. Le `variance_factor` (par défaut 0.25) étend la fourchette sous le minimum parental. Cela signifie qu'un breeding n'est jamais garanti — le joueur doit faire plusieurs tentatives pour optimiser. Le `floor` ne peut jamais descendre en dessous de 1 (plancher absolu).

<!-- Traité par Agent Concepteur : ajout du variance_factor pour permettre la régression des stats en breeding, le floor peut descendre sous le min parental -->

### 4.3 Héritage des Composants

| Composant | Règle d'héritage |
|---|---|
| Squelette | 50/50 d'un parent ou de l'autre |
| Forme | Dominance : certaines formes sont dominantes sur d'autres |
| Couleur | Mélange possible (rouge + bleu → violet) ou héritage pur |

### 4.4 Mutations

- **Débloquées après le 1er rebirth** (achat prestige)
- **Probabilité :** ~5% par breeding (augmentable via prestige/chance)
- **Effets :** Forme ou couleur inédite (Étoile, Doré, Arc-en-ciel), stat qui dépasse le plafond parental

### 4.5 Prédiction de Breeding (UI)

Affichage avant confirmation :
- Hexagone Parent A (bleu)
- Hexagone Parent B (rouge)
- Zone de prédiction enfant (vert semi-transparent = fourchette min/max)

Le joueur voit la **zone probable** et fait un choix éclairé.

---

## 5. Expéditions & Zones

### 5.1 Structure d'une Zone

Chaque zone est une **grille carrée** avec fog of war.

```
  ░ ?? │ ?? │ ?? │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ 👁 │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ 👁 │ 🏠 │ 👁 │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ 👁 │ ?? │ ?? ░
  ░────┼────┼────┼────┼────░
  ░ ?? │ ?? │ ?? │ ?? │ ?? ░

  🏠 = Case de départ (révélée)
  👁 = Cases adjacentes (visibles mais non explorées)
  ?? = Cases masquées (fog of war)
```

### 5.2 Types de Cases

| Type | Icône | Contenu |
|---|---|---|
| **Ressource** | 💎 | Drop de formes, couleurs, matériaux |
| **Combat** | ⚔️ | Affrontement → rewards si victoire (FOR, AGI importants) |
| **Événement** | ❓ | Choix narratif, bonus ou malus aléatoire |
| **Boss** | 💀 | Combat difficile, gros rewards, peut débloquer nouveau squelette |
| **Vide** | ⬜ | Rien de spécial, juste de la progression de carte |
| **Cachée** | 🌟 | Invisible, trouvée uniquement avec haute CHA |

### 5.3 Mécanique d'Exploration

1. Le joueur sélectionne une **créature** et une **case adjacente révélée**
2. Timer idle commence (durée basée sur la difficulté de la case - AGI de la créature)
3. À la fin du timer : case explorée, reward obtenu, cases adjacentes révélées
4. Une créature avec haute CON peut enchaîner plusieurs cases sans repos
5. Une créature avec haute VOL continue d'explorer même quand le joueur est offline
6. Une créature avec haute INT révèle +1 case supplémentaire autour

### 5.4 Scaling des Zones

| Run | Nombre de zones | Taille grille | Notes |
|---|---|---|---|
| Run 1 | **8** zones | 5×5 → 7×7 | Tutoriel, 1 boss simple, basique |
| Run 2 | **12** zones | 6×6 → 9×9 | Nouvelles mécaniques de cases |
| Run 3 | **16** zones | 7×7 → 11×11 | Cases spéciales, événements rares |
| Run N | **8 + 4×(N-1)** zones | Scaling | Profondeur croissante |

### 5.5 Fin de Zone

Une zone est complétée quand :
- Le boss est vaincu, OU
- Toutes les cases sont explorées (si pas de boss)

Compléter une zone débloque la suivante.

---

## 6. Rebirth & Prestige

### 6.1 Déclenchement

Le joueur peut Rebirth à tout moment, mais le jeu le **suggère** quand la progression stagne (expéditions trop lentes, pas assez de stats pour avancer).

### 6.2 Monnaie de Prestige : Essence ✨

```
Essence gagnée = f(zones_explorées, créatures_créées, gen_max, temps_de_run)
```

Plus le run est long et poussé, plus on gagne d'Essence.

### 6.3 Ce qu'on RESET

- Toutes les créatures
- Toutes les ressources (formes, couleurs, matériaux)
- Progression des zones (fog of war remis)
- Niveaux de bâtiments

### 6.4 Ce qu'on GARDE

- Essence accumulée (cumulatif entre rebirth)
- Tous les upgrades achetés dans la boutique prestige
- Squelettes / formes / couleurs débloqués
- Statistiques et achievements

### 6.5 Boutique de Prestige

| Upgrade | Coût (Essence) | Effet | Stackable |
|---|---|---|---|
| **Breeding Min+** | 10 / 25 / 50 / 100 / ... | +1 au minimum de la fourchette de breeding par level | ✅ |
| **Breeding Max+** | 10 / 25 / 50 / 100 / ... | +1 au maximum de la fourchette de breeding par level | ✅ |
| **Stats Craft+** | 15 / 35 / 70 / 140 / ... | +1 à toutes les stats des créatures Gen-0 craftées | ✅ |
| **Slot Créature+** | 20 par slot | +1 slot de créature actif | ✅ |
| **Nouveau Squelette** | 50 / 100 / 200 / ... | Débloque un nouveau type de squelette | Limité |
| **Pack Formes** | 30 | Débloque 2 nouvelles formes | Limité |
| **Pack Couleurs** | 30 | Débloque 2 nouvelles couleurs | Limité |
| **Mutation Unlock** | 75 | Active le système de mutations en breeding | 1× |
| **Mutation Rate+** | 40 / 80 / 160 | +2% chance de mutation | ✅ |
| **Vision+** | 40 | +1 case de vision de base en expédition | ✅ |
| **Idle Speed+** | 20 / 45 / 90 / ... | Production idle de base augmentée | ✅ |

---

## 7. Économie & Ressources

### 7.1 Ressources de Base

| Ressource | Source | Utilisation |
|---|---|---|
| **Fragments** | Clic + idle | Monnaie de base, achats divers |
| **Formes** (items) | Expéditions, drops | Composant de craft |
| **Couleurs** (items) | Expéditions, drops | Composant de craft |
| **Matériaux** | Expéditions, mining | Upgrades de bâtiments |
| **Essence** ✨ | Rebirth uniquement | Boutique prestige |

### 7.2 Coûts (première ébauche — à équilibrer)

| Action | Coût |
|---|---|
| Craft Gen-0 | 1 Squelette + 1 Forme + 1 Couleur + X Fragments |
| Breeding | 2 créatures parentes (immobilisées pendant le timer) + Y Fragments |
| Envoyer en expédition | Gratuit (mais la créature est occupée) |
| Upgrade bâtiment Lv. N | N × base_cost Matériaux |

### 7.3 Timers (première ébauche)

| Action | Durée de base |
|---|---|
| Exploration d'une case | 30s → 5min (selon difficulté) |
| Breeding | 2min → 10min (selon génération) |
| Repos d'une créature | 1min (réduit par CON) |

---

## 8. Progression par Run

### Run 1 — Le Squelette de Run (Tutoriel naturel)

| Étape | Action | Apprentissage |
|---|---|---|
| 1 | Clic → fragments | Mécanique idle/clic |
| 2 | Craft 1ère créature (1 squelette, 3 formes, 3 couleurs) | Système de craft |
| 3 | Envoyer en expédition | Système d'expédition + grille |
| 4 | Crafter 3-5 créatures | Explorer les combinaisons |
| 5 | Premier breeding | Héritage, prédiction hexagone |
| 6 | Explorer les 8 zones | Montée en puissance |
| 7 | Stagnation → le jeu propose le Rebirth | Mécanique prestige |

### Contenu Disponible par Run

| Élément | Run 1 | Run 2 | Run 3+ |
|---|---|---|---|
| Squelettes | 1 | 2 | 3+ |
| Formes | 3 (rond, triangle, carré) | 5 | 7+ |
| Couleurs | 3 (rouge, bleu, vert) | 5 | 7+ |
| Zones | 8 | 12 | 16+ |
| Slots créatures | 5 | 5 + achats | 5 + achats |
| Mutations | ❌ Non | ✅ Oui (si acheté) | ✅ Oui |

---

## 9. Stack Technique

**Décision différée** — dépend du format final (web, desktop, mobile) et du style visuel (2D ou 3D).

| Option | Front | Back | Rendu | Hébergement | Plateformes |
|---|---|---|---|---|---|
| **A — Full Web** | React + TypeScript | Django (Python) | 2D Canvas/SVG | Automia / Vercel | Web |
| **B — Unity 2D** | Unity UI | C# | 2D Sprites/Tilemap | Desktop / Mobile / WebGL | Desktop, Mobile, Web |
| **C — Unity 3D** | Unity UI | C# | 3D | Desktop / Mobile / WebGL | Desktop, Mobile, Web |
| **D — Hybride** | React + TS | Django | Unity WebGL | Automia | Web |

### Critères de choix
- Si web only, prototypage rapide → Option A
- Si 2D avec déploiement multi-plateforme (desktop + mobile + web) → Option B
- Si 3D important → Option C
- Si web + moteur de jeu intégré → Option D (plus complexe)

> **Note :** Unity supporte nativement le rendu 2D (sprites, tilemaps, animations 2D) avec les mêmes avantages multi-plateforme que le 3D. Pour un jeu idle avec hexagones de stats et grilles d'exploration, l'option Unity 2D (B) offre un bon compromis entre richesse visuelle et portabilité.

<!-- Traité par Agent Concepteur : correction du tableau stack — Unity supporte aussi le 2D, ajout de l'option B Unity 2D et colonne Plateformes -->

---

## 10. Place de l'IA

| Phase | IA ? | Justification |
|---|---|---|
| Conception / GDD | ✅ Oui | Accélérateur pour structurer et itérer |
| Code / Développement | ✅ Oui | Génération, refacto, tests |
| Illustration in-game | ❌ Non | Art fait main préféré |
| Équilibrage / Simulation | ❓ À discuter | Simuler des runs pour tester les courbes |

---

## 11. Questions Ouvertes

- [ ] **Lore** : Quel est le monde ? Qui sont Akoia et Prozengan dans l'univers ? Y a-t-il une histoire ?
- [ ] **Nom du jeu**
- [ ] **Multijoueur** : Solo only ou composante sociale (échanges, classements) ?
- [ ] **Monétisation** : Free-to-play ? Premium ? Cosmétiques ?
- [ ] **Plateforme cible** : Web ? Desktop ? Mobile ? Les trois ?
- [ ] **Nombre total de squelettes** prévu à terme
- [ ] **Système d'évolution** : Les créatures peuvent-elles évoluer (en plus du breeding) ?
- [ ] **PvP / Arène** : Les créatures peuvent-elles s'affronter entre joueurs ?
- [ ] **Saisonnalité** : Événements temporaires, créatures limitées ?


## 12 . Discution cours a ajouter dans le GDD

:S: je suis pour un estétique Kawaii et un monde ultra dark, avec un coté arche de noé pour la base du joueur au début mais qui finis par exploité les créatures commes les autres, chaque prestige ou amélioration majeur transformerais de plus en plus certaines partie de la base en usine agro alimentaire par exemple pour feed nos créatures les plus fort on fait de la bouilli des plus faibles, un peut dans l'esprit cult of the lamb.

:A: Je suis ok avec cette idée

:L: je pense qu'il faudrais faire des nains qui chasses des dragon et des licornes

---

*Ce document est vivant. Il sera mis à jour au fur et à mesure de la conception.*
