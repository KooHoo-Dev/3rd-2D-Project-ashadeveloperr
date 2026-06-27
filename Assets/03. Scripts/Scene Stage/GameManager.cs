using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
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
        
        public UIManager UIManager;
        public TMP_Text lootValueText;
        public TMP_Text resultLootValueText;

        public Block[] BallPrefabs;
        public int maxIndexInQueue = 4;

        public Ship ship;

        private float sinkPoint;

        private bool isSinking;
        public bool isGameOver;

        private Queue<int> blockIndexQueue = new Queue<int>();


        public GameObject ggScreen;
        
        private Dictionary<int, int> scoreTable = new Dictionary<int, int>();

        private Coroutine countCoroutine;
        private int currentDisplayedLoot = 0;

        #endregion

        #region User Var;
        private float pumpPower = 1.5f;

        public int InStageLootValue { get; private set; }


        #endregion

        #region Unity Methods;

        protected override void Awake()
        {
            maxIndexInQueue = Mathf.Min(maxIndexInQueue, BallPrefabs.Length);
            maxIndexInQueue = Mathf.Max(maxIndexInQueue, 0);
            blockIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));
        }

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
            UpdateLootValue(InStageLootValue);
        }

        private void FixedUpdate()
        {
            IsDeckSinking();
        }

        private void Update()
        {
            if (isGameOver)
            {
                GameOverEffect();
            }
            else
            {
            }
        }

        #endregion



        #region Custom Methods;


        private void UpdateLootValue(int targetLoot)
        {
            // 기존에 실행 중인 연출이 있다면 멈추고 새로 시작합니다 [4]
            if (countCoroutine != null) StopCoroutine(countCoroutine);
            countCoroutine = StartCoroutine(CountLootRoutine(targetLoot));
        }

        private IEnumerator CountLootRoutine(int targetLoot)
        {
            // 표시된 점수가 목표 점수와 같아질 때까지 반복합니다
            while (currentDisplayedLoot != targetLoot)
            {
                if (currentDisplayedLoot < targetLoot)
                    currentDisplayedLoot++;
                else
                    currentDisplayedLoot--;

                // 텍스트 갱신
                lootValueText.SetText($"{currentDisplayedLoot}");

                // 한 프레임 대기하거나 특정 시간을 기다립니다 [3, 4]
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
                isSinking = true;
            }
            else
            {
                isSinking = false;
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
                        //Debug.LogWarning($"물 퍼내는 중... 현재 {sinkPoint}%");
                    }
                    else sinkPoint = 0;

                }
                else
                {
                    if (sinkPoint < 100)
                    {
                        sinkPoint += Mathf.Abs(deck.position.y) * Time.deltaTime * 10f - (pumpPower / 100);
                        //Debug.LogWarning($"도움! {deck} 위치에서 침수중... 현재 {sinkPoint}%");
                    }
                    else
                    {
                        isGameOver = true;
                        GameOver();
                    }
                }
            }
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
            if (index < 0 || BallPrefabs.Length <= index) return BallPrefabs[0];
            return BallPrefabs[index];
        }

        // ballA 개체와 ballB 개체의 병합을 진행
        //public void MergeBall(Block blockA, Block blockB)
        //{
        //    // 두 개체를 병합 진행 상태로 선언
        //    blockA.IsMerged = true;
        //    blockB.IsMerged = true;

        //    // 삭제하기 전에 중간 지점을 지역 변수로 저장
        //    Vector3 centerPos = (blockA.transform.position + blockB.transform.position) / 2;

        //    // 무엇을 생성할지 입력
        //    int upgradeLevel = blockA.level;
        //    Block upgradeBall = GetBlock(upgradeLevel);
        //    AddScore(upgradeLevel);

        //    // 기존 볼들 삭제
        //    blockA.gameObject.SetActive(false);
        //    blockB.gameObject.SetActive(false);
        //    // 두 개체를 비활성화해주는 이유는 삭제가 느려서 충돌 안일어나게 하려고 일단 비활성화 후 안전하게 삭제하기 위함임

        //    Destroy(blockA.gameObject);
        //    Destroy(blockB.gameObject);

        //    // 상위 볼 생성
        //    Block spawnedBall = Instantiate(upgradeBall, centerPos, Quaternion.identity);
        //    spawnedBall.Drop();

        //}

        // 게임 오버가 되면 활성화될 개체들과 비활성화될 개체들

        [Header("GameOver Settings")]
        public GameObject[] enableObjects;
        public GameObject[] disableObjects;

        public void GameOver()
        {

            InStageLootValue = 0;
            OnLootChanged?.Invoke(InStageLootValue);
            resultLootValueText.SetText($"{InStageLootValue}");

            UIManager.ShowResultWindow();
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
            
            if (ship.rb.gravityScale < 5.0f)
            {
                ship.rb.gravityScale += 0.3f * Time.deltaTime;
            }
            if (screenColor.a < 0.8f)
            {
                screenColor.a += 0.15f * Time.deltaTime;
                screenSR.color = screenColor;
            }
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
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
            if (InStageLootValue < 0)
            {
                InStageLootValue = 0;
            }
        }

        public event System.Action<int> OnLootChanged;

        #endregion

    }
}
