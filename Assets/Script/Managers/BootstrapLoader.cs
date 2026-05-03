using UnityEngine;

namespace Assets.Script.Managers
{
    /// <summary>
    /// Script de démarrage attaché à un GameObject dans la scène Bootstrap.
    /// Initialise tous les managers singleton (DontDestroyOnLoad) puis
    /// redirige vers MainMenu ou MainUI selon l'état du profil.
    /// GDD §18.1 — Flux de Navigation, §18.5 — Architecture des Scènes.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[BootstrapLoader] Initialisation des managers...");

            EnsureManager<GameManager>("GameManager");
            EnsureManager<SaveManager>("SaveManager");
            EnsureManager<SettingsManager>("SettingsManager");
            EnsureManager<GameSceneManager>("GameSceneManager");

            Debug.Log("[BootstrapLoader] Tous les managers initialisés.");

            // Décider de la scène suivante
            if (GameManager.Instance != null && GameManager.Instance.CurrentProfile != null)
            {
                // Profil déjà chargé (cas rare : retour au bootstrap après un hot-reload)
                Debug.Log("[BootstrapLoader] Profil existant détecté → chargement MainUI.");
                GameSceneManager.Instance.LoadMainUI();
            }
            else
            {
                // Cas normal : aucun profil → menu principal
                Debug.Log("[BootstrapLoader] Aucun profil → chargement MainMenu.");
                GameSceneManager.Instance.LoadMainMenu();
            }
        }

        /// <summary>
        /// Vérifie qu'un singleton de type T existe, sinon le crée.
        /// </summary>
        private void EnsureManager<T>(string name) where T : MonoBehaviour
        {
            if (Object.FindObjectOfType<T>() == null)
            {
                var go = new GameObject(name);
                go.AddComponent<T>();
                Debug.Log($"[BootstrapLoader] {name} créé.");
            }
            else
            {
                Debug.Log($"[BootstrapLoader] {name} déjà présent.");
            }
        }
    }
}
