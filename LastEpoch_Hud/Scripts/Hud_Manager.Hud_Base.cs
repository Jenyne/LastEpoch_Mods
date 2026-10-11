using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Hud_Base
    {
        public static GameObject Default_PauseMenu_Btns = null;
        public static Button Btn_Resume;

        public static bool Get_DefaultPauseMenu()
        {
            bool result = false;
            if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            {
                GameObject root = null;
                if (!Refs_Manager.game_uibase.bottomScreenMenu.IsNullOrDestroyed())
                {
                    root = Refs_Manager.game_uibase.bottomScreenMenu.gameObject;
                }
                if (!root.IsNullOrDestroyed())
                {
                    // The old pause panel lived under "Menu Image". The bottom bar does not.
                    // Only treat this object as the pause menu when that panel is actually there,
                    // otherwise the title screen looks paused and clicks (Play Offline) are blocked.
                    GameObject menuImage = FindDirectChild(root, "Menu Image");
                    if (!menuImage.IsNullOrDestroyed())
                    {
                        game_pause_menu = root;
                        Default_PauseMenu_Btns = menuImage;
                        Get_Refs();
                        result = true;
                    }
                }
            }

            return result;
        }

        static GameObject FindDirectChild(GameObject obj, string name)
        {
            if (obj.IsNullOrDestroyed())
            {
                return null;
            }
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        public static bool Get_DefaultPauseMenu_Open()
        {
            if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
            {
                return Default_PauseMenu_Btns.active;
            }
            else
            {
                return false;
            }
        }

        public static void Toogle_DefaultPauseMenu(bool show)
        {
            if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
            {
                Default_PauseMenu_Btns.active = show;
            }
        }

        public static void Get_Refs()
        {
            if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
            {
                GameObject Btns = Functions.GetChild(Default_PauseMenu_Btns, "Buttons");
                if (!Btns.IsNullOrDestroyed())
                {
                    GameObject resume = Functions.GetChild(Btns, "ResumeButton (1)", false);
                    if (!resume.IsNullOrDestroyed())
                    {
                        Hud_Base.Btn_Resume = resume.GetComponent<Button>();
                    }
                }
            }
        }

        public static void Resume_Click()
        {
            if (!Btn_Resume.IsNullOrDestroyed())
            {
                Btn_Resume.onClick.Invoke();
            }
        }
    }
}
