using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class MenuInputs : MonoBehaviour
    {
        public bool openMenu;

        public void OnOpenMenu(InputValue value)
        {
            openMenu = value.isPressed;
        }
    }
}