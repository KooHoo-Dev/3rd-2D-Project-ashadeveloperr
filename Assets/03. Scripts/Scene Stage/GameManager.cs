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

        public Image gaugeFillImage;
        public UIManager UIManager;
        public TMP_Text lootValueText;
        public TMP_Text insuranceTitleText;
        public TMP_Text insuranceValueText;
        public TMP_Text resultLootValueText;
        public TMP_Text resultCostText;
        public TMP_Text resultClosingText;

        public Block[] BlockPrefabs;
        public int maxIndexInQueue;

        public Ship ship;
        public GameObject ggScreen;
        public float sinkPoint;
        public bool isGameOver;
        public bool isFailed;

        private Queue<int> blockIndexQueue = new Queue<int>();


        private Coroutine countCoroutine;
        private int currentDisplayedLoot = 0;

        [Header("게임 오버 타이머")]

        [SerializeField] private TMP_Text timerText;
        public float RemainingTime { get; private set; }

        #endregion

        #region User Var;
        private float pumpPower;

        public int InStageLootValue { get; private set; }


        #endregion

        #region Unity Methods;

        protected override void Awake()
        {
            int stageTargetIndex = Mathf.Max(TotalGameManager.Instance.currentStageLevel, 0);
            maxIndexInQueue = Mathf.Min(stageTargetIndex, BlockPrefabs.Length);
            if (maxIndexInQueue <= 0) maxIndexInQueue = 1;
            blockIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));
        }

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
            UpdateLootValue(InStageLootValue);

            pumpPower = 1.5f * (TotalGameManager.Instance.CurrentLevelPump * 1.0f);
            RemainingTime = 20 + (TotalGameManager.Instance.CurrentLevelTimer * 3) ;
        }

        private void FixedUpdate()
        {
            IsDeckSinking();
        }

        private void Update()
        {
            if (isGameOver && isFailed)
            {
                GameOverEffect();
            }

            UpdateGauge();

            if (RemainingTime > 0)
            {
                RemainingTime -= Time.deltaTime;

                if (RemainingTime < 0)
                {
                    RemainingTime = 0;
                    isGameOver = true;
                    GameOver();
                }

                UpdateTimerDisplay();
            }

        }

        #endregion



        #region Custom Methods;

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
            if (isFailed)
            {
                InStageLootValue = 0;
                TotalGameManager.Instance.currentShipType -= (ShipType)1;
            }
            
            OnLootChanged?.Invoke(InStageLootValue);
            resultLootValueText.SetText($"+{InStageLootValue}");
            resultCostText.SetText($"-{TotalGameManager.Instance.TotalMaintenanceCost()}");

            int baseClosing = InStageLootValue - TotalGameManager.Instance.TotalMaintenanceCost();
            if (baseClosing > 0) TotalGameManager.Instance.currentStageLevel++;

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

            TotalGameManager.Instance.AddGold(totalClosing);

            resultClosingText.SetText($"{totalClosing}");

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
            if (InStageLootValue < 0)
            {
                InStageLootValue = 0;
            }
            OnLootChanged?.Invoke(InStageLootValue);
        }

        public event System.Action<int> OnLootChanged;

        #endregion

    }
}
