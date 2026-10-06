using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppRewired.Components;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Events
    {
        public static void Set_Base_Button_Event(
            GameObject base_obj,
            string child,
            string btn_name,
            UnityEngine.Events.UnityAction action
        )
        {
            if (!base_obj.IsNullOrDestroyed())
            {
                GameObject go = Functions.GetChild(base_obj, child);
                if (go.IsNullOrDestroyed())
                {
                    go = Functions.FindDescendant(base_obj, child);
                }
                if (!go.IsNullOrDestroyed())
                {
                    GameObject btn_obj = Functions.FindDescendant(go, btn_name);
                    if (!btn_obj.IsNullOrDestroyed())
                    {
                        Button btn = btn_obj.GetComponent<Button>();
                        if (btn.IsNullOrDestroyed())
                        {
                            btn = btn_obj.GetComponentInChildren<Button>(true);
                        }
                        if (!btn.IsNullOrDestroyed())
                        {
                            Set_Button_Event(btn, action);
                        }
                        else
                        {
                            Main.logger_instance.Error("Set_Base_Button_Event Can't found button");
                        }
                    }
                    else
                    {
                        Main.logger_instance.Error(
                            "Set_Base_Button_Event Can't found GameObject button " + btn_name
                        );
                    }
                }
                else
                {
                    Main.logger_instance.Error(
                        "Set_Base_Button_Event Can't found " + child + " in base_obj"
                    );
                }
            }
            else
            {
                Main.logger_instance.Error("Set_Base_Button_Event base_obj is null");
            }
        }

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
