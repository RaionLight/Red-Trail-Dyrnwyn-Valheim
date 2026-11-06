using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace RedTrailDyrnwyn
{
    [BepInPlugin("com.sbo.redtraildyrnwyn", "Red Trail Dyrnwyn Mod", "1.0.0")]
    public class TrailModifierPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony harmony = new Harmony("com.sbo.redtraildyrnwyn");
            harmony.PatchAll();
            Logger.LogInfo("Red Trail Dyrnwyn 模組已成功載入！");
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDB_Awake_Patch
        {
            public static void Postfix() => ModifyDyrnwynTrail();
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetScene_Awake_Patch
        {
            public static void Postfix() => ModifyDyrnwynTrail();
        }

        private static bool s_isModified = false;

        private static void ModifyDyrnwynTrail()
        {
            if (s_isModified || ZNetScene.instance == null) return;

            GameObject swordPrefab = ZNetScene.instance.GetPrefab("SwordDyrnwyn");
            if (swordPrefab == null) return;

            // 尋找 SwordDyrnwyn 底下所有的 MeleeWeaponTrail 元件
            foreach (var comp in swordPrefab.GetComponentsInChildren<Component>(true))
            {
                if (comp != null && comp.GetType().Name == "MeleeWeaponTrail")
                {
                    // 直接透過反射抓取 _material 欄位進行替換
                    var materialField = comp.GetType().GetField("_material", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (materialField != null)
                    {
                        Material redTrailMat = GetGameMaterial("red_trail");
                        if (redTrailMat != null)
                        {
                            materialField.SetValue(comp, redTrailMat);
                            Debug.Log("[RedTrailDyrnwyn] 成功將 SwordDyrnwyn 的軌跡材質替換為 red_trail！");
                            s_isModified = true;
                            break;
                        }
                    }
                }
            }
        }

        private static Material GetGameMaterial(string matName)
        {
            foreach (Material mat in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (mat.name == matName) return mat;
            }
            return null;
        }
    }
}