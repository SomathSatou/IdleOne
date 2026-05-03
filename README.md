# IdleOne

Projet de jeu idle/incremental de breeding de créatures.

---

## Setup de l'environnement de travail

### Versions requises

- **Unity** : `6000.4.5f1` (Unity 6, LTS)
- **Pipeline de rendu** : Built-in Render Pipeline (URP désactivé temporairement — voir note technique ci-dessous)
- **IDE recommandés** : Visual Studio 2022 ou JetBrains Rider (package `com.unity.ide.rider` inclus)

### Packages Unity clés

| Package | Version | Rôle |
|---|---|---|
| Input System | `1.19.0` | Gestion des entrées clavier/souris |
| AI Navigation | `2.0.12` | Pathfinding des PNJ et créatures |
| TextMeshPro | `3.0.9` | Rendu texte UI |
| Test Framework | `1.6.0` | Tests unitaires (dédié à l'agent Testeur) |
| Timeline | `1.8.12` | Séquences cinématiques / événements |

### Clone et ouverture du projet

1. **Cloner** le repository :
   ```bash
   git clone <url-du-repo> IdleOne
   ```
2. **Ouvrir** Unity Hub → **Add** → sélectionner le dossier `IdleOne`
3. **Lancer** le projet avec Unity `6000.4.5f1` (Unity Hub téléchargera l'éditeur si nécessaire)
4. **Scènes de démo** disponibles :
   - `Assets/Scenes/DemoBreed.unity` — Système de breeding
   - `Assets/Scenes/DemoSpwanCreature.unity` — Spawn et UI créature

### Bonnes pratiques

- Ne pas modifier le pipeline de rendu : rester en **Built-in** pour éviter l'erreur de compilation `CS8347` liée à URP 17.4.0.
- Pusher les `.meta` : ils sont versionnés dans Git pour éviter les UUID cassées.
- Ouvrir les scenes via le `Project` panel plutôt que la barre de scène par défaut.

---

## Structure du projet

```text
IdleOne/
├── AGENTS.md              ← Configuration des agents IA
├── agents/
│   ├── agent-concepteur.md
│   ├── agent-developpeur.md
│   └── agent-testeur.md
├── GDD.md                 ← Game Design Document
└── README.md              ← Ce fichier
```

---

## Agents IA

Le projet utilise trois agents IA spécialisés (format [AGENTS.md](https://agents.md/)) :

| Agent | Rôle | Ne touche PAS |
|---|---|---|
| **Concepteur** | Documentation, design, specs | Code source |
| **Développeur** | Implémentation des features | GDD / specs |
| **Testeur** | Tests unitaires, procédures de test | Code de production |

Voir `AGENTS.md` pour les détails de chaque agent.

---

## Balises collaboratives

Les collaborateurs peuvent insérer des **balises** directement dans les fichiers de documentation (`.md`) pour communiquer avec l'Agent Concepteur. Chaque balise correspond à un membre de l'équipe — l'agent les traite comme des prompts et propose des améliorations.

### Les 4 balises

| Balise | Auteur | Type | Quand l'utiliser | Exemple |
|---|---|---|---|---|
| `:S:` | **Somath** | Suggestion | Tu as une idée ou une piste à explorer | `:S:` Et si on ajoutait un système de météo qui affecte les expéditions ? |
| `:P:` | **Prozengan** | Problème | Tu identifies un bug, une incohérence ou un trou dans le design | `:P:` La formule de breeding ne permet pas d'avoir des enfants moins bons que les parents |
| `:A:` | **Akoia** | Amélioration | Tu veux qu'une section existante soit enrichie ou restructurée | `:A:` Cette section manque de détails sur les cas limites |
| `:L:` | **Libebulle** | Question | Tu poses une question ou veux clarifier un point | `:L:` Comment est définie l'apparition des boss ? |

### Comment les utiliser

1. **Ouvre** n'importe quel fichier `.md` du projet (GDD.md, docs, etc.)
2. **Écris** ta balise suivie de ton commentaire, directement dans le texte :

```markdown
### 4.2 Phase 2 — Breeding (Gen-1+)

:P: On devrait pouvoir avoir des enfants moins bons que leurs parents
```

3. **L'Agent Concepteur** détectera la balise, analysera le contexte, et proposera une modification documentée.
4. Une fois traitée, la balise est retirée et un commentaire HTML est laissé :

```html
<!-- Traité par Agent Concepteur : ajustement de la formule de breeding pour permettre la régression -->
```

### Bonnes pratiques

- **Une balise = un sujet.** Ne mélange pas plusieurs remarques derrière une seule balise.
- **Sois explicite.** Plus tu donnes de contexte, plus la réponse sera pertinente.
- **Place la balise au bon endroit.** Mets-la dans la section concernée, pas en vrac en fin de fichier.
- **Tout le monde peut en mettre.** Les balises sont faites pour tous les collaborateurs.

### Exemple concret dans le GDD

Le GDD contient déjà une balise active :

```markdown
:P: On devrait pouvoir avoir des enfants moins bons que leurs parents
```

Cette balise dans la section Breeding signale que la formule actuelle (`floor = min(parentA, parentB)`) empêche la régression des stats, ce qui peut poser un problème d'équilibrage.

---

## Documentation

- **`GDD.md`** — Game Design Document complet : créatures, craft & breeding, expéditions & monde top-down, économie & bâtiments, rebirth & prestige, progression par run, les 7 arbres de péchés, direction artistique
- **`AGENTS.md`** — Configuration et workflow des agents IA
- **`agents/`** — Définitions détaillées de chaque agent

---

## Mode Top-Down & Exploration

Au-delà des écrans de gestion, IdleOne propose une couche d'exploration **top-down** qui spatialise l'expérience. Les détails complets sont dans le GDD [§5.6](GDD.md#56-monde-top-down--exploration).

- **Village & Ferme** : le joueur se déplace librement dans un village (place centrale, marché) et sur sa **ferme de créatures** (enclos, pâturages, bâtiments).
- **Caméra & Contrôles** : vue top-down orthographique ou légèrement isométrique, déplacement au clavier (ZQSD / WASD) ou click-to-move.
- **Changement de map** : transitions entre le village, l'intérieur des bâtiments (Maison du Héros, Forge, Laboratoire), et les zones d'expédition.
- **PNJ** : villageois, marchands, quêteurs et visiteurs prestigieux (liés à l'arbre Orgueil). Ils suivent des **routines de déplacement** : patrouilles, horaires jour/nuit, réactions contextuelles.
- **Interactions** : approche d'une machine → touche d'interaction → UI dédiée ; dialogue avec PNJ pour quêtes, commerce et lore ; inspection directe des créatures dans l'enclos.
- **Environnement vivant** : les créatures errent dans leurs enclos avec des comportements simples (faim, sommeil, panique si échappées).

---

## Prototype UI

Un prototype d'interface créature est en cours de développement dans `Assets/Script/UI/` :

- **`CreatureUI.cs`** — Panneau affichant nom, barre de PV et statut de la créature.
- **`CreatureHealthTicker.cs`** — Coroutine infligeant `1 PV/s` pour simuler la dégradation de santé.
- **`DemoCreatureSpawner.cs`** — Spawner de test créant une créature mockée et la bindant à l'UI.
- **Bouton "Nourrir"** — Soigne `+10 PV` par clic. Le statut passe à **"Incapacite"** (rouge) quand les PV atteignent 0, et revient à **"Actif"** (vert) après soin.

Ce prototype valide le système de santé et l'état Incapacité avant intégration aux mécaniques d'expédition et de breeding.

> **Note technique :** Le projet utilise le **Built-in Render Pipeline** (pas URP). URP 17.4.0 provoque une erreur de compilation interne (CS8347 dans `com.unity.render-pipelines.core`). Cette décision est temporaire jusqu'à ce qu'un patch Unity corrige le bug.
