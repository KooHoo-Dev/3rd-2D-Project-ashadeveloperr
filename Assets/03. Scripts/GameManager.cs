using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Babel
{
    public class GameManager : MonoBehaviour
    {
        #region Refs and Var;
        public static GameManager Instance;

        public Block[] BallPrefabs;
        public int maxIndexInQueue = 4;

        public Ship ship;

        private float sinkPoint;

        private bool isSinking;
        private bool isGameOver;

        private Queue<int> blockIndexQueue = new Queue<int>();

        public TextMeshPro text;
        public GameObject ggScreen;
        
        private Dictionary<int, int> scoreTable = new Dictionary<int, int>();
        private int score;
        #endregion

        #region User Var;
        private float pumpPower = 1.5f;

        #endregion

        #region Unity Methods;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            maxIndexInQueue = Mathf.Min(maxIndexInQueue, BallPrefabs.Length);
            maxIndexInQueue = Mathf.Max(maxIndexInQueue, 0);
            blockIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));

        }

        private void Start()
        {
            
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
        }

        #endregion



        #region Custom Methods;


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
                        Debug.LogWarning($"물 퍼내는 중... 현재 {sinkPoint}%");
                    }
                    else sinkPoint = 0;

                }
                else
                {
                    if (sinkPoint < 100)
                    {
                        sinkPoint += Mathf.Abs(deck.position.y) * Time.deltaTime * 10f - (pumpPower / 100);
                        Debug.LogWarning($"도움! {deck} 위치에서 침수중... 현재 {sinkPoint}%");
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
            if (text.transform.position.y < 10.0f)
            {
                Vector3 targetPos = new Vector3(0f, 10f, 0f);
                Vector3 lerpPos = Vector3.Lerp(text.transform.position, targetPos, 1.0f * Time.deltaTime);
                text.transform.position = lerpPos;

                //moveVector = moveVector * 2.2f * Time.deltaTime;
                //text.transform.position += moveVector;
            }
            if (ship.rb.gravityScale < 5.0f)
            {
                ship.rb.gravityScale += 0.2f * Time.deltaTime;
            }
            if (screenColor.a < 0.8f)
            {
                screenColor.a += 0.1f * Time.deltaTime;
                screenSR.color = screenColor;
            }
            Debug.Log($"GG");
        }


        private void AddScore(int upgradeLevel)
        {
            score += scoreTable[upgradeLevel];
            text.SetText($"{score}");
        }

        #endregion

    }
}
