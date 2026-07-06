using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Babel.TotalGameManager;
using static UnityEngine.GraphicsBuffer;

namespace Babel
{
    public class GameManager : SingletonBase<GameManager>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        #region Refs and Var;

        [SerializeField] private Image gaugeFillImage;
        [SerializeField] private UIManager UIManager;
        [SerializeField] private TMP_Text lootValueText;
        [SerializeField] private TMP_Text insuranceTitleText;
        [SerializeField] private TMP_Text insuranceValueText;
        [SerializeField] private TMP_Text resultLootValueText;
        [SerializeField] private TMP_Text resultCostText;
        [SerializeField] private TMP_Text resultClosingText;

        [SerializeField] private Block[] BlockPrefabs;
        [SerializeField] private int maxIndexInQueue;

        [SerializeField] private GameObject ggScreen;

        public float sinkPoint;
        public bool isGameOver;
        public bool isFailed;

        private Queue<int> blockIndexQueue = new Queue<int>();

        private Coroutine countCoroutine;
        private int currentDisplayedLoot = 0;
        private bool hasGameOverProcessed;

        [Header("게임 오버 타이머")]

        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text stageText;

        public float RemainingTime { get; private set; }

        #endregion

        #region User Var;
        private float pumpPower;

        public int InStageLootValue { get; private set; }

        [Header("배 세팅")]
        [SerializeField] private ShipPrefabEntry[] shipPrefabEntries;
        [SerializeField] private Transform shipSpawnPoint;
        private Transform shipParent;

        [System.Serializable]
        public class ShipPrefabEntry
        {
            public ShipType shipType;
            public Ship prefab;
        }

        public Ship ship;

        [Header("바람 세팅")]
        [SerializeField] private BackgroundManager backgroundManager;

        [SerializeField] private float maxWindSpeedPerStage = 1.5f;
        [SerializeField] private float windChangeRatio = 0.1f;

        public float CurrentWindSpeed { get; private set; }
        public float CurrentMaxWindSpeed { get; private set; }


        #endregion

        #region Unity Methods;

        protected override void Awake()
        {
            SpawnShipByCurrentType();
            InitializeBlockQueue();
        }

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
            UpdateLootValue(InStageLootValue);

            pumpPower = 1.5f + (TotalGameManager.Instance.CurrentLevelPump * 1.0f);
            RemainingTime = 20 + (TotalGameManager.Instance.CurrentLevelTimer * 3);

            if (backgroundManager == null)
            {
                backgroundManager = FindAnyObjectByType<BackgroundManager>();
            }

            if (UIManager == null)
            {
                UIManager = FindAnyObjectByType<UIManager>();
            }

            UpdateTimerDisplay();
            UpdateStageDisplay();

