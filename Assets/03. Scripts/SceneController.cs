using Study.Utilities;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Babel
{

    public class SceneController : MonoBehaviour
    {

        public enum GameScene
        {
            MainMenu = 0,
            Stage,
        }

        private GameScene PrevScene { get; set; } = GameScene.MainMenu;
        private GameScene CurrentScene { get; set; } = GameScene.MainMenu;

        public void ChangeScene(int sceneIndex)
        {
            ChangeScene((GameScene)sceneIndex);
        }

        public void ChangeScene(GameScene targetScene)
        {
            PrevScene = CurrentScene;
            CurrentScene = targetScene;
            SceneManager.LoadScene((int)CurrentScene);
        }

        public void MovePrevScene()
        {
            ChangeScene(PrevScene);
        }




    }


}
