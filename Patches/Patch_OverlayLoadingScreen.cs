using HarmonyLib;
using Il2Cpp;
using Il2CppUMUI.UiElements;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace DDSS_ConnectionFix.Patches
{
    [HarmonyPatch]
    internal class Patch_OverlayLoadingScreen
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OverlayLoadingScreen), nameof(OverlayLoadingScreen.SetLoadingScreen))]
        private static void SetLoadingScreen_Postfix(OverlayLoadingScreen __instance, UnityAction __1)
        {
            Transform cancelButtonTrans = __instance.transform.Find("Tab/Tasks/TopBar/Close");
            if ((cancelButtonTrans == null)
                || cancelButtonTrans.WasCollected)
                return;

            UMUIButton button = cancelButtonTrans.gameObject.GetComponent<UMUIButton>();
            if ((button == null)
                || button.WasCollected)
                return;

            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(__1);
            button.OnClick.AddListener(new Action(__instance.Cancel));
        }
    }
}
