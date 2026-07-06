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
        [SerializeField] private string bankruptcySceneName = "BankruptcyEnding";

        public bool HasSeenStageTutorialSplash { get; private set; } = false;

        public int PlayerGold { get; private set; } = 10000;

        public ShipType currentShipType = ShipType.Raft;
        public int CurrentLevelPump { get; set; } = 0;
        public int CurrentLevelTimer { get; set; } = 0;
        public int CurrentLevelInsurance { get; set; } = 0;

        public int MaintenanceCost { get; private set; }

        public int currentStageLevel = 0;
        public bool IsBankrupt { get; private set; }

        public void MarkStageTutorialSplashSeen()
        {
            HasSeenStageTutorialSplash = true;
        }

        public enum ShipType
        {
            Raft,
            Canoe,
            Boat,
            Cargo,
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public int TotalMaintenanceCost()
        {
            MaintenanceCost = (CurrentLevelTimer * 50) + (CurrentLevelPump * 30) + (CurrentLevelInsurance * 20) + GetShipMaintenanceCost(currentShipType);
            return MaintenanceCost;
        }

        public bool AddGold(int amount)
        {
            PlayerGold += amount;

            if (PlayerGold < 0 && amount < 0)
            {
                GoToBankruptcyEnding();
                return false;
            }

            return true;
        }

        private void GoToBankruptcyEnding()
        {
            if (IsBankrupt)
            {
                return;
            }

            IsBankrupt = true;
            SceneManager.LoadScene(bankruptcySceneName);
        }

        public void ResetAllForNewGame()
        {
            PlayerGold = 0;

            currentShipType = ShipType.Raft;

            currentStageLevel = 0;

            CurrentLevelPump = 0;
            CurrentLevelTimer = 0;
            CurrentLevelInsurance = 0;

            MaintenanceCost = 0;

            IsBankrupt = false;
        }

        public int GetShipMaintenanceCost(ShipType type)
        {
            return type switch
            {
                ShipType.Raft => 0,
                ShipType.Canoe => 200,
                ShipType.Boat => 400,
                ShipType.Cargo => 600,
                _ => 0
            };
        }

        public void ResetProgressAfterShipwreck()
        {
            currentShipType = ShipType.Raft;

            currentStageLevel = 0;

            CurrentLevelPump = 0;
            CurrentLevelTimer = 0;
            CurrentLevelInsurance = 0;

            MaintenanceCost = 0;
        }

        public void UpgradeShip()
        {
            int current = (int)currentShipType;
            current = Mathf.Min(current + 1, (int)ShipType.Cargo);
            currentShipType = (ShipType)current;
        }

    }


}
