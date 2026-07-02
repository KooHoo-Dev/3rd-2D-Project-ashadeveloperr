using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


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

        private List<Cloud> activeCloudList = new List<Cloud>();
        private Queue<Cloud> cloudQueue = new Queue<Cloud>();

        private void Start()
        {
            for (int i = 0; i < 10; i++)
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

        private void MoveClouds()
        {
            Vector3 globalFlowSpeed = flowDirection.normalized * ( flowSpeed * Time.deltaTime );

            for (int i = 0; i < activeCloudList.Count; i++)
            {
                if(activeCloudList[i] != null)
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

                bool shouldDisable = false;

                if (flowDirection.x < 0)
                {
                    shouldDisable = currentCloud.transform.position.x <= CloudPivotLeft.position.x;
                }
                else if (flowDirection.x > 0)
                {
                    shouldDisable = currentCloud.transform.position.x >= CloudPivotRight.position.x;
                }

                if (shouldDisable)
                {
                    activeCloudList.RemoveAt(i);

                    currentCloud.gameObject.SetActive(false);

                    float randomHeight = UnityEngine.Random.Range(12.0f, 18.0f);

                    if (flowDirection.x < 0)
                    {
                        currentCloud.transform.position = new Vector2(CloudPivotRight.position.x, randomHeight);
                    }
                    else if (flowDirection.x > 0)
                    {
                        currentCloud.transform.position = new Vector2(CloudPivotLeft.position.x, randomHeight);
                    }

                    cloudQueue.Enqueue(currentCloud);
                }
            }
        }

        private Cloud SpawnRandomCloud()
        {
            if (cloudPrefab == null) return null;

            Cloud cloud = Instantiate(cloudPrefab, cloudLayer.transform);

            float randomHeight = UnityEngine.Random.Range(12.0f, 18.0f);

            if (flowDirection.x < 0)
            {
                cloud.transform.position = new Vector2(CloudPivotRight.position.x, randomHeight);
            }
            if (flowDirection.x > 0)
            {
                cloud.transform.position = new Vector2(CloudPivotLeft.position.x, randomHeight);
            }

            return cloud;
        }

        private Cloud FirstSpawnCloud()
        {
            if (cloudPrefab == null) return null;

            Cloud cloud = Instantiate(cloudPrefab, cloudLayer.transform);

            float randomHeight = UnityEngine.Random.Range(12.0f, 18.0f);
            float firstRandomSpawnPosition = UnityEngine.Random.Range(-30.0f, 70.0f);

            if (flowDirection.x < 0)
            {
                cloud.transform.position = new Vector2(firstRandomSpawnPosition, randomHeight);
            }
            if (flowDirection.x > 0)
            {
                cloud.transform.position = new Vector2(firstRandomSpawnPosition, randomHeight);
            }

            return cloud;
        }

        private void ManageCloudQueue()
        {
            int totalCloudCount = activeCloudList.Count + cloudQueue.Count;
            
            if(totalCloudCount < totalCloud)
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
