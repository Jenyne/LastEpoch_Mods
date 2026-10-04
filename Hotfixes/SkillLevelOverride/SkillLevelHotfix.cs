using HarmonyLib;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

[assembly: MelonInfo(typeof(LastEpochSkillLevelHotfix.SkillLevelHotfix), "LastEpoch Skill Level Hotfix", "0.1.0", "Jenyne/OpenAI")]
[assembly: MelonGame("Eleventh Hour Games", "Last Epoch")]

namespace LastEpochSkillLevelHotfix
{
    public sealed class SkillLevelHotfix : MelonMod
    {
        private static bool _enabled;
        private static int _level = 20;
        private static string _configPath;
        private static DateTime _lastWriteUtc;
        private static float _nextConfigCheck;
        private Harmony _harmony;

        public override void OnInitializeMelon()
        {
            _configPath = Path.Combine(Directory.GetCurrentDirectory(), "Mods", "LastEpoch_Hud", "Save.json");
            LoadConfig(force: true);

            _harmony = new Harmony("jenyne.lastepoch.skilllevelhotfix");
            PatchAbilityLevelMethods();

            LoggerInstance.Msg($"Skill-level hotfix loaded. HUD config: {_configPath}");
            LoggerInstance.Msg($"Initial state: enabled={_enabled}, level={_level}");
        }

        public override void OnUpdate()
        {
            if (UnityEngine.Time.unscaledTime < _nextConfigCheck) return;
            _nextConfigCheck = UnityEngine.Time.unscaledTime + 0.5f;
            LoadConfig(force: false);
        }

        private void PatchAbilityLevelMethods()
        {
            int patched = 0;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (Type type in types)
                {
                    if (type == null) continue;

                    // LE 1.5 still exposes these generated IL2CPP wrapper type names.
                    if (type.Name != "SpecialisedAbilityManager" && type.Name != "LocalTreeData")
                        continue;

                    MethodInfo[] methods;
                    try
                    {
                        methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                  BindingFlags.Static | BindingFlags.Instance);
                    }
                    catch { continue; }

                    foreach (MethodInfo method in methods)
                    {
                        if (method.Name != "getAbilityLevel") continue;

                        HarmonyMethod postfix = null;
                        if (method.ReturnType == typeof(byte))
                            postfix = new HarmonyMethod(typeof(SkillLevelHotfix), nameof(PostfixByte));
                        else if (method.ReturnType == typeof(int))
                            postfix = new HarmonyMethod(typeof(SkillLevelHotfix), nameof(PostfixInt));
                        else
                            continue;

                        try
                        {
                            _harmony.Patch(method, postfix: postfix);
                            patched++;
                            LoggerInstance.Msg($"Patched {type.FullName}.{method.Name} -> {method.ReturnType.Name} ({FormatParameters(method)})");
                        }
                        catch (Exception ex)
                        {
                            LoggerInstance.Warning($"Could not patch {type.FullName}.{method.Name}: {ex.GetType().Name}: {ex.Message}");
                        }
                    }
                }
            }

            if (patched == 0)
                LoggerInstance.Error("No compatible getAbilityLevel methods were found. Send Latest.log; the game API changed more than expected.");
            else
                LoggerInstance.Msg($"Patched {patched} ability-level method(s).");
        }

        private static string FormatParameters(MethodInfo method)
        {
            try
            {
                return string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
            }
            catch { return "?"; }
        }

        public static void PostfixByte(ref byte __result)
        {
            if (!_enabled) return;
            __result = (byte)Math.Clamp(_level, byte.MinValue, byte.MaxValue);
        }

        public static void PostfixInt(ref int __result)
        {
            if (!_enabled) return;
            __result = Math.Clamp(_level, byte.MinValue, byte.MaxValue);
        }

        private void LoadConfig(bool force)
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                DateTime write = File.GetLastWriteTimeUtc(_configPath);
                if (!force && write == _lastWriteUtc) return;
                _lastWriteUtc = write;

                using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(_configPath));
                JsonElement root = doc.RootElement;

                if (!TryGetPropertyRecursive(root, "Skills", out JsonElement skills))
                    return;

                bool enabled = _enabled;
                int level = _level;

                if (TryGetBool(skills, "Enable_SkillLevel", out bool parsedEnabled))
                    enabled = parsedEnabled;

                if (TryGetNumber(skills, "SkillLevel", out int parsedLevel))
                    level = Math.Clamp(parsedLevel, byte.MinValue, byte.MaxValue);

                bool changed = enabled != _enabled || level != _level;
                _enabled = enabled;
                _level = level;

                if (changed)
                    LoggerInstance.Msg($"HUD skill-level setting changed: enabled={_enabled}, level={_level}");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning($"Could not read HUD Save.json: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static bool TryGetPropertyRecursive(JsonElement element, string name, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }

                    if (TryGetPropertyRecursive(property.Value, name, out value))
                        return true;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                    if (TryGetPropertyRecursive(child, name, out value))
                        return true;
            }

            value = default;
            return false;
        }

        private static bool TryGetBool(JsonElement obj, string name, out bool value)
        {
            value = false;
            if (obj.ValueKind != JsonValueKind.Object) return false;

            foreach (JsonProperty property in obj.EnumerateObject())
            {
                if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                if (property.Value.ValueKind == JsonValueKind.True) { value = true; return true; }
                if (property.Value.ValueKind == JsonValueKind.False) { value = false; return true; }
            }
            return false;
        }

        private static bool TryGetNumber(JsonElement obj, string name, out int value)
        {
            value = 0;
            if (obj.ValueKind != JsonValueKind.Object) return false;

            foreach (JsonProperty property in obj.EnumerateObject())
            {
                if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) continue;

                if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out value))
                    return true;

                if (property.Value.ValueKind == JsonValueKind.String &&
                    int.TryParse(property.Value.GetString(), out value))
                    return true;
            }
            return false;
        }
    }
}
