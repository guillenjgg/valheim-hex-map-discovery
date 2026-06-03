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
        private const string PluginVersion = "1.0.0";
        private const float DefaultExplorationRadiusMultiplier = 3f;

        private Harmony _harmonyInstance;
        private ConfigEntry<bool> _isModEnabled;
        private ConfigEntry<float> _explorationRadiusMultiplier;

        internal static ManualLogSource Log;
        internal static Plugin Instance;
        internal static bool IsModEnabled => Instance != null && (Instance._isModEnabled?.Value ?? false);
        internal static float ExplorationRadiusMultiplier => Instance != null ? (Instance._explorationRadiusMultiplier?.Value ?? 1f) : 1f;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _isModEnabled = Config.Bind("General", "Enabled", true, "Enable or disable the mod.");
            _explorationRadiusMultiplier = Config.Bind(
                "Exploration",
                "ExplorationRadiusMultiplier",
                DefaultExplorationRadiusMultiplier,
                new ConfigDescription(
                    "Multiplier for the exploration radius while piloting a ship.",
                    new AcceptableValueRange<float>(1f, 10f)
                )
            );

            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll();

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), typeof(Vector3), typeof(float))]
        internal static class PatchMinimapExplore
        {
            private static readonly FieldInfo AttachedField = typeof(Player).GetField("m_attached", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            private static readonly FieldInfo AttachedToShipField = typeof(Player).GetField("m_attachedToShip", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            private static void Prefix(ref float radius)
            {
                if(!Plugin.IsModEnabled || Player.m_localPlayer == null)
                {
                    return;
                }

                var player = Player.m_localPlayer;

                if (!IsAttachedToShip(player))
                {
                    return;
                }

                // clamp the multiplier to safeguard against invalid config values
                float multiplier = Mathf.Clamp(Plugin.ExplorationRadiusMultiplier, 1f, 10f);
                radius *= multiplier;
            }

            private static bool IsAttachedToShip(Player player)
            {
                if (player == null || AttachedField == null || AttachedToShipField == null)
                {
                    return false;
                }

                var attached = (bool)AttachedField.GetValue(player);
                var attachedToShip = (bool)AttachedToShipField.GetValue(player);

                return attached && attachedToShip;
            }
        }
    }
}
