using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ProjectKai.Player
{
    /// <summary>
    /// Keyboard.current 직접 폴링 방식.
    /// InputActionMap 포커스 문제를 완전 우회.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool ShootPressed { get; private set; }
        public bool DashPressed { get; private set; }
        public bool WeaponSwitchPressed { get; private set; }

        public event Action OnJumpPressed;
        public event Action OnAttackPressed;
        public event Action OnShootPressed;
        public event Action OnDashPressed;
        public event Action OnWeaponSwitchPressed;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
                Destroy(playerInput);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null) return;

            // Move
            float x = 0f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            float y = 0f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            MoveInput = new Vector2(x, y);

            // Jump
            if (kb.spaceKey.wasPressedThisFrame)
            {
                JumpPressed = true;
                JumpHeld = true;
                OnJumpPressed?.Invoke();
            }
            if (kb.spaceKey.wasReleasedThisFrame)
                JumpHeld = false;

            // Attack (J or Left Mouse)
            if (kb.jKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame))
            {
                AttackPressed = true;
                OnAttackPressed?.Invoke();
            }

            // Shoot (K or Right Mouse)
            if (kb.kKey.wasPressedThisFrame || (mouse != null && mouse.rightButton.wasPressedThisFrame))
            {
                ShootPressed = true;
                OnShootPressed?.Invoke();
            }

            // Dash (Shift)
            if (kb.leftShiftKey.wasPressedThisFrame)
            {
                DashPressed = true;
                OnDashPressed?.Invoke();
            }

            // Weapon Switch (Tab)
            if (kb.tabKey.wasPressedThisFrame)
            {
                WeaponSwitchPressed = true;
                OnWeaponSwitchPressed?.Invoke();
            }
        }

        public void ConsumeAllPresses()
        {
            JumpPressed = false;
            AttackPressed = false;
            ShootPressed = false;
            DashPressed = false;
            WeaponSwitchPressed = false;
        }

        public void ConsumeJump() => JumpPressed = false;
        public void ConsumeAttack() => AttackPressed = false;
        public void ConsumeShoot() => ShootPressed = false;
        public void ConsumeDash() => DashPressed = false;
        public void ConsumeWeaponSwitch() => WeaponSwitchPressed = false;
    }
}
