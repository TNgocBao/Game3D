using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    public partial class LumiGame : MonoBehaviour
    {
        private enum GameState { Menu, Loading, Playing, Paused, Won, Lost }

        private GameState state;
        private Transform worldRoot;
        private Canvas canvas;
        private GameObject menuPanel;
        private GameObject hudPanel;
        private GameObject resultPanel;
        private GameObject pausePanel;
        private GameObject dialoguePanel;
        private Text healthText;
        private Text armorText;
        private Text starText;
        private Text objectiveText;
        private Text toastText;
        private Text promptText;
        private Text dialogueText;
        private Slider healthSlider;
        private Slider armorSlider;
        private LumiDirectionArrow directionArrow;
        private LumiMenuBackdrop menuBackdrop;
        private Transform goal;
        private int currentLevel;
        private int collectedStars;
        private int totalStars;
        private readonly int[] kills = new int[3];
        private Coroutine toastRoutine;
        private Coroutine dialogueRoutine;
        private bool pointerWasLocked;
        private int score;
        private LumiTransientPool transientPool;
        private LumiMobileControlsView mobileControlsView;

        public LumiAudio Audio { get; private set; }
        public LumiControlScheme Controls {get;private set;}
        public LumiLevelTimer LevelTimer {get;private set;}
        public LumiNpcBuffService NpcBuffs {get;private set;}
        public LumiBuffStatusView BuffStatusView {get;private set;}
        public LumiPlayer Player { get; private set; }
        public LumiCameraRig CameraRig { get; private set; }
        public int CurrentLevel=>currentLevel;
        public bool IsPlaying => state == GameState.Playing;

        private void Awake()
        {
            DisableOriginalScene();
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadowDistance = 45f;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowCascades = 2;
            Controls=gameObject.AddComponent<LumiControlScheme>();Controls.Initialize();
            LevelTimer=gameObject.AddComponent<LumiLevelTimer>();LevelTimer.Initialize(this);
            NpcBuffs=gameObject.AddComponent<LumiNpcBuffService>();NpcBuffs.Initialize(this);
            gameObject.AddComponent<LumiPauseInputRouter>().Initialize(this);
            Audio = gameObject.AddComponent<LumiAudio>();
            Audio.Initialize();
            BuildInterface();
            ShowLevelMenu();
        }

        internal void HandleEscapeCommand()
        {
            if(settingsPanel!=null && settingsPanel.activeSelf){CloseGameSettings();return;}
            if(state==GameState.Playing)
            {
                if(dialoguePanel.activeSelf)CloseDialogue();
                else PauseGame();
            }
            else if(state==GameState.Paused)ResumeGame();
            else if(state==GameState.Won||state==GameState.Lost)ShowLevelMenu();
        }

        private void DisableOriginalScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == gameObject) continue;
                root.SetActive(false);
                Destroy(root);
            }
        }

        public void ShowLevelMenu()
        {
            CloseDialogue();
            if(toastRoutine!=null){StopCoroutine(toastRoutine);toastRoutine=null;}
            if(toastText!=null)toastText.gameObject.SetActive(false);
            Time.timeScale = 1f;
            state = GameState.Menu;
            SetCursor(false);
            ClearWorld();
            if (menuBackdrop != null) menuBackdrop.SetVisible(true);
            Audio.PlayMenuMusic();
            HideAllPanels();
            BuildReferenceMenu();
            menuPanel.SetActive(true);
        }

        public void StartLevel(int level)
        {
            if(!LumiProgressionRules.CanStartLevel(PlayerPrefs.GetInt("Lumi.Unlocked",1),level)){ShowToast("Hoàn thành màn trước để mở màn này!",new Color(1,.7f,.2f));return;}
            StartLevelInternal(level);
        }
#if UNITY_EDITOR
        public void StartLevelForValidation(int level){StartLevelInternal(level);}
