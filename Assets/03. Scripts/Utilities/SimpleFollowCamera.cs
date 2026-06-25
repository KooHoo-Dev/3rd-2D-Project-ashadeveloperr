using System;
using UnityEngine;

namespace Study.Utilities
{
    // 타겟을 부드럽게(Lerp) 따라가는 범용 추적 카메라


    public class SimpleFollowCamera : MonoBehaviour
    {
        public enum States
        {
            Stopped,
            Holding,
            Following,
        }

        [field: SerializeField] public Transform Target {  get; set; }
        [SerializeField] private float lerpSpeed;
        [SerializeField] private Vector3 offset;

        private Camera Cam {  get; set; }
        private States State { get; set; }

        private void Start()
        {
            // 현재 씬에서 MainCamera Tag가 달려있는 놈을 할당함
            Cam = Camera.main;
        }


        private void Update()
        {
            switch (State)
            {
                case States.Stopped:
                    Target = null;
                    break;
                case States.Holding:
                    break;
                case States.Following:
                    // lerpSpeed는 댐핑값이라고 해서 속도에 대한 보정값임
                    Vector3 lerpPos = Vector3.Lerp(transform.position, Target.position + offset, lerpSpeed * Time.deltaTime);
                    lerpPos.z = transform.position.z;  // 2D니까 z값은 바뀔 필요 없음
                    transform.position = lerpPos;
                    break;
            }
        }


        public void ChangeState(States state)
        {
            if (State == States.Following && Target == null) return;
            State = state;
        }
    }


}

