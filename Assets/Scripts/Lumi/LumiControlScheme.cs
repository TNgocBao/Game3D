using UnityEngine;

namespace LumiAdventure
{
    public enum LumiControlMode { Auto, PC, Mobile }

    public sealed class LumiControlScheme:MonoBehaviour
    {
        private const string PreferenceKey="Lumi.ControlMode";
        public LumiControlMode Mode{get;private set;}
        public bool UsesMobileControls=>Mode==LumiControlMode.Mobile||(Mode==LumiControlMode.Auto&&Application.isMobilePlatform);
        public bool UsesPcControls=>Mode==LumiControlMode.PC||(Mode==LumiControlMode.Auto&&!Application.isMobilePlatform);
        public string DisplayName=>Mode==LumiControlMode.Auto?"TỰ ĐỘNG":Mode==LumiControlMode.PC?"PC":"MOBILE";

        public void Initialize(){Mode=LoadSavedMode();}
        public void Cycle(){SetMode((LumiControlMode)(((int)Mode+1)%3));}
        public void SetMode(LumiControlMode value)
        {
            Mode=value;SaveMode(value);LumiMobileInput.Reset();
        }
        public static LumiControlMode LoadSavedMode()=>
            (LumiControlMode)Mathf.Clamp(PlayerPrefs.GetInt(PreferenceKey,(int)LumiControlMode.Auto),0,2);
        public static void SaveMode(LumiControlMode value)
        {
            PlayerPrefs.SetInt(PreferenceKey,(int)value);PlayerPrefs.Save();
        }
    }
}
