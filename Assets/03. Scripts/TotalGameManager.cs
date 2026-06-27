using Study.Utilities;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace Babel
{

    public class TotalGameManager : SingletonBase<TotalGameManager>
    {
        public int LootValue { get; private set; }

        public void AddScore(int amount)
        {
            LootValue += amount;
            Debug.Log($"획득한 약탈물 : {LootValue}");
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DontDestroyOnLoad(gameObject);
        }






        


    }


}