#endif
        private void StartLevelInternal(int level)
        {
            CloseDialogue();
            Time.timeScale = 1f;
            currentLevel = Mathf.Clamp(level, 1, 5);
            selectedMission = currentLevel;
            collectedStars = 0;
            score = 0;
            LevelTimer.BeginLevel();
            NpcBuffs.BeginLevel();
            totalStars = 5;
            for (int i = 0; i < kills.Length; i++) kills[i] = 0;
            LumiMobileInput.Reset();

            ClearWorld();
            if (menuBackdrop != null) menuBackdrop.SetVisible(false);
            HideAllPanels();
            hudPanel.SetActive(true);
            mobileControlsView.SetVisible(Controls.UsesMobileControls);
            BuildLevel(currentLevel);
            state = GameState.Playing;
            BindNarutoHud();
            SetCursor(CameraRig==null || !CameraRig.MovementFacing);
            Audio.PlayLevelMusic(currentLevel);
            RefreshHud();
            ShowToast("VÙNG ĐẤT " + currentLevel + " — " + LevelName(currentLevel), LevelAccent(currentLevel));
        }

        public void PauseGame()
        {
            if (state != GameState.Playing) return;
            state = GameState.Paused;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            mobileControlsView.SetVisible(false);
            SetCursor(false);
        }

        public void ResumeGame()
        {
            if (state != GameState.Paused) return;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            state = GameState.Playing;
            mobileControlsView.SetVisible(Controls.UsesMobileControls);
            SetCursor(CameraRig==null || !CameraRig.MovementFacing);
        }

        public void LoseLevel()
        {
            if (state != GameState.Playing) return;
            state = GameState.Lost;
            SetCursor(false);
            Audio.Play("lose");
            Audio.StopMusic();
            SetDirectionArrow(false);
            BuildResult(false, 0);
        }

        public void WinLevel()
        {
            if (state != GameState.Playing) return;
            if(!BossCleared){ShowToast("Hạ "+LumiVillageBoss.Names[currentLevel-1]+" để mở cổng!",new Color(1,.7f,.2f));return;}
            state = GameState.Won;
            SetCursor(false);
            Audio.Play("win");
            Audio.StopMusic();
            SetDirectionArrow(false);

            int stars = LumiProgressionRules.Rating(true,collectedStars,totalStars);
            string key = "Lumi.Level." + currentLevel + ".Stars";
            PlayerPrefs.SetInt(key, Mathf.Max(stars, PlayerPrefs.GetInt(key, 0)));
            int unlocked = PlayerPrefs.GetInt("Lumi.Unlocked", 1);
            PlayerPrefs.SetInt("Lumi.Unlocked",LumiProgressionRules.UnlockedAfterResult(unlocked,currentLevel,true));
            PlayerPrefs.Save();

            BuildResult(true, stars);
            StartCoroutine(Fireworks());
        }

        public void CollectStar()
        {
            score += 100;
            collectedStars = Mathf.Min(totalStars, collectedStars + 1);
            ShowToast("NHẶT ĐƯỢC SAO  " + collectedStars + "/" + totalStars, new Color(1f, 0.8f, 0.05f));
            RefreshHud();
        }

        public void RegisterKill(LumiEnemyType type)
        {
            score += (int)type * 50;
            int index = Mathf.Clamp((int)type - 1, 0, 2);
            kills[index]++;
            RefreshHud();
        }

        public void SpawnProjectile(Vector3 position, Vector3 direction, int damage, bool fromPlayer,Transform shooter)
        {
            Material material = LumiFactory.Material(fromPlayer ? "PlayerBolt" : "EnemyBolt",
                fromPlayer ? new Color(0.15f, 0.95f, 1f) : new Color(1f, 0.18f, 0.08f), true);
            LumiProjectile projectile = transientPool.RentProjectile(material, fromPlayer ? .22f : .28f);
            projectile.transform.position = position;
            projectile.Initialize(this, direction, fromPlayer ? 24f : 13f, damage, fromPlayer,shooter);
        }

        public void SpawnImpact(Vector3 position, Color color)
        {
            if(transientPool!=null)transientPool.Impact(position,color);
        }

        public bool TryDropEnemySupportItem(Vector3 position)
        {
            if(worldRoot==null||Random.value>=.5f)return false;
            LumiPickupType type=Random.value<.5f?LumiPickupType.Health:LumiPickupType.Armor;
            GameObject item=new GameObject("Quái rơi "+(type==LumiPickupType.Health?"Hồi máu":"Giáp"));
            item.transform.SetParent(worldRoot,false);
            item.transform.position=position+Vector3.up*.9f;
            item.AddComponent<LumiPickup>().Initialize(this,type);
            return true;
        }

        public void RefreshHud()
        {
            if (Player == null || healthText == null) return;
            healthText.text = "MÁU  " + Player.Health + "/" + LumiPlayer.MaxHealth;
            armorText.text = "GIÁP  " + Player.Armor + "/" + LumiPlayer.MaxArmor;
            starText.text = "★ " + collectedStars + "/" + totalStars + "   ·   " + score;
            healthSlider.value = Player.Health / (float)LumiPlayer.MaxHealth;
            armorSlider.value = Player.Armor / (float)LumiPlayer.MaxArmor;
        }

        public void SetDirectionArrow(bool visible)
        {
            if (directionArrow != null && directionArrow.gameObject.activeSelf != visible)
                directionArrow.gameObject.SetActive(visible);
        }

        public void ShowToast(string message, Color color)
        {
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine(message, color));
        }

        private IEnumerator ToastRoutine(string message, Color color)
        {
            toastText.text = message;
            toastText.color = color;
            toastText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2.2f);
            toastText.gameObject.SetActive(false);
        }

        public void ShowDialogue(string speaker, string message)
        {
            if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
            dialogueRoutine = StartCoroutine(DialogueRoutine(speaker, message));
        }

        private IEnumerator DialogueRoutine(string speaker, string message)
        {
            dialogueText.text = "<b>" + speaker + "</b>\n" + message;
            dialoguePanel.SetActive(true);
            yield return new WaitForSecondsRealtime(5.5f);
            dialoguePanel.SetActive(false);
            dialogueRoutine=null;
        }

        public void CloseDialogue()
        {
            if(dialogueRoutine!=null){StopCoroutine(dialogueRoutine);dialogueRoutine=null;}
            if(dialoguePanel!=null)dialoguePanel.SetActive(false);
        }

        public void ShowInteractionPrompt(string message)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
            mobileControlsView.SetInteractionVisible(true);
        }

        public void HideInteractionPrompt()
        {
            promptText.gameObject.SetActive(false);
            mobileControlsView.SetInteractionVisible(false);
        }

        public bool IsPointerOverUi()
        {
            if(EventSystem.current==null)return false;
            if(Controls.UsesMobileControls)
            {
                for(int i=0;i<Input.touchCount;i++)
                    if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))return true;
                return EventSystem.current.IsPointerOverGameObject();
            }
            if(Cursor.lockState==CursorLockMode.Locked)return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void BuildInterface()
        {
            var backdropObject = new GameObject("Animated Menu Backdrop");
            backdropObject.transform.SetParent(transform, false);
            menuBackdrop = backdropObject.AddComponent<LumiMenuBackdrop>();
            menuBackdrop.Initialize();

            var canvasObject = new GameObject("Lumi UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.matchWidthOrHeight = 0.5f;

            var eventObject = new GameObject("Lumi EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventObject.transform.SetParent(transform, false);

            menuPanel = Panel("Level Menu", new Color(0.015f, 0.035f, 0.07f, 0.56f));
            BuildHud();
            BuildPausePanel();
            BuildDialoguePanel();
            mobileControlsView=gameObject.AddComponent<LumiMobileControlsView>();
            mobileControlsView.Initialize(this,canvas.transform);
            BuildNarutoHud();
            resultPanel = Panel("Result", new Color(0.02f, 0.035f, 0.065f, 0.94f));
            BuildGameSettings();

            toastText = LumiFactory.Text(canvas.transform, string.Empty, 34, TextAnchor.MiddleCenter, Color.white);
            LumiFactory.Rect(toastText.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(900f, 70f), Vector2.zero);
            toastText.raycastTarget = false;
            toastText.gameObject.SetActive(false);

            promptText = LumiFactory.Text(canvas.transform, string.Empty, 26, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.35f));
            LumiFactory.Rect(promptText.rectTransform, new Vector2(0.5f, 0.22f), new Vector2(850f, 60f), Vector2.zero);
            promptText.raycastTarget = false;
            promptText.gameObject.SetActive(false);
        }

        private GameObject Panel(string name, Color color)
        {
            Image image = LumiFactory.Image(canvas.transform, color);
            image.gameObject.name = name;
            LumiFactory.Stretch(image.rectTransform);
            image.raycastTarget = false;
            return image.gameObject;
        }

        private static string LevelDescription(int level)
        {
            switch (level)
            {
                case 1: return "LÀM QUEN • SPROUT";
                case 2: return "MÊ CUNG • RANGER";
                case 3: return "ĐÔNG ĐẢO • SĂN ĐUỔI";
                case 4: return "DUNG NHAM • GOLEM";
                default: return "THỬ THÁCH CUỐI";
            }
        }

        private void BuildHud()
        {
            hudPanel = new GameObject("HUD", typeof(RectTransform));
            hudPanel.transform.SetParent(canvas.transform, false);
            LumiFactory.Stretch(hudPanel.GetComponent<RectTransform>());

            Image status = LumiFactory.Image(hudPanel.transform, new Color(0.02f, 0.04f, 0.08f, 0.78f));
            LumiFactory.Rect(status.rectTransform, new Vector2(0f, 1f), new Vector2(430f, 195f), new Vector2(230f, -115f));
            status.sprite=RoundedPanelSprite();status.type=Image.Type.Sliced;
            status.raycastTarget = false;
            Image healthIcon=LumiFactory.Image(status.transform,Color.white);healthIcon.sprite=LumiIconLibrary.Get(LumiIconKind.Health);
            LumiFactory.Rect(healthIcon.rectTransform,new Vector2(0,.8f),new Vector2(40,40),new Vector2(34,0));healthIcon.raycastTarget=false;
            healthText = LumiFactory.Text(status.transform, "MÁU", 23, TextAnchor.MiddleLeft, Color.white);
            LumiFactory.Rect(healthText.rectTransform, new Vector2(0.5f, 0.8f), new Vector2(330f, 38f), new Vector2(24,0));
            healthText.raycastTarget = false;
            healthSlider = CreateBar(status.transform, new Vector2(0f, 35f), new Color(0.95f, 0.15f, 0.2f));
            Image armorIcon=LumiFactory.Image(status.transform,Color.white);armorIcon.sprite=LumiIconLibrary.Get(LumiIconKind.Armor);
            LumiFactory.Rect(armorIcon.rectTransform,new Vector2(0,.47f),new Vector2(40,40),new Vector2(34,0));armorIcon.raycastTarget=false;
            armorText = LumiFactory.Text(status.transform, "GIÁP", 23, TextAnchor.MiddleLeft, Color.white);
            LumiFactory.Rect(armorText.rectTransform, new Vector2(0.5f, 0.47f), new Vector2(330f, 38f), new Vector2(24,0));
            armorText.raycastTarget = false;
            armorSlider = CreateBar(status.transform, new Vector2(0f, -30f), new Color(0.3f, 0.78f, 1f));

            starText = LumiFactory.Text(hudPanel.transform, "★ 0/5", 34, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.08f));
            LumiFactory.Rect(starText.rectTransform, new Vector2(0.5f, 0.94f), new Vector2(400f, 64f), Vector2.zero);
            starText.raycastTarget = false;
            objectiveText = LumiFactory.Text(hudPanel.transform, "HẠ KAGE CUỐI LÀNG ĐỂ MỞ CỔNG", 22, TextAnchor.MiddleCenter, Color.white);
            LumiFactory.Rect(objectiveText.rectTransform, new Vector2(0.5f, 0.885f), new Vector2(700f, 45f), Vector2.zero);
            objectiveText.raycastTarget = false;
            Text timerText=LumiFactory.Text(hudPanel.transform,"THỜI GIAN  15:00",24,TextAnchor.MiddleCenter,new Color(1f,.9f,.45f));
            LumiFactory.Rect(timerText.rectTransform,new Vector2(.5f,.835f),new Vector2(420f,48f),Vector2.zero);timerText.raycastTarget=false;
            LevelTimer.BindDisplay(timerText);
            BuffStatusView=gameObject.AddComponent<LumiBuffStatusView>();
            BuffStatusView.Initialize(this,hudPanel.transform);
            Button settings=LumiFactory.Button(hudPanel.transform,"SETTING",new Color(.12f,.22f,.28f,.95f),OpenGameSettings);
            settings.gameObject.name="Setting";
            settings.image.sprite=LumiAbilityHud.Circle;
            settings.image.type=Image.Type.Simple;
            Text settingsLabel=settings.GetComponentInChildren<Text>();
            settingsLabel.fontSize=16;settingsLabel.alignment=TMPro.TextAlignmentOptions.Center;
            LumiFactory.Rect(settings.GetComponent<RectTransform>(),new Vector2(1,1),new Vector2(112,112),new Vector2(-75,-90));

            Text crosshair = LumiFactory.Text(hudPanel.transform, "+", 36, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.85f));
            LumiFactory.Rect(crosshair.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), Vector2.zero);
            crosshair.raycastTarget = false;
            crosshair.gameObject.AddComponent<LumiCursorReticle>();
            hudPanel.SetActive(false);
        }

        private Slider CreateBar(Transform parent, Vector2 position, Color fillColor)
        {
            Image background = LumiFactory.Image(parent, new Color(0.1f, 0.14f, 0.18f, 0.95f));
            background.sprite=RoundedPanelSprite();background.type=Image.Type.Sliced;
            LumiFactory.Rect(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(370f, 22f), position);
            background.raycastTarget = false;
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(background.transform, false);
            LumiFactory.Stretch(fillArea.GetComponent<RectTransform>(), 2f);
            Image fill = LumiFactory.Image(fillArea.transform, fillColor);
            fill.sprite=RoundedPanelSprite();fill.type=Image.Type.Sliced;LumiFactory.Stretch(fill.rectTransform);
            fill.raycastTarget = false;
            Slider slider = background.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = null;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;
            return slider;
        }

        private void BuildPausePanel()
        {
            pausePanel = Panel("Pause", new Color(0.01f, 0.02f, 0.04f, 0.8f));
            Image board=UiSurfacePanel(pausePanel.transform,"Pause menu",new Vector2(620,590),Vector2.zero,UiSurface);
            UiText(board.transform,"Tạm dừng",52,Color.white,new Vector2(500,90),new Vector2(0,185),TextAnchor.MiddleCenter);
            UiText(board.transform,"Sẵn sàng tiếp tục hành trình?",28,UiMuted,new Vector2(520,65),new Vector2(0,97),TextAnchor.MiddleCenter);
            UiAction(board.transform,"TIẾP TỤC",new Vector2(500,76),new Vector2(0,0),UiOrange,ResumeGame);
            UiAction(board.transform,"CHỌN LÀNG",new Vector2(500,76),new Vector2(0,-97),new Color(.12f,.155f,.22f),ShowLevelMenu);
            UiAction(board.transform,"SETTING",new Vector2(500,76),new Vector2(0,-194),new Color(.12f,.155f,.22f),OpenGameSettings);
            pausePanel.SetActive(false);
        }

        private void ChangeVolume(float change)
        {
            Audio.MusicVolume = Mathf.Clamp01(Audio.MusicVolume + change);
            Audio.EffectsVolume = Mathf.Clamp01(Audio.EffectsVolume + change);
            PlayerPrefs.SetFloat("Lumi.Music", Audio.MusicVolume);
            PlayerPrefs.SetFloat("Lumi.Sfx", Audio.EffectsVolume);
            PlayerPrefs.Save();
        }

        private void BuildDialoguePanel()
        {
            dialoguePanel = LumiFactory.Image(canvas.transform, new Color(0.025f, 0.05f, 0.09f, 0.94f)).gameObject;
            LumiFactory.Rect(dialoguePanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.13f), new Vector2(1100f, 170f), Vector2.zero);
            dialogueText = LumiFactory.Text(dialoguePanel.transform, string.Empty, 28, TextAnchor.MiddleLeft, Color.white);
            LumiFactory.Stretch(dialogueText.rectTransform, 35f);
            dialogueText.richText = true;
            dialogueText.raycastTarget = false;
            dialogueText.rectTransform.offsetMax=new Vector2(-80f,-35f);
            Button close=LumiFactory.Button(dialoguePanel.transform,"×",new Color(.2f,.28f,.3f),CloseDialogue);
            LumiFactory.Rect(close.GetComponent<RectTransform>(),new Vector2(1,1),new Vector2(44,44),new Vector2(-32,-32));
            dialoguePanel.SetActive(false);
        }

        private void BuildResult(bool won, int stars)
        {
            BuildMissionReport(won,stars);
        }

        private IEnumerator Fireworks()
        {
            for (int burst = 0; burst < 10; burst++)
            {
                if (CameraRig == null || goal == null || state != GameState.Won) yield break;
                Vector3 position = goal.position + new Vector3(Random.Range(-12f,12f),Random.Range(3f,7f),Random.Range(-4f,4f));
                Color color = Color.HSVToRGB(Random.value, 0.75f, 1f);
                SpawnFirework(position, color);
                yield return new WaitForSecondsRealtime(0.28f);
            }
        }

        private void SpawnFirework(Vector3 position, Color color)
        {
            var root = new GameObject("Firework");
            root.transform.SetParent(worldRoot, false);
            root.transform.position = position;
            var particles = root.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 1.1f;
            main.startSpeed = 6f;
            main.startSize = 0.16f;
            main.startColor = color;
            main.gravityModifier = 0.15f;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = particles.emission;
            emission.enabled = false;
            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.material = LumiFactory.Material("Firework" + color, color, true, true);
            particles.Emit(55);
            particles.Play();
        }

        private void SetCursor(bool playing)
        {
            pointerWasLocked = playing && Controls.UsesPcControls;
            Cursor.lockState = pointerWasLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !pointerWasLocked;
        }

        public void SetMouseLook(bool enabled)
        {
            SetCursor(enabled && IsPlaying);
        }
        public void ApplyControlScheme()
        {
            LumiMobileInput.Reset();
            if(mobileControlsView!=null)mobileControlsView.SetVisible(IsPlaying&&Controls.UsesMobileControls);
            if(abilityHud!=null)abilityHud.ApplyControlScheme();
            if(IsPlaying)SetCursor(Controls.UsesPcControls&&(CameraRig==null||!CameraRig.MovementFacing));
            else SetCursor(false);
        }
        private void OnApplicationFocus(bool focused)
        {
            if(!focused){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        }
        private void OnDestroy(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}

        private void HideAllPanels()
        {
            menuPanel.SetActive(false);
            hudPanel.SetActive(false);
            resultPanel.SetActive(false);
            pausePanel.SetActive(false);
            dialoguePanel.SetActive(false);
            mobileControlsView.SetVisible(false);
            promptText.gameObject.SetActive(false);
            if(settingsPanel!=null)settingsPanel.SetActive(false);
        }

        private void ClearWorld()
        {
            Player = null;
            CameraRig = null;
            directionArrow = null;
            goal = null;
            if (worldRoot != null)
            {
                worldRoot.gameObject.SetActive(false);
                Destroy(worldRoot.gameObject);
            }
            worldRoot = new GameObject("Lumi World").transform;
            worldRoot.SetParent(transform, false);
            transientPool = worldRoot.gameObject.AddComponent<LumiTransientPool>();
        }

        private static string Stars(int count)
        {
            string value = string.Empty;
            for (int i = 0; i < 3; i++) value += i < count ? "★" : "☆";
            return value;
        }
    }
}





