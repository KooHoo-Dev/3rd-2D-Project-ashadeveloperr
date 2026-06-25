using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Study.Utilities
{
    // PC 플랫폼에서 사용하는 키보드 마우스 인풋들을 묶어놓은 유틸 클래스를 작성해보자.
    // 목표는 쉽고 직관적으로 사용할 수 있돌고 기능들을 묶어놓는 것

    // 아래처럼 작업하는 디자인 패턴을 Wrapper 패턴이라고 부름 (Wrapping 한다고 표현함.)
    
    // SimpleInput 클래스는 어디서든 호출할 수 있게 static(정적, 고정, 랜드마크) 키워드를 사용해 선언해야함
    public static class SimpleInput
    {
        #region 키보드 입력
        public static bool GetKeyDown(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        public static bool GetKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }
        public static bool GetKey(Key key)
        {
            return Keyboard.current[key].isPressed;
        }
        public static bool AnyKeyDown()
        {
            return Keyboard.current.anyKey.wasPressedThisFrame;
        }
        #endregion

        #region 마우스 입력
        /// <summary>
        /// 마우스 버튼 종류를 정의하는 열거형
        /// </summary>
        public enum MouseButton { Left = 0, Right, Middle}

        private static ButtonControl GetMouseControl(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return Mouse.current.leftButton;
                case MouseButton.Right:
                    return Mouse.current.rightButton;
                case MouseButton.Middle:
                    return Mouse.current.middleButton;
            }

            return null;
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return GetMouseControl(button).wasPressedThisFrame;
        }
        public static bool GetMouseButtonUp(MouseButton button)
        {
            return GetMouseControl(button).wasReleasedThisFrame;
        }
        public static bool GetMouseButton(MouseButton button)
        {
            return GetMouseControl(button).isPressed;
        }

        public static Vector2 GetMousePosition()
        {
            return Mouse.current.position.ReadValue();
        }
        /// <summary>
        /// 마우스의 한 프레임당 이동량 반환
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetMouseDelta()
        {
            return Mouse.current.delta.ReadValue();
        }
        public static float GetMouseScrollDeltaRaw()
        {
            return Mouse.current.scroll.ReadValue().y;
        }
        /// <summary>
        /// 마우스 휠 스크롤을 -1, 0, 1 단위로 반환
        /// 스크롤의 한 단위
        /// </summary>
        /// <returns></returns>
        public static float GetMouseScrollDelta()
        {
            float raw = GetMouseScrollDeltaRaw();
            return Mathf.Approximately(raw, 0f) ? 0f : Mathf.Sign(raw);
            // Mathf.Approximately
            // 0이랑 비교를 하면 근사치로 일정 범위 내에 raw가 있는지 비교를 함
        }

        #endregion

        #region 축 입력 (WASD / 방향키)
        /// <summary>
        /// 수평 축 원시 입력값 반환
        /// A/D키 또는 좌/우 화살표 키 대응
        /// 레거시에서 Input.GetAxisRaw("Horizontal")과 동일
        /// </summary>
        /// <returns></returns>
        public static float GetAxisHorizontalRaw()
        {
            float axis = 0.0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) axis += 1.0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) axis -= 1.0f;

            return axis;
        }
        /// <summary>
        /// 수직 축 원시 입력값 반환
        /// W/S키 또는 상/하 화살표 키 대응
        /// 레거시에서 Input.GetAxisRaw("Vertical")과 동일
        /// </summary>
        /// <returns></returns>
        public static float GetAxisVerticalRaw()
        {
            float axis = 0.0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) axis += 1.0f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) axis -= 1.0f;

            return axis;
        }

        /// <summary>
        /// 수평 / 수직 Raw 입력값을 합쳐 정규화된 Vector2로 반환
        /// 대각선 입력시 속도가 조금 빨라지는(루트2 되는거) 현상을 보정(normalized)하여 반환
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetMoveAxisRaw()
        {
            Vector2 axis = new Vector2();
            axis.x = GetAxisHorizontalRaw();
            axis.y = GetAxisVerticalRaw();

            if (axis.sqrMagnitude > 1f) return axis.normalized;
            return axis;
        }

        #endregion
    }
}


