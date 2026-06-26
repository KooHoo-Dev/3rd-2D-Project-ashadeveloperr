using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        private Camera Cam { get; set; }

        public Block[] BallPrefabs;
        public int maxIndexInQueue = 4;

        public Ship ship;

        private float sinkPoint;

        private bool isSinking;
        private bool isGameOver;

        private Queue<int> blockIndexQueue = new Queue<int>();

        public Transform resultUI;
        public TextMeshProUGUI scoreText;


        public GameObject ggScreen;
        
        private Dictionary<int, int> scoreTable = new Dictionary<int, int>();
        private int score;
        #endregion

        #region User Var;
        private float pumpPower = 1.5f;

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
            Cam = Camera.main;
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
                CameraReset();
            }
            else
            {
                CameraWork();
            }
        }

        #endregion



        #region Custom Methods;


        private void CameraWork()
        {
            Vector3 offset = new Vector3(0, 7, 0);
            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, ship.transform.position + offset, 0.7f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;  // 2D니까 z값은 바뀔 필요 없음
            Cam.transform.position = lerpPos;
        }

        private void CameraReset()
        {
            Vector3 offset = new Vector3(0, 18, 0);
            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, Vector3.zero + offset, 0.4f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;  // 2D니까 z값은 바뀔 필요 없음
            Cam.transform.position = lerpPos;
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
            if (resultUI.transform.position.y < Cam.transform.position.y)
            {
                Vector3 targetPos = Cam.transform.position;
                Vector3 lerpPos = Vector3.Lerp(resultUI.transform.position, targetPos, 1.5f * Time.deltaTime);
                lerpPos.z = 1.0f;
                resultUI.transform.position = lerpPos;

                //moveVector = moveVector * 2.2f * Time.deltaTime;
                //text.transform.position += moveVector;
            }
            if (ship.rb.gravityScale < 5.0f)
            {
                ship.rb.gravityScale += 0.3f * Time.deltaTime;
            }
            if (screenColor.a < 0.8f)
            {
                screenColor.a += 0.15f * Time.deltaTime;
                screenSR.color = screenColor;
            }
            Debug.Log($"GG");
        }


        private void AddScore(int upgradeLevel)
        {
            score += scoreTable[upgradeLevel];
            scoreText.SetText($"{score}");
        }

        #endregion

    }
}
