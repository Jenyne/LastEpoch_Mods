using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Summon
{
    [RegisterTypeInIl2Cpp]
    public class Summon_Options : MonoBehaviour
    {
        public Summon_Options(IntPtr ptr) : base(ptr) { }
        static Toggle godMode, forever, dontCollide;
        // Read the control itself: callback arguments can report true when toggled off.
        static readonly Action<bool> GodModeChanged = value => {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !godMode.IsNullOrDestroyed())
            { Save_Manager.instance.data.Summon.Enable_GodMode = godMode.isOn; }
        };
        static readonly Action<bool> ForeverChanged = value => {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !forever.IsNullOrDestroyed())
            { Save_Manager.instance.data.Summon.Enable_Forever = forever.isOn; }
        };
        static readonly Action<bool> DontCollideChanged = value => {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !dontCollide.IsNullOrDestroyed())
            { Save_Manager.instance.data.Summon.Enable_DontCollide = dontCollide.isOn; }
        };

        public static void BindUI(GameObject content)
        {
            godMode = Bind(content, "SummonGodMode", GodModeChanged);
            forever = Bind(content, "SummonForever", ForeverChanged);
            dontCollide = Bind(content, "SummonDontCollide", DontCollideChanged);
        }
        static Toggle Bind(GameObject content, string rowName, Action<bool> action)
        {
            GameObject row = Functions.GetChild(content, rowName);
            if (row.IsNullOrDestroyed()) { return null; }
            row.SetActive(true);
            Toggle toggle = row.GetComponentInChildren<Toggle>(true);
            if (!toggle.IsNullOrDestroyed()) { Hud_Manager.Events.Set_Toggle_Event(toggle, action); }
            return toggle;
        }
        public static void RefreshUI()
        {
            if (Save_Manager.instance.IsNullOrDestroyed()) { return; }
            var options = Save_Manager.instance.data.Summon;
            if (!godMode.IsNullOrDestroyed()) { godMode.SetIsOnWithoutNotify(options.Enable_GodMode); }
            if (!forever.IsNullOrDestroyed()) { forever.SetIsOnWithoutNotify(options.Enable_Forever); }
            if (!dontCollide.IsNullOrDestroyed()) { dontCollide.SetIsOnWithoutNotify(options.Enable_DontCollide); }
        }

        readonly Dictionary<BaseHealth, bool> health = new Dictionary<BaseHealth, bool>();
        readonly Dictionary<NavMeshAgent, float> radii = new Dictionary<NavMeshAgent, float>();

        void RestoreHealth()
        {
            foreach (var entry in health) { if (!entry.Key.IsNullOrDestroyed()) { entry.Key.damageable = entry.Value; } }
            health.Clear();
        }
        void RestoreRadii()
        {
            foreach (var entry in radii) { if (!entry.Key.IsNullOrDestroyed()) { entry.Key.radius = entry.Value; } }
            radii.Clear();
        }
        void OnDisable() { RestoreHealth(); RestoreRadii(); }

        void Update()
        {
            bool ready = Scenes.IsGameScene() && !Save_Manager.instance.IsNullOrDestroyed()
                && Save_Manager.instance.initialized && !Refs_Manager.player_actor.IsNullOrDestroyed();
            if (!ready) { OnDisable(); return; }
            var options = Save_Manager.instance.data.Summon;
            if (!options.Enable_GodMode) { RestoreHealth(); }
            if (!options.Enable_DontCollide) { RestoreRadii(); }
            if (!options.Enable_GodMode && !options.Enable_DontCollide && !options.Enable_Forever) { return; }
            SummonTracker tracker = null;
            if (!Refs_Manager.player_actor.TryGetExistingSummonTracker(out tracker) || tracker.IsNullOrDestroyed()) { return; }
            if (tracker.summons == null) { return; }
            foreach (Summoned summoned in tracker.summons)
            {
                if (summoned.IsNullOrDestroyed() || summoned.gameObject.IsNullOrDestroyed()) { continue; }
                if (options.Enable_GodMode)
                {
                    UnitHealth unit = summoned.gameObject.GetComponent<UnitHealth>();
                    BaseHealth target = unit.IsNullOrDestroyed() ? null : unit.TryCast<BaseHealth>();
                    if (!target.IsNullOrDestroyed())
                    {
                        if (!health.ContainsKey(target)) { health.Add(target, target.damageable); }
                        target.damageable = false;
                    }
                }
                if (options.Enable_DontCollide)
                {
                    NavMeshAgent agent = summoned.gameObject.GetComponent<NavMeshAgent>();
                    if (!agent.IsNullOrDestroyed())
                    {
                        if (!radii.ContainsKey(agent)) { radii.Add(agent, agent.radius); }
                        agent.radius = 0f;
                    }
                }
                if (options.Enable_Forever)
                {
                    UnsummonAfterDelay delay = summoned.gameObject.GetComponent<UnsummonAfterDelay>();
                    // Match the original Forever option: remove the lifetime component.
                    // Existing summons stay permanent after this option is switched off.
                    if (!delay.IsNullOrDestroyed()) { UnityEngine.Object.Destroy(delay); }
                }
            }
        }
    }
}
