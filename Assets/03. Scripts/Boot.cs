using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babel
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}