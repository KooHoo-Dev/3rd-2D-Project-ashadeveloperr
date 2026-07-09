using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babel
{
    public class TotalGameManager : SingletonBase<TotalGameManager>
    {
        [Header("씬 설정")]
        [SerializeField] private string bankruptcySceneName = "BankruptcyEnding";

        [SerializeField] private ShipType currentShipType = ShipType.Raft;
        [SerializeField] private int currentStageLevel;

        public bool HasSeenStageTutorialSplash { get; private set; }
        public int PlayerGold { get; private set; } = 10000;
        public ShipType CurrentShipType { get => currentShipType; set => currentShipType = value; }
        public int CurrentStageLevel { get => currentStageLevel; set => currentStageLevel = value; }
        public int CurrentLevelPump { get; set; }
        public int CurrentLevelTimer { get; set; }
        public int CurrentLevelInsurance { get; set; }
        private int MaintenanceCost { get; set; }
        private bool IsBankrupt { get; set; }

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

        public void MarkStageTutorialSplashSeen()
        {
            HasSeenStageTutorialSplash = true;
        }

        public int TotalMaintenanceCost()
        {
            MaintenanceCost = (CurrentLevelTimer * 50) + (CurrentLevelPump * 30) + (CurrentLevelInsurance * 20) + GetShipMaintenanceCost(CurrentShipType);
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
            CurrentShipType = ShipType.Raft;
            CurrentStageLevel = 0;
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
            CurrentShipType = ShipType.Raft;
            CurrentStageLevel = 0;
            CurrentLevelPump = 0;
            CurrentLevelTimer = 0;
            CurrentLevelInsurance = 0;
            MaintenanceCost = 0;
        }

        public void UpgradeShip()
        {
            int current = (int)CurrentShipType;
            current = Mathf.Min(current + 1, (int)ShipType.Cargo);
            CurrentShipType = (ShipType)current;
        }
    }
}
