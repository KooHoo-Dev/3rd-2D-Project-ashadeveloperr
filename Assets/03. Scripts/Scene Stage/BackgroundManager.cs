using UnityEngine;
using System.Collections.Generic;

namespace Babel
{
    public class BackgroundManager : MonoBehaviour
    {
        public float flowSpeed = 2.0f;
        public Vector3 flowDirection = Vector3.left;

        public Transform CloudPivotLeft;
        public Transform CloudPivotRight;

        public Cloud cloudPrefab;

        public GameObject cloudLayer;
        public int totalCloud = 10;

        [Header("Wind Cloud Settings")]
        public float cloudWindSpeedMultiplier = 0.35f;
        public float minimumCloudSpeed = 0.2f;

        private List<Cloud> activeCloudList = new List<Cloud>();
        private Queue<Cloud> cloudQueue = new Queue<Cloud>();

        private void Start()
        {
            for (int i = 0; i < totalCloud; i++)
            {
                Cloud randCloud = FirstSpawnCloud();

                if (randCloud != null)
                {
                    randCloud.gameObject.SetActive(false);
                    cloudQueue.Enqueue(randCloud);
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
                    return cloud.transform.position.x <= CloudPivotLeft.position.x;
                }
                else if (flowDirection.x > 0)
                {
                    return cloud.transform.position.x >= CloudPivotRight.position.x;
                }

                return false;
            }

            if (flowDirection.x < 0)
            {
                return spriteRenderer.bounds.max.x <= CloudPivotLeft.position.x;
            }
            else if (flowDirection.x > 0)
            {
                return spriteRenderer.bounds.min.x >= CloudPivotRight.position.x;
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
                resetPosition.x = CloudPivotRight.position.x + halfWidth;
            }
            else if (flowDirection.x > 0)
            {
                resetPosition.x = CloudPivotLeft.position.x - halfWidth;
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

            float leftX = Mathf.Min(CloudPivotLeft.position.x, CloudPivotRight.position.x);
            float rightX = Mathf.Max(CloudPivotLeft.position.x, CloudPivotRight.position.x);

            float firstRandomSpawnPosition = Random.Range(leftX, rightX);

            cloud.transform.position = new Vector3(
                firstRandomSpawnPosition,
                randomHeight,
                cloud.transform.position.z
            );

            return cloud;
        }

        private void ManageCloudQueue()
        {
            int totalCloudCount = activeCloudList.Count + cloudQueue.Count;

            if (totalCloudCount < totalCloud)
            {
                Cloud randCloud = SpawnRandomCloud();

                if (randCloud != null)
                {
                    randCloud.gameObject.SetActive(false);
                    cloudQueue.Enqueue(randCloud);
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