            InitializeWind();
        }

        private void FixedUpdate()
        {
            if (IsTutorialSplashBlockingGame())
            {
                return;
            }

            IsDeckSinking();
        }

        private void Update()
        {
            if (isGameOver && isFailed)
            {
                GameOverEffect();
            }

            UpdateGauge();

            if (IsTutorialSplashBlockingGame())
            {
                return;
            }

            if (isGameOver == false && RemainingTime > 0)
            {
                RemainingTime -= Time.deltaTime;

                if (RemainingTime <= 0)
                {
                    RemainingTime = 0;
                    isGameOver = true;
                    isFailed = false;
                    GameOver();
                }

                UpdateTimerDisplay();
            }
        }

        #endregion



        #region Custom Methods;

        private bool IsTutorialSplashBlockingGame()
        {
            return UIManager != null && UIManager.IsTutorialSplashActive;
        }

        private void InitializeWind()
        {
            CurrentMaxWindSpeed = GetCurrentMaxWindSpeed();
            CurrentWindSpeed = Random.Range(-CurrentMaxWindSpeed, CurrentMaxWindSpeed);

            ApplyWindState();
        }

        private float GetCurrentMaxWindSpeed()
        {
            int displayStage = Mathf.Max(TotalGameManager.Instance.currentStageLevel + 1, 1);
            return displayStage * maxWindSpeedPerStage;
        }

        public void ChangeWindForNextThrow()
        {
            CurrentMaxWindSpeed = GetCurrentMaxWindSpeed();

            float maxDelta = CurrentMaxWindSpeed * windChangeRatio;
            float delta = Random.Range(-maxDelta, maxDelta);

            CurrentWindSpeed += delta;
            CurrentWindSpeed = Mathf.Clamp(CurrentWindSpeed, -CurrentMaxWindSpeed, CurrentMaxWindSpeed);

            ApplyWindState();
        }

        private void ApplyWindState()
        {
            if (backgroundManager != null)
            {
                backgroundManager.SetWind(CurrentWindSpeed);
            }

            if (UIManager != null)
            {
                UIManager.UpdateWindDisplay(CurrentWindSpeed);
            }
        }

        private void UpdateStageDisplay()
        {
            if (stageText == null)
            {
                return;
            }

            int displayStage = TotalGameManager.Instance.currentStageLevel + 1;
            stageText.SetText($"Stage {displayStage:00}");
        }

        private void SpawnShipByCurrentType()
        {
            ShipType currentType = TotalGameManager.Instance.currentShipType;
            Ship prefab = GetShipPrefab(currentType);

            Vector3 spawnPosition = shipSpawnPoint != null ? shipSpawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = shipSpawnPoint != null ? shipSpawnPoint.rotation : Quaternion.identity;

            ship = Instantiate(prefab, spawnPosition, spawnRotation, shipParent);

            UIManager uiManager = FindAnyObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ship = ship;
            }
        }

        private Ship GetShipPrefab(ShipType shipType)
        {
            foreach (ShipPrefabEntry entry in shipPrefabEntries)
            {
                if (entry.shipType == shipType)
                {
                    return entry.prefab;
                }
            }

            return null;
        }

        private void InitializeBlockQueue()
        {
            int stageTargetIndex = Mathf.Max(TotalGameManager.Instance.currentStageLevel, 0);
            maxIndexInQueue = Mathf.Min(stageTargetIndex, BlockPrefabs.Length);

            if (maxIndexInQueue <= 0)
            {
                maxIndexInQueue = 1;
            }

            blockIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));
        }


        private void UpdateTimerDisplay()
        {
            int totalSeconds = Mathf.CeilToInt(RemainingTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }


        private void UpdateLootValue(int targetLoot)
        {
            if (countCoroutine != null) StopCoroutine(countCoroutine);
            countCoroutine = StartCoroutine(CountLootRoutine(targetLoot));
        }

        private IEnumerator CountLootRoutine(int targetLoot)
        {
            while (currentDisplayedLoot != targetLoot)
            {
                if (currentDisplayedLoot < targetLoot)
                    currentDisplayedLoot++;
                else
                    currentDisplayedLoot--;

                lootValueText.SetText($"{currentDisplayedLoot}");

                yield return new WaitForSeconds(0.003f);
            }
            countCoroutine = null;
        }

        private void IsDeckSinking()
        {
            if (ship.deckLeft.position.y <= 0 || ship.deckCenter.position.y <= 0 || ship.deckRight.position.y <= 0)
            {
                if (ship.deckLeft.position.y <= 0)
                {
                    SinkingEffect(ship.deckLeft);
                }
                if (ship.deckCenter.position.y <= 0)
                {
                    SinkingEffect(ship.deckCenter);
                }
                if (ship.deckRight.position.y <= 0)
                {
                    SinkingEffect(ship.deckRight);
                }
            }
            else
            {
                SinkingEffect(null);
            }
        }

        private void SinkingEffect(Transform deck)
        {
            if (isGameOver == false)
            {
                if (deck == null)
                {
                    if (sinkPoint > 0)
                    {
                        sinkPoint -= pumpPower * Time.deltaTime;
                    }
                    else sinkPoint = 0;

                }
                else
                {
                    if (sinkPoint < 100)
                    {
                        sinkPoint += Mathf.Abs(deck.position.y) * Time.deltaTime * 10f - (pumpPower / 100);
                    }
                    else
                    {
                        isGameOver = true;
                        isFailed = true;
                        GameOver();
                    }
                }
            }
        }

        private void UpdateGauge()
        {
            gaugeFillImage.fillAmount = sinkPoint / 100;
        }

        public Block GetNextBlock()
        {
            int dequeueIndex = blockIndexQueue.Dequeue();
            blockIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));

            Block block = GetBlock(dequeueIndex);
            return block;
        }

        private Block GetBlock(int index)
        {
            if (index < 0 || BlockPrefabs.Length <= index) return BlockPrefabs[0];
            return BlockPrefabs[index];
        }

        [Header("GameOver Settings")]
        public GameObject[] enableObjects;
        public GameObject[] disableObjects;

        public void GameOver()
        {
            if (hasGameOverProcessed)
            {
                return;
            }

            hasGameOverProcessed = true;
            isGameOver = true;

            int maintenanceCost = TotalGameManager.Instance.TotalMaintenanceCost();

            if (isFailed)
            {
                InStageLootValue = 0;
            }

            OnLootChanged?.Invoke(InStageLootValue);
            resultLootValueText.SetText($"+{InStageLootValue}");
            resultCostText.SetText($"-{maintenanceCost}");

            int baseClosing = InStageLootValue - maintenanceCost;

            if (isFailed == false && baseClosing > 0)
            {
                TotalGameManager.Instance.currentStageLevel++;
            }

            insuranceTitleText.gameObject.SetActive(false);
            insuranceValueText.gameObject.SetActive(false);

            int totalClosing;

            if (baseClosing < 0 && TotalGameManager.Instance.CurrentLevelInsurance > 0)
            {
                insuranceTitleText.gameObject.SetActive(true);
                insuranceValueText.gameObject.SetActive(true);

                int divisor = TotalGameManager.Instance.CurrentLevelInsurance + 1;
                totalClosing = baseClosing / divisor;

                int savedAmount = Mathf.Abs(baseClosing - totalClosing);
                insuranceValueText.SetText($"+{savedAmount}");
            }
            else
            {
                totalClosing = baseClosing;
                insuranceValueText.SetText("0");
            }

            bool canContinue = TotalGameManager.Instance.AddGold(totalClosing);

            if (!canContinue)
            {
                return;
            }

            resultClosingText.SetText($"{totalClosing}");


            if (isFailed) // 똑같은 조건문을 처리 방식 때문에 두번 부르는건 에반데
            {
                TotalGameManager.Instance.ResetProgressAfterShipwreck();
            }

            UIManager.ShowResultWindow(totalClosing);

            for (int i = 0; i < enableObjects.Length; ++i)
            {
                enableObjects[i].SetActive(true);
            }

            for (int i = 0; i < disableObjects.Length; ++i)
            {
                disableObjects[i].SetActive(false);
            }
        }

        private void GameOverEffect()
        {
            Vector3 moveVector = new Vector3(0, 1, 0);
            SpriteRenderer screenSR = ggScreen.GetComponent<SpriteRenderer>();
            Color screenColor = screenSR.color;
            
            if (ship.rb.gravityScale < 10.0f)
            {
                ship.rb.gravityScale += 0.7f * Time.deltaTime;
            }
            if (screenColor.a < 0.8f)
            {
                screenColor.a += 0.15f * Time.deltaTime;
                screenSR.color = screenColor;
            }
        }



        public void AddLoot(int loot)
        {
            InStageLootValue += loot;
            OnLootChanged?.Invoke(InStageLootValue);
        }

        public void LostLoot(int loot)
        {
            InStageLootValue -= loot;
            OnLootChanged?.Invoke(InStageLootValue);
        }

        public event System.Action<int> OnLootChanged;

        #endregion

    }
}
