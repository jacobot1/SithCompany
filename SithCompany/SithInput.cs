using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace SithCompany
{
    public class SithInput : LcInputActions
    {
        // Set up force lightning keybind
        [InputAction(KeyboardControl.R, Name = "ForceLightning")]
        public InputAction LightningButton { get; set; }

        // Set up force mode keybind
        [InputAction(KeyboardControl.J, Name = "ForceMode")]
        public InputAction ForceModeButton { get; set; }

        // Set up use the force keybind
        [InputAction(KeyboardControl.V, Name = "UseTheForce")]
        public InputAction UseTheForceButton { get; set; }
    }
}
