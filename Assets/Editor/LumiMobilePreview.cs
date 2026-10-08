using LumiAdventure;
using UnityEditor;
using UnityEngine;

public static class LumiMobilePreview
{
    private const string MenuPath="Naruto/Mobile Preview/Simulate Mobile Controls";

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        bool enable=LumiControlScheme.LoadSavedMode()!=LumiControlMode.Mobile;
        LumiControlScheme.SaveMode(enable?LumiControlMode.Mobile:LumiControlMode.Auto);
        Menu.SetChecked(MenuPath,enable);
        Debug.Log("Mobile controls preview: "+(enable?"ON":"OFF")+". Enter Play Mode to apply.");
    }

    [MenuItem(MenuPath,true)]
    private static bool Validate()
    {
        Menu.SetChecked(MenuPath,LumiControlScheme.LoadSavedMode()==LumiControlMode.Mobile);
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
