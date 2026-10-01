using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private static readonly string[] ReferenceNames={"Forest","Desert","Ice","Dungeon","Facility"};
        private GameObject loadingPanel;
        private bool deploying;
        private int selectedMission=1;

        private RawImage ReferenceImage(Transform parent,string key,Vector2 anchor,Vector2 size,Vector2 offset)
        {
            var obj=new GameObject("Reference art - "+key,typeof(RectTransform),typeof(CanvasRenderer),typeof(LumiCoverImage));
            obj.transform.SetParent(parent,false);
            RawImage img=obj.GetComponent<RawImage>();
            img.texture=Resources.Load<Texture2D>("LumiReference/"+key);
            img.raycastTarget=false;
            LumiFactory.Rect(img.rectTransform,anchor,size,offset);
            ((LumiCoverImage)img).UpdateCrop();
            return img;
        }

        private void UiLabel(Transform parent,string value,int size,Color color,Vector2 anchor,Vector2 bounds,Vector2 offset,TextAnchor alignment=TextAnchor.MiddleLeft)
        {
            Text label=LumiFactory.Text(parent,value,size,alignment,color);
            label.raycastTarget=false;
            LumiFactory.Rect(label.rectTransform,anchor,bounds,offset);
        }

        private void BuildReferenceMenu()
        {
            LumiFactory.DestroyChildren(menuPanel.transform);
            int unlocked=Mathf.Clamp(PlayerPrefs.GetInt("Lumi.Unlocked",1),1,5);
            selectedMission=Mathf.Clamp(selectedMission,1,unlocked);
            Color gold=new Color(.92f,.77f,.47f);
            Color muted=new Color(.7f,.76f,.76f);
            RawImage background=ReferenceImage(menuPanel.transform,"Camp",new Vector2(.5f,.5f),new Vector2(1920,1080),Vector2.zero);
            LumiFactory.Stretch(background.rectTransform);
            Image shade=LumiFactory.Image(menuPanel.transform,new Color(.025f,.04f,.045f,.78f));
            LumiFactory.Stretch(shade.rectTransform);shade.raycastTarget=false;
            var frame=new GameObject("Mission layout",typeof(RectTransform));
            frame.transform.SetParent(menuPanel.transform,false);
            LumiFactory.Rect(frame.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(1920,1080),Vector2.zero);
            Transform content=frame.transform;
            UiLabel(content,"V A N G U A R D",54,Color.white,new Vector2(.5f,.91f),new Vector2(1700,80),Vector2.zero);
            UiLabel(content,"BIÊN NIÊN SỬ NĂM VÙNG ĐẤT",20,gold,new Vector2(.5f,.855f),new Vector2(1700,35),Vector2.zero);
            Button settings=LumiFactory.Button(content,"CÀI ĐẶT",new Color(.16f,.26f,.29f),OpenAudioSettings);
            LumiFactory.Rect(settings.GetComponent<RectTransform>(),new Vector2(.895f,.91f),new Vector2(240,62),Vector2.zero);

            Image hero=LumiFactory.Image(content,new Color(.06f,.09f,.09f,.96f));
            LumiFactory.Rect(hero.rectTransform,new Vector2(.5f,.6f),new Vector2(1700,450),Vector2.zero);
            ReferenceImage(hero.transform,ReferenceNames[selectedMission-1],new Vector2(.34f,.5f),new Vector2(1150,450),Vector2.zero);
            Image brief=LumiFactory.Image(hero.transform,new Color(.025f,.045f,.055f,.96f));
            LumiFactory.Rect(brief.rectTransform,new Vector2(1,.5f),new Vector2(550,450),new Vector2(-275,0));
            UiLabel(brief.transform,"NHIỆM VỤ "+selectedMission.ToString("00"),19,gold,new Vector2(.5f,.87f),new Vector2(460,35),Vector2.zero);
            UiLabel(brief.transform,LevelName(selectedMission).ToUpperInvariant(),35,Color.white,new Vector2(.5f,.72f),new Vector2(460,110),Vector2.zero);
            UiLabel(brief.transform,"Thu thập 5 lõi sao và tìm cổng cuối bản đồ.\nCẩn thận với các tuyến phục kích.",28,muted,new Vector2(.5f,.48f),new Vector2(460,120),Vector2.zero);
            UiLabel(brief.transform,"THÀNH TÍCH   "+Stars(PlayerPrefs.GetInt("Lumi.Level."+selectedMission+".Stars",0)),24,gold,new Vector2(.5f,.29f),new Vector2(460,45),Vector2.zero);
            Button deploy=LumiFactory.Button(brief.transform,"XUẤT PHÁT   ›",new Color(.31f,.43f,.34f),()=>BeginDeployment(selectedMission));
            LumiFactory.Rect(deploy.GetComponent<RectTransform>(),new Vector2(.5f,.13f),new Vector2(460,66),Vector2.zero);

            for(int i=1;i<=5;i++)
            {
                int mission=i;bool available=i<=unlocked;
                Image card=LumiFactory.Image(content,new Color(.05f,.08f,.09f));
                LumiFactory.Rect(card.rectTransform,new Vector2(.5f,.225f),new Vector2(324,282),new Vector2((i-3)*344,0));
                ReferenceImage(card.transform,ReferenceNames[i-1],new Vector2(.5f,.73f),new Vector2(318,151),Vector2.zero);
                Image veil=LumiFactory.Image(card.transform,new Color(.015f,.025f,.03f,available?.22f:.75f));LumiFactory.Stretch(veil.rectTransform);veil.raycastTarget=false;
                Image footer=LumiFactory.Image(card.transform,new Color(.03f,.055f,.06f,.94f));
                LumiFactory.Rect(footer.rectTransform,new Vector2(.5f,0),new Vector2(324,146),new Vector2(0,73));footer.raycastTarget=false;
                UiLabel(card.transform,LevelName(i),32,available?Color.white:muted,new Vector2(.5f,.355f),new Vector2(300,86),Vector2.zero);
                int bestStars=PlayerPrefs.GetInt("Lumi.Level."+i+".Stars",0);
                UiLabel(card.transform,available?Stars(bestStars)+(bestStars>0?"  ·  ĐÃ XONG":"  ·  SẴN SÀNG"):"KHÓA · QUA VÙNG "+(i-1),24,gold,new Vector2(.5f,.09f),new Vector2(300,36),Vector2.zero);
                if(i==selectedMission){Image line=LumiFactory.Image(card.transform,gold);LumiFactory.Rect(line.rectTransform,new Vector2(.5f,1),new Vector2(324,3),Vector2.zero);line.raycastTarget=false;}
                Button button=card.gameObject.AddComponent<Button>();button.targetGraphic=card;button.interactable=available;
                button.onClick.AddListener(()=>{selectedMission=mission;Audio.Play("ui");BuildReferenceMenu();});
                card.gameObject.AddComponent<LumiUiMotion>().Locked=!available;
            }
            string[] keys={"WASD","CHUỘT TRÁI","CHUỘT PHẢI","CUỘN / V","E / SPACE","ESC"};
            string[] actions={"Di chuyển","Bắn súng","Giữ để xoay","Zoom / Góc nhìn","Nói / Nhảy","Tạm dừng"};
            for(int i=0;i<keys.Length;i++)
            {
                Image hint=LumiFactory.Image(content,new Color(.04f,.075f,.09f,.96f));hint.raycastTarget=false;
                LumiFactory.Rect(hint.rectTransform,new Vector2(.5f,.045f),new Vector2(270,102),new Vector2((i-2.5f)*286,0));
                UiLabel(hint.transform,keys[i],28,gold,new Vector2(.5f,.76f),new Vector2(255,44),Vector2.zero,TextAnchor.MiddleCenter);
                UiLabel(hint.transform,actions[i],28,Color.white,new Vector2(.5f,.25f),new Vector2(255,44),Vector2.zero,TextAnchor.MiddleCenter);
            }
        }

        private void BeginDeployment(int level)
        {
            if(deploying || level>PlayerPrefs.GetInt("Lumi.Unlocked",1))return;
            StartCoroutine(DeploymentRoutine(level));
        }

        private IEnumerator DeploymentRoutine(int level)
        {
            deploying=true;
            state=GameState.Loading;
            loadingPanel=Panel("Mission loading",Color.black);
            loadingPanel.GetComponent<Image>().raycastTarget=true;
            RawImage scene=ReferenceImage(loadingPanel.transform,level==1?"Camp":"Loading",new Vector2(.5f,.5f),new Vector2(1920,1080),Vector2.zero);
            LumiFactory.Stretch(scene.rectTransform);
            Image veil=LumiFactory.Image(loadingPanel.transform,new Color(.02f,.025f,.035f,.44f));LumiFactory.Stretch(veil.rectTransform);veil.raycastTarget=false;
            UiLabel(loadingPanel.transform,"V A N G U A R D",23,Color.white,new Vector2(.5f,.92f),new Vector2(1640,40),Vector2.zero);
            Image info=LumiFactory.Image(loadingPanel.transform,new Color(.025f,.045f,.055f,.94f));
            LumiFactory.Rect(info.rectTransform,new Vector2(.5f,0),new Vector2(1920,220),new Vector2(0,110));
            UiLabel(info.transform,"ĐANG TIẾN VÀO  /  "+LevelName(level).ToUpperInvariant(),32,Color.white,new Vector2(.5f,.69f),new Vector2(1640,55),Vector2.zero);
            UiLabel(info.transform,"Giáp hấp thụ sát thương trước máu. Áo choàng giúp bạn thoát khỏi tầm nhìn kẻ địch trong 5 giây.",20,new Color(.74f,.8f,.81f),new Vector2(.5f,.41f),new Vector2(1640,45),Vector2.zero);
            Image track=LumiFactory.Image(info.transform,new Color(.19f,.25f,.25f));LumiFactory.Rect(track.rectTransform,new Vector2(.5f,.18f),new Vector2(1640,5),Vector2.zero);
            Image fill=LumiFactory.Image(track.transform,new Color(.84f,.74f,.46f));fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchorMin=fill.rectTransform.anchorMax=new Vector2(0,.5f);
            fill.rectTransform.sizeDelta=new Vector2(164,5);
            yield return null;
            fill.rectTransform.sizeDelta=new Vector2(574,5);
            yield return null;
            StartLevel(level);
            state=GameState.Loading;
            loadingPanel.transform.SetAsLastSibling();
            fill.rectTransform.sizeDelta=new Vector2(1640,5);
            yield return null;
            CanvasGroup fade=loadingPanel.AddComponent<CanvasGroup>();
            float elapsed=0;
            while(elapsed<.35f){elapsed+=Time.unscaledDeltaTime;fade.alpha=1-Mathf.Clamp01(elapsed/.35f);yield return null;}
            Destroy(loadingPanel);loadingPanel=null;deploying=false;
            state=GameState.Playing;
        }

        private void BuildMissionReport(bool won,int stars)
        {
            LumiFactory.DestroyChildren(resultPanel.transform);
            resultPanel.GetComponent<Image>().color=new Color(.015f,.025f,.03f,.65f);
            resultPanel.SetActive(true);
            Color gold=new Color(.92f,.77f,.47f);
            Color accent=won?gold:new Color(.94f,.46f,.33f);
            Image board=LumiFactory.Image(resultPanel.transform,new Color(.035f,.06f,.065f,.98f));
            LumiFactory.Rect(board.rectTransform,new Vector2(.5f,.5f),new Vector2(1120,780),Vector2.zero);
            RawImage banner=ReferenceImage(board.transform,ReferenceNames[currentLevel-1],new Vector2(.5f,1),new Vector2(1120,210),new Vector2(0,-105));
            Image bannerShade=LumiFactory.Image(banner.transform,new Color(.02f,.035f,.04f,.62f));LumiFactory.Stretch(bannerShade.rectTransform);bannerShade.raycastTarget=false;
            UiLabel(board.transform,won?"NHIỆM VỤ HOÀN THÀNH":"NHIỆM VỤ THẤT BẠI",40,Color.white,new Vector2(.5f,.91f),new Vector2(1000,65),Vector2.zero,TextAnchor.MiddleCenter);
            UiLabel(board.transform,LevelName(currentLevel).ToUpperInvariant(),20,gold,new Vector2(.5f,.835f),new Vector2(1000,40),Vector2.zero,TextAnchor.MiddleCenter);
            UiLabel(board.transform,won?Stars(stars):"TUYẾN PHÒNG THỦ CHƯA BỊ PHÁ VỠ",won?64:27,accent,new Vector2(.5f,.67f),new Vector2(1000,85),Vector2.zero,TextAnchor.MiddleCenter);
            UiLabel(board.transform,"ĐIỂM  "+score+"     /     LÕI SAO  "+collectedStars+" / "+totalStars,28,Color.white,new Vector2(.5f,.54f),new Vector2(1000,55),Vector2.zero,TextAnchor.MiddleCenter);
            string[] names={"TRINH SÁT","XẠ THỦ","HỘ VỆ"};
            for(int i=0;i<3;i++)
            {
                Image stat=LumiFactory.Image(board.transform,new Color(.085f,.115f,.115f));
                LumiFactory.Rect(stat.rectTransform,new Vector2(.5f,.39f),new Vector2(294,136),new Vector2((i-1)*320,0));stat.raycastTarget=false;
                UiLabel(stat.transform,kills[i].ToString("00"),36,accent,new Vector2(.5f,.63f),new Vector2(260,45),Vector2.zero,TextAnchor.MiddleCenter);
                UiLabel(stat.transform,names[i],32,new Color(.71f,.78f,.77f),new Vector2(.5f,.2f),new Vector2(260,50),Vector2.zero,TextAnchor.MiddleCenter);
            }
            UiLabel(board.transform,won?(currentLevel<5?"ĐÃ MỞ KHÓA  /  "+LevelName(currentLevel+1):"BẠN ĐÃ HOÀN THÀNH NĂM VÙNG ĐẤT"):
                "Hãy tận dụng vật phẩm và đường vòng để giữ khoảng cách với kẻ địch.",21,new Color(.74f,.81f,.8f),new Vector2(.5f,.24f),new Vector2(1000,60),Vector2.zero,TextAnchor.MiddleCenter);
            Button menu=LumiFactory.Button(board.transform,"CHỌN NHIỆM VỤ",new Color(.17f,.24f,.25f),ShowLevelMenu);
            LumiFactory.Rect(menu.GetComponent<RectTransform>(),new Vector2(.5f,.1f),new Vector2(440,70),new Vector2(-240,0));
            Button action=LumiFactory.Button(board.transform,won?(currentLevel<5?"NHIỆM VỤ TIẾP THEO  ›":"TRỞ VỀ CĂN CỨ"):"THỬ LẠI NHIỆM VỤ",won?new Color(.33f,.43f,.34f):new Color(.54f,.27f,.21f),
                ()=>{if(won && currentLevel==5)ShowLevelMenu();else BeginDeployment(won?currentLevel+1:currentLevel);});
            LumiFactory.Rect(action.GetComponent<RectTransform>(),new Vector2(.5f,.1f),new Vector2(440,70),new Vector2(240,0));
        }
    }
}
