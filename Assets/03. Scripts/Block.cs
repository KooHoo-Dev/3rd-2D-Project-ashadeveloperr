using UnityEngine;


namespace Babel
{

    public class Block : MonoBehaviour
    {
        public int level = 1;

        // C#의 프로퍼티를 이용해서 외부에서는 조회가 가능하지만 변경에 대한 책임은 나만 가질 수 있는 안전한 변수(필드)를 만들어 줌
        public bool HasFirstContact { get; private set; } = false;
        public bool IsMerged { get; set; } = false;

        private bool isUnderWater;

        private Rigidbody2D rb;
        private Collider2D col;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            col.enabled = false;
            //  gravityScale은 중력의 영향을 얼마나 받는지 결정하는 변수입니다
            rb.gravityScale = 0.0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public void Drop()
        {
            col.enabled = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.0f;

            // transform.SetParent()
            // : 부모자식 관계를 형성해주는 함수 입니다. null 입력시
            //  부모 transform이 없는 월드 최상위 개체가 됩니다
            transform.SetParent(null);
        }

        private void Update()
        {
            IsUnderWater();
        }

        private void IsUnderWater()
        {
            if(transform.position.y <= 0 && isUnderWater == false)
            {
                isUnderWater = true;
                rb.gravityScale /= 5;
                rb.mass /= 5;
                rb.linearVelocity /= 15;
            }
            else if(transform.position.y > 0 && isUnderWater)
            {
                isUnderWater = false;
                rb.gravityScale *= 5;
                rb.mass *= 5;
                rb.linearVelocity *= 15;
            }
        }

        // OnCollision 시리즈
        // - OnTrigger와 함께 사용되는 물리 감지 이벤트 함수로, rigidbody를 통해서 감지할 수 있음

        // OnCollisionEnter2D
        // - Trigger가 아닌 Collider와 충돌했을 때 호출되는 이벤트 함수.

        // OnCollisionStay2D
        // - 물리적 충돌이 유지되고 있을 때 호출되는 이벤트 함수.

        // OnCollisionExit2D
        // - 물리적 충돌이 종료되었을 때 호출되는 이벤트 함수.

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HasFirstContact = true;

            // 두 볼이 병합되는 과정
            // 1. 접촉 발생
            // 2. 두 개체가 사라짐
            // 3. 상위 개체가 사라진 두 개체의 중앙 위치에 생김

            //// 두 개체가 접촉해서 병합 요청이 두번 호출될 수 있으므로 한쪽에 bool 변수를 넣어서 중복호출되지 않게 만듦
            //if (IsMerged) return;

            //// 충돌 정보에서 나와 충돌한 gameObject에서 AnimalBall 컴포넌트를 읽어들임
            //Block contactBall = collision.gameObject.GetComponent<Block>();

            //// 벽에 부딪혀서 충돌한 개체에 AnimalBall 컴포넌트가 없다면 이탈
            //if (contactBall == null) return;

            //// 접촉한 볼과 나의 레벨이 다르다면
            //if (level != contactBall.level) return;

            //GameManager.Instance.MergeBall(this, contactBall);
        }

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    if (HasFirstContact && collision.CompareTag("Finish"))
        //    {
        //    }
        //}


    }
}
