using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Babel
{
    public class BackgroundManager : MonoBehaviour
    {
        [Header("구름 흐름")]
        [SerializeField] private float flowSpeed = 2.0f;
        [SerializeField] private Vector3 flowDirection = Vector3.left;

        [Header("구름 생성 기준점")]
        [FormerlySerializedAs("CloudPivotLeft")]
        [SerializeField] private Transform cloudPivotLeft;
        [FormerlySerializedAs("CloudPivotRight")]
        [SerializeField] private Transform cloudPivotRight;

        [Header("구름 프리팹")]
        [SerializeField] private Cloud cloudPrefab;
        [SerializeField] private GameObject cloudLayer;
        [SerializeField] private int totalCloud = 10;

        [Header("바람에 따른 구름 속도")]
        [SerializeField] private float cloudWindSpeedMultiplier = 0.35f;
        [SerializeField] private float minimumCloudSpeed = 0.2f;

        private readonly List<Cloud> activeCloudList = new List<Cloud>();
        private readonly Queue<Cloud> cloudQueue = new Queue<Cloud>();

        private void Start()
        {
            for (int i = 0; i < totalCloud; i++)
            {
                Cloud randomCloud = FirstSpawnCloud();

                if (randomCloud != null)
                {
                    randomCloud.gameObject.SetActive(false);
                    cloudQueue.Enqueue(randomCloud);
                }
            }
        }

        private void Update()
        {
            MoveClouds();
            CheckDisableClouds();
            ManageCloudQueue();
        }

        public void SetWind(float windSpeed)
        {
            if (Mathf.Abs(windSpeed) < 0.05f)
            {
                flowSpeed = 0.0f;
                return;
            }

            flowDirection = windSpeed < 0 ? Vector3.left : Vector3.right;
            flowSpeed = Mathf.Max(minimumCloudSpeed, Mathf.Abs(windSpeed) * cloudWindSpeedMultiplier);
        }

        private void MoveClouds()
        {
            Vector3 globalFlowSpeed = flowDirection.normalized * (flowSpeed * Time.deltaTime);

            for (int i = 0; i < activeCloudList.Count; i++)
            {
                if (activeCloudList[i] != null)
                {
                    activeCloudList[i].Move(globalFlowSpeed);
                }
            }
        }

        private void CheckDisableClouds()
        {
            if (activeCloudList.Count == 0) return;

            for (int i = activeCloudList.Count - 1; i >= 0; i--)
            {
                Cloud currentCloud = activeCloudList[i];
                if (currentCloud == null) continue;

                bool shouldDisable = IsCloudOutOfBounds(currentCloud);

                if (shouldDisable)
                {
                    activeCloudList.RemoveAt(i);
                    ResetCloudPositionByDirection(currentCloud);
                    currentCloud.gameObject.SetActive(false);
                    cloudQueue.Enqueue(currentCloud);
                }
            }
        }

        private bool IsCloudOutOfBounds(Cloud cloud)
        {
            SpriteRenderer spriteRenderer = cloud.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                if (flowDirection.x < 0)
                {
                    return cloud.transform.position.x <= cloudPivotLeft.position.x;
                }
                else if (flowDirection.x > 0)
                {
                    return cloud.transform.position.x >= cloudPivotRight.position.x;
                }

                return false;
            }

            if (flowDirection.x < 0)
            {
                return spriteRenderer.bounds.max.x <= cloudPivotLeft.position.x;
            }
            else if (flowDirection.x > 0)
            {
                return spriteRenderer.bounds.min.x >= cloudPivotRight.position.x;
            }

            return false;
        }

        private void ResetCloudPositionByDirection(Cloud cloud)
        {
            float randomHeight = Random.Range(12.0f, 18.0f);
            SpriteRenderer spriteRenderer = cloud.GetComponent<SpriteRenderer>();
            float halfWidth = 0.0f;

            if (spriteRenderer != null)
            {
                halfWidth = spriteRenderer.bounds.extents.x;
            }

            Vector3 resetPosition = cloud.transform.position;

            if (flowDirection.x < 0)
            {
                resetPosition.x = cloudPivotRight.position.x + halfWidth;
            }
            else if (flowDirection.x > 0)
            {
                resetPosition.x = cloudPivotLeft.position.x - halfWidth;
            }

            resetPosition.y = randomHeight;
            cloud.transform.position = resetPosition;
        }

        private Cloud SpawnRandomCloud()
        {
            if (cloudPrefab == null) return null;

            Cloud cloud = Instantiate(cloudPrefab, cloudLayer.transform);
            ResetCloudPositionByDirection(cloud);
            return cloud;
        }

        private Cloud FirstSpawnCloud()
        {
            if (cloudPrefab == null) return null;

            Cloud cloud = Instantiate(cloudPrefab, cloudLayer.transform);
            float randomHeight = Random.Range(12.0f, 18.0f);
            float leftX = Mathf.Min(cloudPivotLeft.position.x, cloudPivotRight.position.x);
            float rightX = Mathf.Max(cloudPivotLeft.position.x, cloudPivotRight.position.x);
            float firstRandomSpawnPosition = Random.Range(leftX, rightX);
            cloud.transform.position = new Vector3(firstRandomSpawnPosition, randomHeight, cloud.transform.position.z);
            return cloud;
        }

        private void ManageCloudQueue()
        {
            int totalCloudCount = activeCloudList.Count + cloudQueue.Count;

            if (totalCloudCount < totalCloud)
            {
                Cloud randomCloud = SpawnRandomCloud();

                if (randomCloud != null)
                {
                    randomCloud.gameObject.SetActive(false);
                    cloudQueue.Enqueue(randomCloud);
                }
            }

            if (activeCloudList.Count < totalCloud && cloudQueue.Count > 0)
            {
                Cloud enableCloud = cloudQueue.Dequeue();
                enableCloud.gameObject.SetActive(true);
                activeCloudList.Add(enableCloud);
            }
        }
    }
}
