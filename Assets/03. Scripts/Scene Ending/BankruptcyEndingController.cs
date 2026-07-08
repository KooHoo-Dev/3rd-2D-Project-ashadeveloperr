using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babel
{
    public class BankruptcyEndingController : MonoBehaviour
    {
        [Header("이동할 씬")]
        [SerializeField] private string mainMenuSceneName = "Main Menu";

        private void Update()
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                TotalGameManager.Instance.ResetAllForNewGame();
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
