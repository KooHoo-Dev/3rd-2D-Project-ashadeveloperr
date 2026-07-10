using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babel
{
    public class TotalGameManager : SingletonBase<TotalGameManager>
    {
        [Header("씬 설정")]
        [SerializeField] private string bankruptcySceneName = "BankruptcyEnding";

        [Header("게임 모드")]
        [SerializeField] private GameMode currentGameMode = GameMode.Story;
        [SerializeField] private bool isEndlessUnlocked;
        [SerializeField] private int finalStoryStageLevel = 5;

        [Header("진행 정보")]
        [SerializeField] private ShipType currentShipType = ShipType.Raft;
        [SerializeField] private int currentStageLevel = 1;

        private readonly HashSet<string> seenDialogueSceneIds = new HashSet<string>();

        public bool HasSeenStageTutorialSplash { get; private set; }
        public int PlayerGold { get; private set; } = 10000;
        public GameMode CurrentGameMode => currentGameMode;
        public bool IsStoryMode => currentGameMode == GameMode.Story;
        public bool IsEndlessMode => currentGameMode == GameMode.Endless;
        public bool IsEndlessUnlocked => isEndlessUnlocked;
        public int FinalStoryStageLevel => finalStoryStageLevel;
        public ShipType CurrentShipType { get => currentShipType; set => currentShipType = value; }
        public int CurrentStageLevel { get => currentStageLevel; set => currentStageLevel = Mathf.Max(1, value); }
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

        public enum GameMode
        {
            Story,
            Endless,
        }

        protected override void Awake()
        {
            base.Awake();

            if (instance != this)
            {
                return;
            }

            DontDestroyOnLoad(gameObject);

            if (currentStageLevel < 1)
            {
                currentStageLevel = 1;
            }
        }

        public void StartStoryMode()
        {
            currentGameMode = GameMode.Story;
            ResetAllForNewGame(false);
        }

        public bool StartEndlessMode()
        {
            if (isEndlessUnlocked == false)
            {
                Debug.Log("[TotalGameManager] 엔드리스 모드는 아직 해금되지 않았습니다.");
                return false;
            }

            currentGameMode = GameMode.Endless;
            ResetAllForNewGame(true);
            return true;
        }

        public void CompleteStoryModeAndPrepareEndless()
        {
            isEndlessUnlocked = true;
            currentGameMode = GameMode.Endless;
            ResetProgressForModeTransition();
        }

        public void MarkStageTutorialSplashSeen()
        {
            HasSeenStageTutorialSplash = true;
        }

        public bool HasSeenDialogue(string dialogueSceneId)
        {
            return string.IsNullOrWhiteSpace(dialogueSceneId) || seenDialogueSceneIds.Contains(dialogueSceneId);
        }

        public void MarkDialogueSeen(string dialogueSceneId)
        {
            if (string.IsNullOrWhiteSpace(dialogueSceneId))
            {
                return;
            }

            seenDialogueSceneIds.Add(dialogueSceneId);
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
            ResetAllForNewGame(isEndlessUnlocked);
        }

        private void ResetAllForNewGame(bool keepEndlessUnlocked)
        {
            PlayerGold = 0;
            CurrentShipType = ShipType.Raft;
            CurrentStageLevel = 1;
            CurrentLevelPump = 0;
            CurrentLevelTimer = 0;
            CurrentLevelInsurance = 0;
            MaintenanceCost = 0;
            IsBankrupt = false;
            HasSeenStageTutorialSplash = false;
            seenDialogueSceneIds.Clear();

            if (keepEndlessUnlocked == false)
            {
                isEndlessUnlocked = false;
            }
        }

        private void ResetProgressForModeTransition()
        {
            PlayerGold = 0;
            CurrentShipType = ShipType.Raft;
            CurrentStageLevel = 1;
            CurrentLevelPump = 0;
            CurrentLevelTimer = 0;
            CurrentLevelInsurance = 0;
            MaintenanceCost = 0;
            IsBankrupt = false;
            HasSeenStageTutorialSplash = true;
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
            CurrentStageLevel = 1;
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
