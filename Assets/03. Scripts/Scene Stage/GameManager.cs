using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Babel.TotalGameManager;

namespace Babel
{
    public class GameManager : SingletonBase<GameManager>
    {
        [Header("인게임 UI")]
        [SerializeField] private Image gaugeFillImage;
        [FormerlySerializedAs("UIManager")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private TMP_Text lootValueText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text stageText;

        [Header("결과창 UI")]
        [SerializeField] private TMP_Text insuranceTitleText;
        [SerializeField] private TMP_Text insuranceValueText;
        [SerializeField] private TMP_Text resultLootValueText;
        [SerializeField] private TMP_Text resultCostText;
        [SerializeField] private TMP_Text resultClosingText;

        [Header("게임오버 연출")]
        [SerializeField] private GameObject ggScreen;
        [SerializeField] private GameObject[] enableObjects;
        [SerializeField] private GameObject[] disableObjects;

        [Header("블록 설정")]
        [FormerlySerializedAs("BlockPrefabs")]
        [SerializeField] private Block[] blockPrefabs;

        [Header("선박 설정")]
        [SerializeField] private ShipPrefabEntry[] shipPrefabEntries;
        [SerializeField] private Transform shipSpawnPoint;

        [Header("바람 설정")]
        [SerializeField] private BackgroundManager backgroundManager;
        [SerializeField] private float maxWindSpeedPerStage = 1.5f;
        [SerializeField] private float windChangeRatio = 0.1f;

        private readonly Queue<int> blockIndexQueue = new Queue<int>();
        private Ship ship;
        private Transform shipParent;
        private Coroutine countCoroutine;
        private int maxIndexInQueue;
        private int currentDisplayedLoot = 0;
        private float sinkPoint;
        private float pumpPower;
        private bool isGameOver;
        private bool isFailed;
        private bool hasGameOverProcessed;
        private event System.Action<int> OnLootChanged;

        public Ship CurrentShip => ship;
        public float SinkPoint => sinkPoint;
        public bool IsGameOver => isGameOver;
        public bool IsFailed => isFailed;
        public float RemainingTime { get; private set; }
        public int InStageLootValue { get; private set; }
        public float CurrentWindSpeed { get; private set; }
        public float CurrentMaxWindSpeed { get; private set; }

        [System.Serializable]
        public class ShipPrefabEntry
        {
            [FormerlySerializedAs("shipType")]
            [SerializeField] private ShipType shipType;
            [FormerlySerializedAs("prefab")]
            [SerializeField] private Ship prefab;

            public ShipType ShipType => shipType;
            public Ship Prefab => prefab;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void Awake()
        {
            base.Awake();

            if (instance != this)
            {
                return;
            }

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

            if (uiManager == null)
            {
                uiManager = FindAnyObjectByType<UIManager>();
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

        private bool IsTutorialSplashBlockingGame()
        {
            return uiManager != null && uiManager.IsTutorialSplashActive;
        }

        private void InitializeWind()
        {
            CurrentMaxWindSpeed = GetCurrentMaxWindSpeed();
            CurrentWindSpeed = Random.Range(-CurrentMaxWindSpeed, CurrentMaxWindSpeed);
            ApplyWindState();
        }

        private float GetCurrentMaxWindSpeed()
        {
            int displayStage = Mathf.Max(TotalGameManager.Instance.CurrentStageLevel + 1, 1);
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

            if (uiManager != null)
            {
                uiManager.UpdateWindDisplay(CurrentWindSpeed);
            }
        }

        private void UpdateStageDisplay()
        {
            if (stageText == null)
            {
                return;
            }

            int displayStage = TotalGameManager.Instance.CurrentStageLevel + 1;
            stageText.SetText($"Stage {displayStage:00}");
        }

        private void SpawnShipByCurrentType()
        {
            ShipType currentType = TotalGameManager.Instance.CurrentShipType;
            Ship prefab = GetShipPrefab(currentType);
            Vector3 spawnPosition = shipSpawnPoint != null ? shipSpawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = shipSpawnPoint != null ? shipSpawnPoint.rotation : Quaternion.identity;
            ship = Instantiate(prefab, spawnPosition, spawnRotation, shipParent);
            UIManager foundUIManager = FindAnyObjectByType<UIManager>();

            if (foundUIManager != null)
            {
                foundUIManager.SetShip(ship);
            }
        }

        private Ship GetShipPrefab(ShipType shipType)
        {
            foreach (ShipPrefabEntry entry in shipPrefabEntries)
            {
                if (entry.ShipType == shipType)
                {
                    return entry.Prefab;
                }
            }

            return null;
        }

        private void InitializeBlockQueue()
        {
            int stageTargetIndex = Mathf.Max(TotalGameManager.Instance.CurrentStageLevel, 0);
            maxIndexInQueue = Mathf.Min(stageTargetIndex, blockPrefabs.Length);

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
                {
                    currentDisplayedLoot++;
                }
                else
                {
                    currentDisplayedLoot--;
                }

                lootValueText.SetText($"{currentDisplayedLoot}");
                yield return new WaitForSeconds(0.003f);
            }

            countCoroutine = null;
        }

        private void IsDeckSinking()
        {
            if (ship.DeckLeft.position.y <= 0 || ship.DeckCenter.position.y <= 0 || ship.DeckRight.position.y <= 0)
            {
                if (ship.DeckLeft.position.y <= 0)
                {
                    SinkingEffect(ship.DeckLeft);
                }

                if (ship.DeckCenter.position.y <= 0)
                {
                    SinkingEffect(ship.DeckCenter);
                }

                if (ship.DeckRight.position.y <= 0)
                {
                    SinkingEffect(ship.DeckRight);
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
                    else
                    {
                        sinkPoint = 0;
                    }
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
            if (index < 0 || blockPrefabs.Length <= index) return blockPrefabs[0];
            return blockPrefabs[index];
        }

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
                TotalGameManager.Instance.CurrentStageLevel++;
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

            if (isFailed)
            {
                TotalGameManager.Instance.ResetProgressAfterShipwreck();
            }

            uiManager.ShowResultWindow(totalClosing);

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
            SpriteRenderer screenSpriteRenderer = ggScreen.GetComponent<SpriteRenderer>();
            Color screenColor = screenSpriteRenderer.color;

            if (ship.Rigidbody.gravityScale < 10.0f)
            {
                ship.Rigidbody.gravityScale += 0.7f * Time.deltaTime;
            }

            if (screenColor.a < 0.8f)
            {
                screenColor.a += 0.15f * Time.deltaTime;
                screenSpriteRenderer.color = screenColor;
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
    }
}
