using UnityEngine;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private sealed class LevelConfig
        {
            public string Name;
            public float Width;
            public float Depth;
            public Color Ground;
            public Color Wall;
            public Color Sky;
            public Color Fog;
            public Color Accent;
            public int Sprouts;
            public int Rangers;
            public int Golems;
            public float Intelligence;
            public Vector3[] Route;
        }

        private void BuildLevel(int level)
        {
            LevelConfig config = GetLevelConfig(level);
            config.Route=ExpeditionRoute(level);
            RenderSettings.fog = true;
            RenderSettings.fogColor = config.Fog;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = level==5?18f:45f;
            RenderSettings.fogEndDistance = level==5?65f:140f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(config.Sky,new Color(.65f,.62f,.55f),.7f)*.75f;

            BuildLighting(config, level);
            BuildBoundaries(config);
            BuildVillage(config, level);
            Physics.SyncTransforms();
            worldRoot.gameObject.AddComponent<LumiNavigation>().Build(worldRoot, config.Width, config.Depth);

            Vector3 start = config.Route[0]+Vector3.up*.08f;
            GameObject playerObject = new GameObject("Lumi", typeof(CharacterController));
            playerObject.transform.SetParent(worldRoot, false);
            playerObject.transform.position = start;
            Player = playerObject.AddComponent<LumiPlayer>();
            Player.Initialize(this);

            GameObject cameraObject = new GameObject("Lumi Camera");
            cameraObject.transform.SetParent(worldRoot, false);
            CameraRig = cameraObject.AddComponent<LumiCameraRig>();
            CameraRig.Initialize(this, Player.transform, config.Sky,new Vector2(config.Width,config.Depth));

            GameObject goalObject = new GameObject("Cổng đích");
            goalObject.transform.SetParent(worldRoot, false);
            goalObject.transform.position = config.Route[config.Route.Length-1];
            goalObject.AddComponent<LumiGoal>().Initialize(this, config.Accent);
            goal = goalObject.transform;

            GameObject arrowObject = new GameObject("Mũi tên chỉ đường");
            arrowObject.transform.SetParent(worldRoot, false);
            directionArrow = arrowObject.AddComponent<LumiDirectionArrow>();
            directionArrow.Initialize(Player.transform, goal);

            SpawnVillageBoss(level,config);
            SpawnEnemies(config);
            SpawnStars(config);
            SpawnItems(config);
            SpawnNpc(config, level);
        }

        private void BuildLighting(LevelConfig config, int level)
        {
            var lightObject = new GameObject("Ánh sáng chính");
            lightObject.transform.SetParent(worldRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(42f, level * 34f - 70f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.Lerp(Color.white, config.Accent, 0.08f);
            light.intensity = level == 3 ? 0.9f : 1.15f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.72f;
            // Soft opposing portrait lights keep the chibi face readable as the player turns.
            AddCharacterFill("Naruto warm key",new Vector3(28,155,0),new Color(1,.94f,.86f),.95f);
            AddCharacterFill("Naruto cool fill",new Vector3(35,-25,0),new Color(.82f,.9f,1),.55f);

        }

        private void AddCharacterFill(string name,Vector3 rotation,Color color,float intensity)
        {
            var obj=new GameObject(name);obj.transform.SetParent(worldRoot,false);obj.transform.rotation=Quaternion.Euler(rotation);
            var light=obj.AddComponent<Light>();light.type=LightType.Directional;light.color=color;light.intensity=intensity;light.cullingMask=1<<30;light.shadows=LightShadows.None;
        }

        private void BuildBoundaries(LevelConfig config)
        {
            Material ground = LumiFactory.Material("Ground" + currentLevel, config.Ground);
            ground.mainTexture = LumiTerrainFinish.Texture(currentLevel);
            ground.mainTextureScale = new Vector2(config.Width / 5f, config.Depth / 5f);
            Material wall = LumiFactory.Material("Boundary" + currentLevel, config.Wall);
            CreateBlock("Mặt đất", Vector3.down * 0.5f, new Vector3(config.Width, 1f, config.Depth), ground);
            CreateBlock("Scenic ground",Vector3.down*1.1f,new Vector3(config.Width+38f,1f,config.Depth+32f),ground);
            float h = 2.5f;
            HideBoundary(CreateBlock("Tường Bắc", new Vector3(0f, h * 0.5f, config.Depth * 0.5f), new Vector3(config.Width + 2f, h, 1.4f), wall));
            HideBoundary(CreateBlock("Tường Nam", new Vector3(0f, h * 0.5f, -config.Depth * 0.5f), new Vector3(config.Width + 2f, h, 1.4f), wall));
            HideBoundary(CreateBlock("Tường Đông", new Vector3(config.Width * 0.5f, h * 0.5f, 0f), new Vector3(1.4f, h, config.Depth + 2f), wall));
            HideBoundary(CreateBlock("Tường Tây", new Vector3(-config.Width * 0.5f, h * 0.5f, 0f), new Vector3(1.4f, h, config.Depth + 2f), wall));
        }

        private void HideBoundary(GameObject boundary)
        {
            Renderer renderer = boundary.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private void SpawnEnemies(LevelConfig config)
        {
            SpawnEnemyGroup(LumiEnemyType.Sprout, RandomizedCount(config.Sprouts), config);
            SpawnEnemyGroup(LumiEnemyType.Ranger, RandomizedCount(config.Rangers), config);
            SpawnEnemyGroup(LumiEnemyType.Golem, RandomizedCount(config.Golems), config);
        }

        private static int RandomizedCount(int baseline)
        {
            if (baseline <= 0) return 0;
            int spread = Mathf.Max(1, Mathf.CeilToInt(baseline * 0.2f));
            return Random.Range(Mathf.Max(1, baseline - spread), baseline + spread + 1);
        }

        private void SpawnEnemyGroup(LumiEnemyType type, int count, LevelConfig config)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 preferred=RoutePoint(config.Route,Random.Range(.18f,.92f))+Vector3.right*Random.Range(-8f,8f);
                if (!TryFindOpenPosition(config, 14f, out Vector3 position,preferred)) continue;
                var enemyObject = new GameObject(type.ToString(), typeof(CharacterController));
                enemyObject.transform.SetParent(worldRoot, false);
                enemyObject.transform.position = position;
                enemyObject.AddComponent<LumiEnemy>().Initialize(this, Player, type, config.Intelligence);
            }
        }

        private void SpawnStars(LevelConfig config)
        {
            for (int i = 0; i < totalStars; i++)
            {
                if (!TryFindOpenPosition(config, 7f, out Vector3 position,RoutePoint(config.Route,(i+1f)/6f))) continue;
                position += Vector3.up * 1.05f;
                var star = new GameObject("Sao " + (i + 1));
                star.transform.SetParent(worldRoot, false);
                star.transform.position = position;
                star.AddComponent<LumiPickup>().Initialize(this, LumiPickupType.Star);
            }
        }

        private void SpawnItems(LevelConfig config)
        {
            LumiPickupType[] items = { LumiPickupType.Health, LumiPickupType.Armor, LumiPickupType.Speed, LumiPickupType.Invisibility };
            for (int i = 0; i < items.Length; i++)
            {
                float progress=i==2?.055f:(i+1)*.12f;
                if (!TryFindOpenPosition(config, 3f, out Vector3 position,RoutePoint(config.Route,progress))) continue;
                position += Vector3.up * 0.9f;
                var item = new GameObject("Vật phẩm " + items[i]);
                item.transform.SetParent(worldRoot, false);
                item.transform.position = position;
                item.AddComponent<LumiPickup>().Initialize(this, items[i]);
            }
        }

        private void SpawnNpc(LevelConfig config, int level)
        {
            string[] names = { "Mộc", "Sahir", "Băng Lam", "Hỏa Tâm", "Nova" };
            string[] messages =
            {
                "Lumi, hãy men theo con đường giữa hai hàng cây. Những ngôi sao sẽ cho biết bạn đã khám phá vùng đất kỹ đến đâu.",
                "Tàn tích thay đổi lối đi bằng những bức tường so le. Hãy quan sát khoảng trống và đừng đứng yên trước Ranger.",
                "Pha lê xanh đánh dấu con đường an toàn. Gió lạnh khiến kẻ địch cảnh giác hơn, hãy dùng áo choàng đúng lúc.",
                "Hồ độc gây sát thương liên tục. Hãy bước trên các phiến đá, giữ giáp và tận dụng ánh tím để tìm đường qua hầm ngục.",
                "Cổng cuối ở cuối đại lộ Neon. Ba loại quái đều đang canh giữ nơi đó — hành trình chinh phục sắp hoàn tất!"
            };
            Color[] colors =
            {
                new Color(0.96f, 0.56f, 0.25f), new Color(0.88f, 0.65f, 0.22f), new Color(0.28f, 0.7f, 0.95f),
                new Color(0.85f, 0.2f, 0.18f), new Color(0.8f, 0.25f, 0.88f)
            };
            Vector3 position = config.Route[0]+new Vector3(-3.2f,0,3f);
            var npc = new GameObject(names[level - 1]);
            npc.transform.SetParent(worldRoot, false);
            npc.transform.position = position;
            npc.transform.rotation = Quaternion.LookRotation(Vector3.back);
            npc.AddComponent<LumiNpc>().Initialize(this, names[level - 1], messages[level - 1], colors[level - 1]);
        }

        private bool TryFindOpenPosition(LevelConfig config, float startClearance, out Vector3 position,Vector3? preferred=null)
        {
            Physics.SyncTransforms();
            for (int attempt = 0; attempt < 80; attempt++)
            {
                Vector3 point = new Vector3(
                    Random.Range(-config.Width * 0.5f + 3f, config.Width * 0.5f - 3f),
                    0.06f,
                    Random.Range(-config.Depth * 0.5f + 7f, config.Depth * 0.5f - 7f));
                if(preferred.HasValue && attempt<30)point=preferred.Value+new Vector3(Random.Range(-3f,3f),.06f,Random.Range(-3f,3f));
                Vector3 start = config.Route[0];
                Vector3 end = config.Route[config.Route.Length-1];
                if (Vector3.Distance(point, start) < startClearance || Vector3.Distance(point, end) < 4f) continue;
                if (Physics.CheckSphere(point + Vector3.up * 0.9f, 0.8f, ~0, QueryTriggerInteraction.Collide)) continue;
                if (!UnityEngine.AI.NavMesh.SamplePosition(point, out UnityEngine.AI.NavMeshHit hit, .5f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                var path = new UnityEngine.AI.NavMeshPath();
                if (!UnityEngine.AI.NavMesh.CalculatePath(start, hit.position, UnityEngine.AI.NavMesh.AllAreas, path)
                    || path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) continue;
                position = hit.position + Vector3.up * .06f;
                return true;
            }
            position = Vector3.zero;
            Debug.LogWarning("No safe spawn position; skipped rather than placing an actor in a wall.");
            return false;
        }

        private GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material, Quaternion rotation = default(Quaternion))
        {
            GameObject block = LumiFactory.Primitive(name, PrimitiveType.Cube, worldRoot, position, scale, material, true);
            block.transform.position = position;
            if (rotation != default(Quaternion)) block.transform.rotation = rotation;
            return block;
        }

        private void CreateTree(Vector3 position, Material trunk, Material leaves)
        {
            LumiFactory.Primitive("Thân cây", PrimitiveType.Cylinder, worldRoot, position + Vector3.up * 1.5f,
                new Vector3(0.7f, 1.5f, 0.7f), trunk, true);
            LumiFactory.Primitive("Tán cây", PrimitiveType.Sphere, worldRoot, position + Vector3.up * 3.5f,
                new Vector3(2.5f, 2.1f, 2.5f), leaves, false);
        }

        private void CreatePillar(Vector3 position, Material body, Material accent, float height)
        {
            LumiFactory.Primitive("Cột", PrimitiveType.Cylinder, worldRoot, position + Vector3.up * height * 0.5f,
                new Vector3(0.8f, height * 0.5f, 0.8f), body, true);
            LumiFactory.Primitive("Đỉnh cột", PrimitiveType.Cube, worldRoot, position + Vector3.up * height,
                new Vector3(1.8f, 0.45f, 1.8f), accent, false);
        }

        private void CreateCrystal(Vector3 position, Material material, float height)
        {
            GameObject crystal = LumiFactory.Primitive("Pha lê", PrimitiveType.Cube, worldRoot,
                position + Vector3.up * height * 0.5f, new Vector3(0.75f, height, 0.75f), material, false);
            crystal.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 180f), 45f);
        }

        private void CreateHazard(Vector3 position, Vector3 scale, Material material)
        {
            GameObject hazard = CreateBlock("Dung nham", position, scale, material);
            Collider collider = hazard.GetComponent<Collider>();
            collider.isTrigger = true;
            hazard.AddComponent<LumiHazard>();
        }

        private static LevelConfig GetLevelConfig(int level)
        {
            switch (level)
            {
                case 1: return new LevelConfig
                {
                    Name = "Làng Lá", Width = 96f, Depth = 140f,
                    Ground = new Color(.25f,.38f,.25f), Wall = new Color(0.12f, 0.34f, 0.18f),
                    Sky = new Color(0.42f, 0.72f, 0.92f), Fog = new Color(0.55f, 0.78f, 0.72f), Accent = new Color(0.15f, 0.95f, 0.62f),
                    Sprouts = 12, Rangers = 0, Golems = 0, Intelligence = 1f
                };
                case 2: return new LevelConfig
                {
                    Name = "Làng Cát", Width = 110f, Depth = 150f,
                    Ground = new Color(0.72f, 0.47f, 0.2f), Wall = new Color(0.48f, 0.28f, 0.12f),
                    Sky = new Color(0.92f, 0.58f, 0.28f), Fog = new Color(0.82f, 0.55f, 0.3f), Accent = new Color(1f, 0.66f, 0.12f),
                    Sprouts = 15, Rangers = 5, Golems = 0, Intelligence = 1.6f
                };
                case 3: return new LevelConfig
                {
                    Name = "Làng Đá", Width = 110f, Depth = 156f,
                    Ground = new Color(.49f,.46f,.39f), Wall = new Color(0.2f, 0.42f, 0.62f),
                    Sky = new Color(.55f,.68f,.77f), Fog = new Color(.62f,.64f,.6f), Accent = new Color(0.25f, 0.88f, 1f),
                    Sprouts = 18, Rangers = 8, Golems = 0, Intelligence = 2.3f
                };
                case 4: return new LevelConfig
                {
                    Name = "Làng Mây", Width = 106f, Depth = 160f,
                    Ground = new Color(.57f,.62f,.56f), Wall = new Color(0.16f, 0.19f, 0.17f),
                    Sky = new Color(.63f,.8f,.94f), Fog = new Color(.83f,.9f,.95f), Accent = new Color(0.25f, 1f, 0.36f),
                    Sprouts = 16, Rangers = 9, Golems = 4, Intelligence = 3f
                };
                default: return new LevelConfig
                {
                    Name = "Làng Sương Mù", Width = 110f, Depth = 150f,
                    Ground = new Color(.36f,.45f,.42f), Wall = new Color(0.1f, 0.04f, 0.16f),
                    Sky = new Color(.52f,.67f,.73f), Fog = new Color(.64f,.77f,.8f), Accent = new Color(0.12f, 0.95f, 1f),
                    Sprouts = 22, Rangers = 12, Golems = 6, Intelligence = 4f
                };
            }
        }

        private static string LevelName(int level) => GetLevelConfig(level).Name;
        private static Color LevelAccent(int level) => GetLevelConfig(level).Accent;
    }

}


