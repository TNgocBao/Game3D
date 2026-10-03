using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private static readonly string[] ReferenceNames={"VillageLeaf","VillageSand","VillageStone","VillageCloud","VillageMist"};
        private static readonly string[] VillageKages={"Hashirama Senju","Gaara","Ōnoki","Raikage Đệ Tứ","Mei Terumī"};
        private static readonly string[] VillageElements={"Mộc Độn","Cát","Trần Độn","Lôi Độn","Dung Độn"};
        private static readonly string[] VillageDescriptions={"Rừng xanh, mái ngói đỏ và vách núi Hokage.","Ốc đảo giữa những hẻm núi sa thạch.","Thành đá trải dài trên núi cao.","Những cây cầu phía trên biển mây.","Kênh nước và phố nhỏ trong sương sớm."};
        private static readonly Color UiBackground=new Color(.025f,.037f,.065f);
        private static readonly Color UiSurface=new Color(.06f,.078f,.115f);
        private static readonly Color UiMuted=new Color(.65f,.71f,.8f);
        private static readonly Color UiOrange=new Color(1f,.49f,.19f);
        private static Sprite roundedPanelSprite;
        private GameObject loadingPanel;
        private bool deploying;
        private int selectedMission=1;

        private static Sprite RoundedPanelSprite()
        {
            if(roundedPanelSprite!=null)return roundedPanelSprite;
            const int size=64;const float radius=18;
            Texture2D texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            texture.name="Naruto UI rounded surface";texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;
            Color[] pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(Mathf.Abs(x+.5f-size*.5f)-(size*.5f-radius),0);
                float dy=Mathf.Max(Mathf.Abs(y+.5f-size*.5f)-(size*.5f-radius),0);
                pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)+.5f));
            }
            texture.SetPixels(pixels);texture.Apply();
            roundedPanelSprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));
            return roundedPanelSprite;
        }

        private Image UiSurfacePanel(Transform parent,string name,Vector2 size,Vector2 position,Color color,bool clip=false)
        {
            Image panel=LumiFactory.Image(parent,color);panel.name=name;panel.sprite=RoundedPanelSprite();panel.type=Image.Type.Sliced;
            LumiFactory.Rect(panel.rectTransform,new Vector2(.5f,.5f),size,position);panel.raycastTarget=false;
            if(clip){panel.gameObject.AddComponent<Mask>().showMaskGraphic=true;}
            return panel;
        }

        private Button UiAction(Transform parent,string label,Vector2 size,Vector2 position,Color color,UnityEngine.Events.UnityAction action)
        {
            Button button=LumiFactory.Button(parent,label,color,action);
            Image image=button.GetComponent<Image>();image.sprite=RoundedPanelSprite();image.type=Image.Type.Sliced;
            LumiFactory.Rect(image.rectTransform,new Vector2(.5f,.5f),size,position);
            ColorBlock palette=button.colors;palette.normalColor=Color.white;palette.highlightedColor=new Color(1.15f,1.15f,1.15f);palette.pressedColor=new Color(.8f,.8f,.8f);palette.disabledColor=new Color(.55f,.55f,.55f,.7f);button.colors=palette;
            return button;
        }

        private RawImage ReferenceImage(Transform parent,string key,Vector2 anchor,Vector2 size,Vector2 offset)
        {
            var obj=new GameObject("Artwork - "+key,typeof(RectTransform),typeof(CanvasRenderer),typeof(LumiCoverImage));
            obj.transform.SetParent(parent,false);RawImage img=obj.GetComponent<RawImage>();
            img.texture=Resources.Load<Texture2D>("LumiReference/"+key) ?? Resources.Load<Texture2D>("LumiReference/NarutoMenu");
            img.raycastTarget=false;LumiFactory.Rect(img.rectTransform,anchor,size,offset);((LumiCoverImage)img).UpdateCrop();return img;
        }

        private void UiLabel(Transform parent,string value,int size,Color color,Vector2 anchor,Vector2 bounds,Vector2 offset,TextAnchor alignment=TextAnchor.MiddleLeft)
        {
            Text label=LumiFactory.Text(parent,value,size,alignment,color);label.raycastTarget=false;label.fontSize=Mathf.Max(24,size);
            LumiFactory.Rect(label.rectTransform,anchor,bounds,offset);
        }
        private void UiText(Transform parent,string text,int size,Color color,Vector2 bounds,Vector2 position,TextAnchor alignment=TextAnchor.MiddleLeft)
        {UiLabel(parent,text,size,color,new Vector2(.5f,.5f),bounds,position,alignment);}

        private void BuildReferenceMenu()
        {
            LumiFactory.DestroyChildren(menuPanel.transform);menuPanel.GetComponent<Image>().color=UiBackground;
            int unlocked=Mathf.Clamp(PlayerPrefs.GetInt("Lumi.Unlocked",1),1,5);selectedMission=Mathf.Clamp(selectedMission,1,5);
            var root=new GameObject("Modern campaign layout",typeof(RectTransform));root.transform.SetParent(menuPanel.transform,false);
            LumiFactory.Rect(root.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(1920,1080),Vector2.zero);
            Transform content=root.transform;
            Image side=UiSurfacePanel(content,"Navigation",new Vector2(330,984),new Vector2(-735,0),UiSurface);
            UiText(side.transform,"NARUTO",58,Color.white,new Vector2(270,90),new Vector2(0,382));
            UiText(side.transform,"CHIBI ADVENTURE",24,UiOrange,new Vector2(270,55),new Vector2(0,315));
            UiSurfacePanel(side.transform,"Divider",new Vector2(270,2),new Vector2(0,260),new Color(.15f,.18f,.25f));
            UiSurfacePanel(side.transform,"Active navigation",new Vector2(286,76),new Vector2(0,185),new Color(.18f,.115f,.09f));
            UiText(side.transform,"Hành trình",34,UiOrange,new Vector2(246,55),new Vector2(0,185));
            UiText(side.transform,"5 làng ninja",32,Color.white,new Vector2(270,55),new Vector2(0,79));
            UiText(side.transform,"Đã mở "+unlocked+" / 5 làng",28,UiMuted,new Vector2(270,55),new Vector2(0,22));
            UiText(side.transform,"ĐIỀU KHIỂN",28,UiMuted,new Vector2(270,55),new Vector2(0,-104));
            string[] keys={"WASD","CHUỘT","Q / R / F","TAB / ESC"};
            string[] actions={"Di chuyển","Xoay / Ném","Nhẫn thuật","UI / Menu"};
            for(int i=0;i<4;i++)
            {
                float y=-168-i*62;
                UiText(side.transform,keys[i],24,Color.white,new Vector2(122,50),new Vector2(-70,y));
                UiText(side.transform,actions[i],24,UiMuted,new Vector2(160,50),new Vector2(68,y));
            }
            UiAction(side.transform,"CÀI ĐẶT",new Vector2(270,68),new Vector2(0,-419),new Color(.12f,.155f,.22f),OpenAudioSettings);
            UiText(content,"Hành trình ninja",48,Color.white,new Vector2(1100,85),new Vector2(170,427));
            UiText(content,"Chọn làng và sẵn sàng thi triển nhẫn thuật",30,UiMuted,new Vector2(1300,60),new Vector2(270,360));
            Image hero=UiSurfacePanel(content,"Selected village",new Vector2(1350,530),new Vector2(225,38),Color.white,true);
            ReferenceImage(hero.transform,ReferenceNames[selectedMission-1],new Vector2(.5f,.5f),new Vector2(1350,530),Vector2.zero);
            Image info=LumiFactory.Image(hero.transform,new Color(.02f,.032f,.058f,.9f));info.raycastTarget=false;
            LumiFactory.Rect(info.rectTransform,new Vector2(.5f,0),new Vector2(1350,164),new Vector2(0,82));
            UiText(info.transform,LevelName(selectedMission),46,Color.white,new Vector2(800,76),new Vector2(-235,34));
            UiText(info.transform,VillageKages[selectedMission-1]+"  ·  "+VillageElements[selectedMission-1],30,UiMuted,new Vector2(790,60),new Vector2(-240,-35));
            bool playable=selectedMission<=unlocked;
            Button deploy=UiAction(info.transform,playable?"XUẤT PHÁT  ›":"CHƯA MỞ KHÓA",new Vector2(360,76),new Vector2(451,0),UiOrange,()=>BeginDeployment(selectedMission));deploy.interactable=playable;
            UiText(content,"NGŨ ĐẠI NHẪN THÔN",28,UiMuted,new Vector2(1350,52),new Vector2(225,-274));
            for(int i=1;i<=5;i++)
            {
                int mission=i;bool available=i<=unlocked;bool selected=i==selectedMission;
                Image card=UiSurfacePanel(content,"Village choice "+i,new Vector2(254,180),new Vector2(225+(i-3)*274,-396),selected?UiOrange:UiSurface,true);
                ReferenceImage(card.transform,ReferenceNames[i-1],new Vector2(.5f,1),new Vector2(250,120),new Vector2(0,-62));
                Image footer=LumiFactory.Image(card.transform,selected?new Color(.34f,.16f,.07f):UiSurface);footer.raycastTarget=false;
                LumiFactory.Rect(footer.rectTransform,new Vector2(.5f,0),new Vector2(254,60),new Vector2(0,30));
                UiText(footer.transform,LevelName(i).Replace("Làng ",""),30,Color.white,new Vector2(220,56),Vector2.zero);
                if(!available){Image lockTag=UiSurfacePanel(card.transform,"Locked village",new Vector2(104,42),new Vector2(62,60),new Color(.02f,.03f,.05f,.88f));UiText(lockTag.transform,"KHÓA",24,UiMuted,new Vector2(100,46),Vector2.zero,TextAnchor.MiddleCenter);}
                Button button=card.gameObject.AddComponent<Button>();card.raycastTarget=true;button.targetGraphic=card;
                button.onClick.AddListener(()=>{selectedMission=mission;Audio.Play("ui");BuildReferenceMenu();});card.gameObject.AddComponent<LumiUiMotion>();
            }
            UiText(content,"Thu thập 5 sao  ·  Hạ Kage  ·  Mở cổng tới làng tiếp theo",26,UiMuted,new Vector2(1350,55),new Vector2(225,-510));
        }

        private void BeginDeployment(int level)
        {
            if(deploying || level>PlayerPrefs.GetInt("Lumi.Unlocked",1))return;StartCoroutine(DeploymentRoutine(level));
        }
        private IEnumerator DeploymentRoutine(int level)
        {
            deploying=true;state=GameState.Loading;loadingPanel=Panel("Village loading",UiBackground);loadingPanel.GetComponent<Image>().raycastTarget=true;
            RawImage scene=ReferenceImage(loadingPanel.transform,ReferenceNames[level-1],new Vector2(.5f,.5f),new Vector2(1920,1080),Vector2.zero);LumiFactory.Stretch(scene.rectTransform);
            Image veil=LumiFactory.Image(loadingPanel.transform,new Color(.015f,.025f,.045f,.18f));LumiFactory.Stretch(veil.rectTransform);veil.raycastTarget=false;
            Image info=UiSurfacePanel(loadingPanel.transform,"Loading information",new Vector2(1740,230),new Vector2(0,-359),new Color(.025f,.04f,.07f,.96f));
            UiText(info.transform,"Đang tới "+LevelName(level),48,Color.white,new Vector2(1500,80),new Vector2(-50,56));
            string[] tips={"Mộc Độn: rời vùng cảnh báo trước khi rễ cây trồi lên.","Cát: dùng phân thân để tạo khoảng trống tiếp cận Gaara.","Trần Độn: tránh khối lập phương trước khi chiêu kích hoạt.","Lôi Độn: di chuyển ngang để tránh Raikage đột kích.","Dung Độn: tránh đứng trong vùng dung nham của Mei."};
            UiText(info.transform,tips[Mathf.Clamp(level-1,0,4)],30,UiMuted,new Vector2(1500,60),new Vector2(-50,-12));
            Image track=UiSurfacePanel(info.transform,"Loading track",new Vector2(1580,8),new Vector2(0,-77),new Color(.15f,.18f,.25f));
            Image fill=LumiFactory.Image(track.transform,UiOrange);fill.raycastTarget=false;fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchorMin=fill.rectTransform.anchorMax=new Vector2(0,.5f);fill.rectTransform.sizeDelta=new Vector2(158,8);
            yield return null;fill.rectTransform.sizeDelta=new Vector2(553,8);yield return null;
            StartLevel(level);state=GameState.Loading;loadingPanel.transform.SetAsLastSibling();fill.rectTransform.sizeDelta=new Vector2(1580,8);yield return null;
            CanvasGroup fade=loadingPanel.AddComponent<CanvasGroup>();float elapsed=0;
            while(elapsed<.35f){elapsed+=Time.unscaledDeltaTime;fade.alpha=1-Mathf.Clamp01(elapsed/.35f);yield return null;}
            Destroy(loadingPanel);loadingPanel=null;deploying=false;state=GameState.Playing;
        }

        private void BuildMissionReport(bool won,int stars)
        {
            LumiFactory.DestroyChildren(resultPanel.transform);resultPanel.GetComponent<Image>().color=new Color(.01f,.02f,.04f,.8f);resultPanel.SetActive(true);
            Image board=UiSurfacePanel(resultPanel.transform,"Mission report",new Vector2(1300,800),Vector2.zero,UiSurface,true);
            Image portrait=UiSurfacePanel(board.transform,"Village artwork",new Vector2(450,800),new Vector2(-425,0),Color.white,true);
            ReferenceImage(portrait.transform,ReferenceNames[currentLevel-1],new Vector2(.5f,.5f),new Vector2(450,800),Vector2.zero);
            Image shade=LumiFactory.Image(portrait.transform,new Color(.02f,.03f,.05f,.36f));LumiFactory.Stretch(shade.rectTransform);shade.raycastTarget=false;
            UiText(portrait.transform,LevelName(currentLevel),44,Color.white,new Vector2(370,120),new Vector2(0,-220));
            UiText(portrait.transform,VillageKages[currentLevel-1],30,Color.white,new Vector2(370,65),new Vector2(0,-300));
            Color accent=won?UiOrange:new Color(1f,.38f,.4f);
            UiText(board.transform,won?"Hoàn thành!":"Thử lại nhé",52,Color.white,new Vector2(690,85),new Vector2(210,286));
            UiText(board.transform,won?"Bạn đã vượt qua thử thách của Kage.":"Nghỉ một chút và chuẩn bị chiến thuật mới.",30,UiMuted,new Vector2(690,95),new Vector2(210,189));
            UiText(board.transform,won?Stars(stars):"NHIỆM VỤ CHƯA HOÀN THÀNH",won?64:30,accent,new Vector2(690,100),new Vector2(210,80));
            for(int i=0;i<2;i++)
            {
                Image stat=UiSurfacePanel(board.transform,"Statistic",new Vector2(324,126),new Vector2(32+i*352,-60),UiBackground);
                UiText(stat.transform,i==0?score.ToString():collectedStars+" / "+totalStars,40,Color.white,new Vector2(276,65),new Vector2(0,24));
                UiText(stat.transform,i==0?"ĐIỂM":"SAO THU THẬP",28,UiMuted,new Vector2(276,50),new Vector2(0,-34));
            }
            UiText(board.transform,won?(currentLevel<5?"Đã mở khóa "+LevelName(currentLevel+1):"Đã hoàn thành hành trình qua 5 làng!"):"Dùng Rasengan, Rasenshuriken và phân thân đúng lúc.",30,UiMuted,new Vector2(690,90),new Vector2(210,-190));
            UiAction(board.transform,"CHỌN LÀNG",new Vector2(290,76),new Vector2(3,-308),new Color(.14f,.18f,.25f),ShowLevelMenu);
            UiAction(board.transform,won?(currentLevel<5?"LÀNG TIẾP THEO  ›":"VỀ MENU"):"THỬ LẠI",new Vector2(370,76),new Vector2(365,-308),accent,()=>{if(won && currentLevel==5)ShowLevelMenu();else BeginDeployment(won?currentLevel+1:currentLevel);});
        }
    }
}
