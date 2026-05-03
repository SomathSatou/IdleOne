using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script.Managers
{
    /// <summary>
    /// Singleton gérant les transitions entre scènes.
    /// Toutes les navigations doivent passer par ce manager.
    /// GDD §18.1 — Flux de Navigation, §18.5 — Architecture des Scènes.
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        // ==========================
        //  Singleton
        // ==========================

        /// <summary>Instance unique du GameSceneManager, accessible globalement.</summary>
        public static GameSceneManager Instance { get; private set; }

        // ==========================
        //  Constantes — Noms de scènes
        // ==========================

        /// <summary>Scène de démarrage qui initialise tous les managers.</summary>
        public const string SCENE_BOOTSTRAP = "Bootstrap";

        /// <summary>Scène du menu principal (nouvelle partie, charger, paramètres, quitter).</summary>
        public const string SCENE_MAIN_MENU = "MainMenu";

        /// <summary>Scène principale du jeu avec les onglets (créatures, breeding, inventaire, stats).</summary>
        public const string SCENE_MAIN_UI = "MainUI";

        // ==========================
        //  Propriétés
        // ==========================

        /// <summary>Nom de la scène actuellement chargée.</summary>
        public string CurrentScene => SceneManager.GetActiveScene().name;

        // ==========================
        //  Cycle de vie Unity
        // ==========================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ==========================
        //  Navigation — GDD §18.1
        // ==========================

        /// <summary>
        /// Charge la scène Bootstrap (réinitialisation complète).
        /// GDD §18.5 — Architecture des Scènes.
        /// </summary>
        public void LoadBootstrap()
        {
            Debug.Log("[GameSceneManager] Chargement de Bootstrap...");
            SceneManager.LoadScene(SCENE_BOOTSTRAP);
        }

        /// <summary>
        /// Charge le menu principal.
        /// GDD §18.2 — Menu Principal.
        /// </summary>
        public void LoadMainMenu()
        {
            Debug.Log("[GameSceneManager] Chargement de MainMenu...");
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        /// <summary>
        /// Charge la scène principale du jeu (MainUI avec onglets).
        /// GDD §18.5 — Architecture des Scènes.
        /// </summary>
        public void LoadMainUI()
        {
            Debug.Log("[GameSceneManager] Chargement de MainUI...");
            SceneManager.LoadScene(SCENE_MAIN_UI);
        }
    }
}
