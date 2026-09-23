using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HexMapDiscovery
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.mapdiscovery";
        private const string PluginName = "HexMapDiscovery";
        private const string PluginVersion = "1.0.1";
        private const float DefaultExplorationRadiusMultiplier = 3f;

        private Harmony _harmonyInstance;

        private static ConfigEntry<bool> IsModEnabled = null;
        private static ConfigEntry<float> ExplorationRadiusMultiplier = null;

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            IsModEnabled = Config.Bind("General", "Enabled", true, "Enable or disable the mod.");

            ExplorationRadiusMultiplier = Config.Bind(
                "Exploration",
                "ExplorationRadiusMultiplier",
                DefaultExplorationRadiusMultiplier,
                new ConfigDescription(
                    "Multiplier for the exploration radius while piloting a ship.",
                    new AcceptableValueRange<float>(1f, 10f)
                )
            );

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Log = null;
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), typeof(Vector3), typeof(float))]
        internal static class PatchMinimapExplore
        {
            private static readonly FieldInfo AttachedField = typeof(Player).GetField("m_attached", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            private static readonly FieldInfo AttachedToShipField = typeof(Player).GetField("m_attachedToShip", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static void Prefix(ref float radius)
            {
                if (!IsModEnabled.Value || Player.m_localPlayer == null)
                {
                    return;
                }

                Player player = Player.m_localPlayer;

                if (!IsAttachedToShip(player))
                {
                    return;
                }

                // Clamp the multiplier to safeguard against invalid config values.
                float multiplier = Mathf.Clamp(ExplorationRadiusMultiplier.Value, 1f, 10f);
                radius *= multiplier;
            }

            private static bool IsAttachedToShip(Player player)
            {
                if (player == null || AttachedField == null || AttachedToShipField == null)
                {
                    return false;
                }

                bool attached = (bool)AttachedField.GetValue(player);
                bool attachedToShip = (bool)AttachedToShipField.GetValue(player);

                return attached && attachedToShip;
            }
        }
    }
}