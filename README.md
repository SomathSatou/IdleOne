# IdleOne

Projet de jeu idle/incremental de breeding de créatures.

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

- **`GDD.md`** — Game Design Document complet (créatures, craft, breeding, expéditions, prestige, économie)
- **`AGENTS.md`** — Configuration et workflow des agents IA
- **`agents/`** — Définitions détaillées de chaque agent
