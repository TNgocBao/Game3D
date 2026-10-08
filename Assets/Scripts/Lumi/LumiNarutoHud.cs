using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        public LumiVillageBoss VillageBoss{get;private set;}
        public bool BossCleared{get;private set;}
        private LumiAbilityHud abilityHud;
        private void SpawnVillageBoss(int level,LevelConfig config)
        {
            BossCleared=false;
            GameObject obj=new GameObject(LumiVillageBoss.Names[level-1],typeof(CharacterController));obj.transform.SetParent(worldRoot,false);
            obj.transform.position=config.Route[config.Route.Length-1]+Vector3.back*10;
            VillageBoss=obj.AddComponent<LumiVillageBoss>();VillageBoss.Initialize(this,level);
        }
        public void BossDefeated(LumiVillageBoss boss)
        {
            if(boss!=VillageBoss || BossCleared)return;BossCleared=true;score+=1000;
            ShowToast("ĐÃ HẠ "+boss.BossName.ToUpperInvariant()+" • CỔNG ĐÃ MỞ",new Color(.45f,1,.68f));Audio.Play("star");
        }
        private void BuildNarutoHud()
        {
            GameObject obj=new GameObject("Naruto skill controls",typeof(RectTransform));obj.transform.SetParent(hudPanel.transform,false);LumiFactory.Stretch(obj.GetComponent<RectTransform>());
            abilityHud=obj.AddComponent<LumiAbilityHud>();abilityHud.Build(this);
        }
        private void BindNarutoHud()
        {
            GameObject mobileRoot=mobileControlsView.Root;
            if(mobileRoot!=null)hudPanel.transform.SetSiblingIndex(mobileRoot.transform.GetSiblingIndex()+1);
            abilityHud.Bind(Player.GetComponent<LumiNarutoSkills>());
        }
    }
    public sealed class LumiAbilityHud:MonoBehaviour
    {
        [SerializeField] private LumiGame game;[SerializeField] private LumiNarutoSkills skills;
        [SerializeField] private Image[] cooldown=new Image[4];[SerializeField] private TMP_Text[] counters=new TMP_Text[4];
        [SerializeField] private TMP_Text[] controlHints=new TMP_Text[4];
        [SerializeField] private GameObject bossPanel;[SerializeField] private TMP_Text bossName;[SerializeField] private Image bossFill;
        private static Sprite circle;
        public static Sprite Circle
        {
            get
            {
                if(circle!=null)return circle;const int n=128;Texture2D texture=new Texture2D(n,n,TextureFormat.RGBA32,false);Color[] pixels=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++){float distance=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f));pixels[y*n+x]=new Color(1,1,1,Mathf.Clamp01(63-distance));}
                texture.SetPixels(pixels);texture.Apply(false,true);circle=Sprite.Create(texture,new Rect(0,0,n,n),Vector2.one*.5f);return circle;
            }
        }
        public void Bind(LumiNarutoSkills target){skills=target;}
        public void BeginHold(int ability){if(skills!=null)skills.BeginCharge(ability);}
        public void EndHold(int ability){if(skills!=null)skills.ReleaseCharge(ability);}
        public void Build(LumiGame owner)
        {
            game=owner;Vector2[] positions={new Vector2(-140,150),new Vector2(-365,115),new Vector2(-355,315),new Vector2(-145,415)};
            string[] names={"Phi tiêu","Rasengan","Rasenshuriken","Phân thân"};
            Color[] colors={new Color(.97f,.57f,.12f),new Color(.12f,.65f,1),new Color(.35f,.87f,1),new Color(.85f,.75f,.54f)};
            for(int i=0;i<4;i++)
            {
                int ability=i;float size=i==0?160:120;
                Image rim=LumiFactory.Image(transform,colors[i]);rim.sprite=Circle;LumiFactory.Rect(rim.rectTransform,new Vector2(1,0),Vector2.one*(size+9),positions[i]);rim.raycastTarget=false;
                Image face=LumiFactory.Image(rim.transform,new Color(.025f,.07f,.13f,.96f));face.sprite=Circle;LumiFactory.Rect(face.rectTransform,Vector2.one*.5f,Vector2.one*size,Vector2.zero);
                Button button=face.gameObject.AddComponent<Button>();button.targetGraphic=face;if(i==1 || i==2){var hold=face.gameObject.AddComponent<LumiJutsuHoldButton>();hold.Hud=this;hold.Ability=i;}
                else button.onClick.AddListener(()=>{if(skills!=null)skills.Cast(ability);});
                GameObject icon=new GameObject("Jutsu icon "+i,typeof(RectTransform),typeof(CanvasRenderer));icon.transform.SetParent(face.transform,false);
                LumiFactory.Rect(icon.GetComponent<RectTransform>(),Vector2.one*.5f,Vector2.one*(size*.62f),Vector2.zero);LumiJutsuIcon graphic=icon.AddComponent<LumiJutsuIcon>();graphic.Kind=i;graphic.color=colors[i];graphic.raycastTarget=false;
                cooldown[i]=LumiFactory.Image(face.transform,new Color(.01f,.02f,.04f,.83f));cooldown[i].sprite=Circle;cooldown[i].type=Image.Type.Filled;cooldown[i].fillMethod=Image.FillMethod.Radial360;cooldown[i].fillOrigin=2;LumiFactory.Stretch(cooldown[i].rectTransform);cooldown[i].raycastTarget=false;
                counters[i]=LumiFactory.Text(face.transform,"",40,TextAnchor.MiddleCenter,Color.white);LumiFactory.Stretch(counters[i].rectTransform);
                int labelSize=22;
                TMP_Text name=LumiFactory.Text(rim.transform,names[i],labelSize,TextAnchor.MiddleCenter,Color.white);name.fontSize=labelSize;name.enableWordWrapping=false;
                LumiFactory.Rect(name.rectTransform,new Vector2(.5f,0),new Vector2(220,44),new Vector2(0,-30));
                controlHints[i]=LumiFactory.Text(rim.transform,"",20,TextAnchor.MiddleCenter,new Color(.84f,.9f,1));
                LumiFactory.Rect(controlHints[i].rectTransform,new Vector2(.5f,1),new Vector2(120,38),new Vector2(0,20));
            }
            bossPanel=LumiFactory.Image(transform,new Color(.025f,.04f,.07f,.85f)).gameObject;
            LumiFactory.Rect(bossPanel.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(640,100),new Vector2(0,-195));
            bossName=LumiFactory.Text(bossPanel.transform,"",32,TextAnchor.MiddleCenter,Color.white);LumiFactory.Rect(bossName.rectTransform,new Vector2(.5f,.68f),new Vector2(610,48),Vector2.zero);
            Image track=LumiFactory.Image(bossPanel.transform,new Color(.18f,.08f,.08f));LumiFactory.Rect(track.rectTransform,new Vector2(.5f,.23f),new Vector2(580,13),Vector2.zero);track.raycastTarget=false;
            bossFill=LumiFactory.Image(track.transform,new Color(.95f,.23f,.16f));LumiFactory.Stretch(bossFill.rectTransform);bossFill.raycastTarget=false;bossFill.rectTransform.pivot=new Vector2(0,.5f);
            bossPanel.GetComponent<Image>().raycastTarget=false;bossPanel.SetActive(false);
            ApplyControlScheme();
        }
        public void ApplyControlScheme()
        {
            if(game==null)return;
            string[] hints=game.Controls.UsesMobileControls?new[]{"","GIỮ","GIỮ",""}:new[]{"CHUỘT","GIỮ Q","GIỮ R","F"};
            for(int i=0;i<controlHints.Length;i++)if(controlHints[i]!=null)
            {
                controlHints[i].text=hints[i];
                controlHints[i].fontSize=game.Controls.UsesMobileControls?20:24;
            }
        }
        private void LateUpdate()
        {
            if(skills==null || game==null || bossPanel==null || cooldown[0]==null)return;
            for(int i=0;i<4;i++)
            {
                float remaining=skills.Remaining(i);bool charging=skills.ChargingAbility==i;
                cooldown[i].color=charging?new Color(.1f,.65f,1,.45f):new Color(.01f,.02f,.04f,.83f);
                cooldown[i].fillAmount=charging?(skills.ChargeMultiplier-1)/2:remaining/skills.Cooldown(i);
                counters[i].text=charging?skills.ChargeMultiplier.ToString("0.0")+"×":remaining>.05f?Mathf.CeilToInt(remaining).ToString():"";
            }
            LumiVillageBoss boss=game.VillageBoss;bool visible=boss!=null && boss.Engaged;bossPanel.SetActive(visible);
            if(visible){bossName.text=boss.BossName+"  •  "+boss.Element;bossFill.rectTransform.anchorMax=new Vector2(boss.HealthFraction,1);}
        }
    }
    public sealed class LumiJutsuHoldButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {
        public LumiAbilityHud Hud;public int Ability;
        public void OnPointerDown(PointerEventData data){Hud.BeginHold(Ability);}
        public void OnPointerUp(PointerEventData data){Hud.EndHold(Ability);}
        private void OnDisable(){if(Hud!=null)Hud.EndHold(Ability);}
    }
    public sealed class LumiJutsuIcon:MaskableGraphic
    {
        public int Kind;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            if(Kind==1 || Kind==2 || Kind==0)
            {
                Disc(vh,Vector2.zero,r*.45f,color);Ring(vh,r*.7f,r*.57f,new Color(.8f,.97f,1));
                if(Kind==2 || Kind==0)for(int i=0;i<4;i++){float angle=i*Mathf.PI*.5f;Vector2 d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),side=new Vector2(-d.y,d.x);Triangle(vh,d*r,d*r*.15f+side*r*.27f,d*r*.15f-side*r*.27f,color);}
            }
            else if(Kind==3)for(int i=-1;i<=1;i++){Vector2 p=new Vector2(i*r*.55f,i==0?r*.2f:-r*.1f);Disc(vh,p,r*.23f,color);Quad(vh,p+Vector2.down*r*.45f,new Vector2(r*.42f,r*.38f),color);}
            else {Quad(vh,new Vector2(0,-r*.18f),new Vector2(r*1.05f,r*.65f),color);for(int i=0;i<4;i++)Quad(vh,new Vector2((i-1.5f)*r*.24f,r*.27f),new Vector2(r*.21f,r*.6f),color);}
        }
        private static void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color color){int n=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);}
        private static void Disc(VertexHelper vh,Vector2 center,float r,Color color){for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Triangle(vh,center,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,color);}}
        private static void Ring(VertexHelper vh,float outer,float inner,Color color){for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Vector2 x=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),y=new Vector2(Mathf.Cos(b),Mathf.Sin(b));Triangle(vh,x*outer,y*outer,x*inner,color);Triangle(vh,y*outer,y*inner,x*inner,color);}}
        private static void Quad(VertexHelper vh,Vector2 center,Vector2 size,Color color){Vector2 a=center-size*.5f,b=center+new Vector2(size.x,-size.y)*.5f,c=center+size*.5f,d=center+new Vector2(-size.x,size.y)*.5f;Triangle(vh,a,b,c,color);Triangle(vh,a,c,d,color);}
    }
}
