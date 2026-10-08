using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private GameObject settingsPanel;
        private Slider masterControl,musicControl,effectsControl;
        private Text masterPercent,musicPercent,effectsPercent,muteLabel,controlModeLabel;
        private bool settingsResumesGame;

        private void BuildGameSettings()
        {
            settingsPanel=Panel("Audio Settings",new Color(.01f,.02f,.04f,.82f));
            settingsPanel.GetComponent<Image>().raycastTarget=true;
            Image board=UiSurfacePanel(settingsPanel.transform,"Game preferences",new Vector2(980,920),Vector2.zero,UiSurface);
            UiLabel(board.transform,"SETTING",46,Color.white,new Vector2(.5f,.91f),new Vector2(850,80),Vector2.zero,TextAnchor.MiddleCenter);
            UiLabel(board.transform,"Điều khiển và âm thanh · Tự lưu",26,new Color(.7f,.82f,.88f),new Vector2(.5f,.83f),new Vector2(850,45),Vector2.zero,TextAnchor.MiddleCenter);
            masterControl=AudioSlider(board.transform,"Âm lượng tổng",.70f,v=>Audio.MasterVolume=v,out masterPercent);
            musicControl=AudioSlider(board.transform,"Nhạc nền",.57f,v=>Audio.MusicVolume=v,out musicPercent);
            effectsControl=AudioSlider(board.transform,"Hiệu ứng âm thanh",.44f,v=>Audio.EffectsVolume=v,out effectsPercent);
            Button controls=UiAction(board.transform,"",new Vector2(820,64),new Vector2(0,-238),new Color(.10f,.24f,.32f),()=>{Controls.Cycle();UpdateControlModeLabel();ApplyControlScheme();Audio.Play("ui");});
            controlModeLabel=controls.GetComponentInChildren<Text>();
            Button mute=UiAction(board.transform,"",new Vector2(820,60),new Vector2(0,-318),new Color(.12f,.155f,.22f),()=>{Audio.Muted=!Audio.Muted;UpdateMuteLabel();});
            muteLabel=mute.GetComponentInChildren<Text>();
            Button reset=UiAction(board.transform,"MẶC ĐỊNH",new Vector2(390,64),new Vector2(-215,-357),new Color(.12f,.155f,.22f),()=>{
                Audio.MasterVolume=1;Audio.MusicVolume=.55f;Audio.EffectsVolume=.65f;Audio.Muted=false;Controls.SetMode(LumiControlMode.Auto);ApplyControlScheme();RefreshAudioControls();Audio.Play("ui");});
            LumiFactory.Rect(reset.GetComponent<RectTransform>(),new Vector2(.5f,.055f),new Vector2(390,64),new Vector2(-215,0));
            Button close=UiAction(board.transform,"LƯU & ĐÓNG",new Vector2(390,64),new Vector2(215,-357),UiOrange,CloseGameSettings);
            LumiFactory.Rect(close.GetComponent<RectTransform>(),new Vector2(.5f,.055f),new Vector2(390,64),new Vector2(215,0));
            settingsPanel.SetActive(false);
        }

        private Slider AudioSlider(Transform parent,string label,float y,UnityEngine.Events.UnityAction<float> apply,out Text percentage)
        {
            UiLabel(parent,label,30,Color.white,new Vector2(.5f,y+.045f),new Vector2(650,45),new Vector2(-85,0));
            percentage=LumiFactory.Text(parent,"",32,TextAnchor.MiddleRight,UiOrange);
            LumiFactory.Rect(percentage.rectTransform,new Vector2(.5f,y+.045f),new Vector2(160,45),new Vector2(330,0));
            Text valueLabel=percentage;
            Image track=LumiFactory.Image(parent,new Color(.025f,.05f,.07f));
            LumiFactory.Rect(track.rectTransform,new Vector2(.5f,y-.017f),new Vector2(820,32),Vector2.zero);
            Image fill=LumiFactory.Image(track.transform,UiOrange);LumiFactory.Stretch(fill.rectTransform,4);
            fill.raycastTarget=false;
            Image handle=LumiFactory.Image(track.transform,Color.white);handle.sprite=RoundedPanelSprite();handle.type=Image.Type.Sliced;
            handle.rectTransform.sizeDelta=new Vector2(24,44);handle.raycastTarget=false;
            Slider slider=track.gameObject.AddComponent<Slider>();
            slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
            slider.minValue=0;slider.maxValue=1;
            slider.onValueChanged.AddListener(v=>{apply(v);valueLabel.text=Mathf.RoundToInt(v*100)+"%";});
            return slider;
        }

        public void OpenGameSettings()
        {
            if(state==GameState.Loading)return;
            settingsResumesGame=state==GameState.Playing;
            if(settingsResumesGame)PauseGame();
            RefreshAudioControls();settingsPanel.SetActive(true);settingsPanel.transform.SetAsLastSibling();
            Audio.Play("ui");
        }
        private void UpdateMuteLabel(){muteLabel.text=Audio.Muted?"ÂM THANH: ĐANG TẮT  ·  BẤM ĐỂ BẬT":"ÂM THANH: ĐANG BẬT  ·  BẤM ĐỂ TẮT";}
        private void UpdateControlModeLabel(){controlModeLabel.text="KIỂU ĐIỀU KHIỂN: "+Controls.DisplayName+"  ·  BẤM ĐỂ ĐỔI";}
        private void RefreshAudioControls()
        {
            masterControl.SetValueWithoutNotify(Audio.MasterVolume);musicControl.SetValueWithoutNotify(Audio.MusicVolume);effectsControl.SetValueWithoutNotify(Audio.EffectsVolume);
            masterPercent.text=Mathf.RoundToInt(Audio.MasterVolume*100)+"%";musicPercent.text=Mathf.RoundToInt(Audio.MusicVolume*100)+"%";effectsPercent.text=Mathf.RoundToInt(Audio.EffectsVolume*100)+"%";UpdateMuteLabel();UpdateControlModeLabel();
        }
        private void CloseGameSettings()
        {
            Audio.SaveSettings();settingsPanel.SetActive(false);Audio.Play("ui");
            if(settingsResumesGame)ResumeGame();settingsResumesGame=false;
        }
    }
}
