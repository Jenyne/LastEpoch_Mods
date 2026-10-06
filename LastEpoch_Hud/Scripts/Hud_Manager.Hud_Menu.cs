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
    public class Hud_Menu
    {
        public static void Set_Events()
        {
            if (!hud_object.IsNullOrDestroyed())
            {
                Events.Set_Base_Button_Event(
                    hud_object,
                    "Menu",
                    "Btn_Menu_Character",
                    Character_OnClick_Action
                );
                Events.Set_Base_Button_Event(
                    hud_object,
                    "Menu",
                    "Btn_Menu_Items",
                    Items_OnClick_Action
                );
                Events.Set_Base_Button_Event(
                    hud_object,
                    "Menu",
                    "Btn_Menu_Scenes",
                    Scenes_OnClick_Action
                );
                Events.Set_Base_Button_Event(
                    hud_object,
                    "Menu",
                    "Btn_Menu_TreeSkills",
                    Skills_OnClick_Action
                );
                Events.Set_Base_Button_Event(
                    hud_object,
                    "Menu",
                    "Btn_Menu_ForceDrop",
                    OldForceDrop_OnClick_Action
                );
                // Optional unfinished page: only wire/show the button when the
                // donor HUD bundle actually contains Headhunter content.
                var headhunterButton = Functions.FindDescendant(hud_object, "Btn_Menu_Headhunter");
                var headhunterContent = Functions.FindDescendant(hud_object, "Headhunter_Content");
                if (!headhunterButton.IsNullOrDestroyed())
                {
                    headhunterButton.SetActive(!headhunterContent.IsNullOrDestroyed());
                    if (!headhunterContent.IsNullOrDestroyed())
                    {
                        var button = headhunterButton.GetComponent<Button>();
                        if (!button.IsNullOrDestroyed())
                        {
                            button.onClick = new Button.ButtonClickedEvent();
                            button.onClick.AddListener(Headhunter_OnClick_Action);
                        }
                    }
                }
            }
        }

        private static readonly System.Action Character_OnClick_Action = new System.Action(
            Character_Click
        );

        public static void Character_Click()
        {
            Content.Items.Set_Active(false);
            Content.Scenes.Set_Active(false);
            Content.Skills.Set_Active(false);
            Content.OdlForceDrop.Set_Active(false);
            Content.Headhunter.Set_Active(false);
            Content.Character.Set_Active(true);
        }

        private static readonly System.Action Items_OnClick_Action = new System.Action(Items_Click);

        public static void Items_Click()
        {
            Content.Character.Set_Active(false);
            Content.Scenes.Set_Active(false);
            Content.Skills.Set_Active(false);
            Content.OdlForceDrop.Set_Active(false);
            Content.Headhunter.Set_Active(false);
            Content.Items.Set_Active(true);
        }

        private static readonly System.Action Scenes_OnClick_Action = new System.Action(
            Scenes_Click
        );

        public static void Scenes_Click()
        {
            Content.Character.Set_Active(false);
            Content.Items.Set_Active(false);
            Content.Skills.Set_Active(false);
            Content.OdlForceDrop.Set_Active(false);
            Content.Headhunter.Set_Active(false);
            Content.Scenes.Set_Active(true);
        }

        private static readonly System.Action Skills_OnClick_Action = new System.Action(
            Skills_Click
        );

        public static void Skills_Click()
        {
            Content.Character.Set_Active(false);
            Content.Items.Set_Active(false);
            Content.Scenes.Set_Active(false);
            Content.OdlForceDrop.Set_Active(false);
            Content.Headhunter.Set_Active(false);
            Content.Skills.Set_Active(true);
        }

        private static readonly System.Action OldForceDrop_OnClick_Action = new System.Action(
            OldForceDrop_Click
        );

        public static void OldForceDrop_Click()
        {
            Content.Character.Set_Active(false);
            Content.Items.Set_Active(false);
            Content.Scenes.Set_Active(false);
            Content.Skills.Set_Active(false);
            Content.Headhunter.Set_Active(false);
            Content.OdlForceDrop.Set_Active(true);
        }

        private static readonly System.Action Headhunter_OnClick_Action = new System.Action(
            Headhunter_Click
        );

        public static void Headhunter_Click()
        {
            Content.Character.Set_Active(false);
            Content.Items.Set_Active(false);
            Content.Scenes.Set_Active(false);
            Content.Skills.Set_Active(false);
            Content.OdlForceDrop.Set_Active(false);
            Content.Headhunter.Set_Active(true);
        }
    }
}
