using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace Babel
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Block[] BallPrefabs;
        public int maxIndexInQueue = 4;

        private Queue<int> ballIndexQueue = new Queue<int>();

        #region Unity Methods;
        private void Awake()
        {
            // 약식 싱글톤 선언
            // 특정 한 객체만 남기고 역할이 겹치는 놈들을 모두 제거함
            // 이처럼 하나의 인스턴스만 남기는 것을 싱글톤이라고 함
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            maxIndexInQueue = Mathf.Min(maxIndexInQueue, BallPrefabs.Length);
            maxIndexInQueue = Mathf.Max(maxIndexInQueue, 0);
            ballIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));

        }
        #endregion

        // 큐에서 꺼내온 녀석을 반환
        public Block GetNextBlock()
        {
            int dequeueIndex = ballIndexQueue.Dequeue();
            ballIndexQueue.Enqueue(Random.Range(0, maxIndexInQueue));

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

        public TextMeshProUGUI text;
        private Dictionary<int, int> scoreTable = new Dictionary<int, int>();
        private int score;

        private void Start()
        {
            scoreTable.Add(1, 10);
            scoreTable.Add(2, 20);
            scoreTable.Add(3, 30);
            scoreTable.Add(4, 50);
            scoreTable.Add(5, 70);
            scoreTable.Add(6, 100);
            scoreTable.Add(7, 130);
            scoreTable.Add(8, 160);
            scoreTable.Add(9, 200);
        }

        private void AddScore(int upgradeLevel)
        {
            score += scoreTable[upgradeLevel];
            text.SetText($"{score}");
        }
    }
}
