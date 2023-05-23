using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace InputSystem
{
    public class ThirdInputs : MonoBehaviour
    {
        [Header("Character Input Values")] public Vector2 move;

        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool pickup;
        public bool menuOpen;
        public bool tapFire;
        public bool holdFire;
        public bool reload;
        public bool aiming;

        [Header("Movement Settings")] public bool analogMovement;

        [Header("Mouse Cursor Settings")] public bool cursorLocked;

        public bool cursorInputForLook = true;

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            switch (context)
            {
                case { performed: true, interaction: HoldInteraction }:
                    tapFire = true;
                    holdFire = true;
                    break;
                case { performed: true, interaction: TapInteraction }:
                    tapFire = true;
                    break;
                case { canceled: true, interaction: HoldInteraction }:
                    holdFire = false;
                    break;
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (cursorInputForLook)
                look = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            jump = context.performed;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            sprint = context.performed;
        }

        public void OnPickUp(InputAction.CallbackContext context)
        {
            pickup = context.performed;
        }

        public void OnOpenMenu(InputAction.CallbackContext context)
        {
            menuOpen = context.performed;
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            reload = context.performed;
        }

        public void OnAiming(InputAction.CallbackContext context)
        {
            aiming = context.performed;
        }

        // todo: finish method
        public int GetSwitchWeaponInput()
        {
            return 0;
        }

        // todo: finish method
        public int GetSelectWeaponInput()
        {
            return 0;
        }
    }
}