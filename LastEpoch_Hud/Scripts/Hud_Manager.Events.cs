using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Events
    {
        public static void Set_Button_Event(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn.IsNullOrDestroyed() || action == null)
            {
                return;
            }
            Button.ButtonClickedEvent click = btn.onClick;
            if (click == null)
            {
                click = new Button.ButtonClickedEvent();
                btn.onClick = click;
            }
            click.AddListener(action);
        }

        public static void Set_Slider_Event(
            Slider slider,
            UnityEngine.Events.UnityAction<float> action
        )
        {
            slider.onValueChanged = new Slider.SliderEvent();
            slider.onValueChanged.AddListener(action);
        }

        public static void Set_Input_Event(
            Il2CppTMPro.TMP_InputField input,
            UnityEngine.Events.UnityAction<string> action
        )
        {
            if (input.IsNullOrDestroyed() || action == null)
            {
                return;
            }
            input.onEndEdit = new Il2CppTMPro.TMP_InputField.SubmitEvent();
            input.onEndEdit.AddListener(action);
        }

        public static void Set_Toggle_Event(
            Toggle toggle,
            UnityEngine.Events.UnityAction<bool> action
        )
        {
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener(action);
        }
    }
}
