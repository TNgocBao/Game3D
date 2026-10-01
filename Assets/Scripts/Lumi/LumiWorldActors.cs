using UnityEngine;

namespace LumiAdventure
{
    public enum LumiPickupType { Health, Armor, Speed, Invisibility, Star }

    public class LumiPickup : MonoBehaviour
    {
        private LumiGame game;
        private LumiPickupType type;
        private Vector3 basePosition;

        public void Initialize(LumiGame owner, LumiPickupType pickupType)
        {
            game = owner;
            type = pickupType;
            basePosition = transform.position;
            BuildVisual();
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.8f;
            collider.isTrigger = true;
        }

        private void BuildVisual()
        {
            Transform visual = LumiFactory.WorldObject("Visual", transform, Vector3.zero).transform;
            if (type == LumiPickupType.Star)
            {
                Material gold = LumiFactory.Material("Star", new Color(1f, 0.78f, 0.05f), true);
                LumiFactory.Primitive("Star Core", PrimitiveType.Sphere, visual, Vector3.zero, Vector3.one * 0.45f, gold, false);
                for (int i = 0; i < 5; i++)
                {
                    var ray = LumiFactory.Primitive("Star Ray", PrimitiveType.Cube, visual, Vector3.zero, new Vector3(0.18f, 0.7f, 0.13f), gold, false);
                    ray.transform.localRotation = Quaternion.Euler(0f, 0f, i * 72f);
                }
            }
            else if (type == LumiPickupType.Health)
            {
                Material red = LumiFactory.Material("Heart", new Color(0.95f, 0.08f, 0.18f), true);
                LumiFactory.Primitive("Heart L", PrimitiveType.Sphere, visual, new Vector3(-0.22f, 0.15f, 0f), Vector3.one * 0.43f, red, false);
                LumiFactory.Primitive("Heart R", PrimitiveType.Sphere, visual, new Vector3(0.22f, 0.15f, 0f), Vector3.one * 0.43f, red, false);
                var tip = LumiFactory.Primitive("Heart Tip", PrimitiveType.Cube, visual, new Vector3(0f, -0.18f, 0f), new Vector3(0.55f, 0.55f, 0.35f), red, false);
                tip.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            else if (type == LumiPickupType.Armor)
            {
                Material white = LumiFactory.Material("Armor", new Color(0.85f, 0.95f, 1f), true);
                LumiFactory.Primitive("Shield", PrimitiveType.Sphere, visual, Vector3.zero, new Vector3(0.62f, 0.8f, 0.18f), white, false);
                LumiFactory.Primitive("Shield Cut", PrimitiveType.Cube, visual, new Vector3(0f, 0.25f, 0.11f), new Vector3(0.52f, 0.12f, 0.08f), LumiFactory.Material("ArmorLine", new Color(0.3f, 0.7f, 1f), true), false);
            }
            else if (type == LumiPickupType.Speed)
            {
                Material white = LumiFactory.Material("Shoe", Color.white, true);
                LumiFactory.Primitive("Shoe", PrimitiveType.Cube, visual, new Vector3(0f, -0.05f, 0f), new Vector3(0.9f, 0.32f, 0.38f), white, false);
                LumiFactory.Primitive("Ankle", PrimitiveType.Cube, visual, new Vector3(-0.28f, 0.25f, 0f), new Vector3(0.34f, 0.48f, 0.36f), white, false);
            }
            else
            {
                Material cloak = LumiFactory.Material("Cloak", new Color(0.64f, 0.78f, 1f, 0.7f), true, true);
                LumiFactory.Primitive("Cloak", PrimitiveType.Capsule, visual, Vector3.zero, new Vector3(0.65f, 0.8f, 0.25f), cloak, false);
                LumiFactory.Primitive("Hood", PrimitiveType.Sphere, visual, new Vector3(0f, 0.62f, 0f), Vector3.one * 0.42f, cloak, false);
            }
        }

        private void Update()
        {
            transform.Rotate(0f, 70f * Time.deltaTime, 0f);
            transform.position = basePosition + Vector3.up * (0.18f + Mathf.Sin(Time.time * 2.5f) * 0.16f);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<LumiPlayer>();
            if (player == null) return;
            switch (type)
            {
                case LumiPickupType.Health: player.Heal(5); game.ShowToast("+5 MÁU", new Color(1f, 0.3f, 0.35f)); break;
                case LumiPickupType.Armor: player.AddArmor(5); game.ShowToast("+5 GIÁP", new Color(0.75f, 0.95f, 1f)); break;
                case LumiPickupType.Speed: player.BoostSpeed(8f); game.ShowToast("TĂNG TỐC 8 GIÂY", Color.white); break;
                case LumiPickupType.Invisibility: player.BecomeInvisible(5f); game.ShowToast("TÀNG HÌNH 5 GIÂY", new Color(0.65f, 0.8f, 1f)); break;
                case LumiPickupType.Star: game.CollectStar(); break;
            }
            game.SpawnImpact(transform.position, type == LumiPickupType.Star ? new Color(1f, 0.8f, 0.05f) : Color.white);
            game.Audio.Play(type == LumiPickupType.Star ? "star" : "pickup");
            Destroy(gameObject);
        }
    }

    public class LumiNpc : MonoBehaviour
    {
        private LumiGame game;
        private string speaker;
        private string message;
        private bool nearby;

        public void Initialize(LumiGame owner, string npcName, string dialogue, Color color)
        {
            game = owner;
            speaker = npcName;
            message = dialogue;
            BuildVisual(color);
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 2.2f;
            collider.isTrigger = true;
        }

        private void BuildVisual(Color color)
        {
            LumiCharacterArt.Soldier(transform,color);
        }

        private void Update()
        {
            if (!nearby || !game.IsPlaying) return;
            if (Input.GetKeyDown(KeyCode.E) || LumiMobileInput.ConsumeInteract())
            {
                game.Audio.Play("dialogue");
                game.ShowDialogue(speaker, message);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<LumiPlayer>() == null) return;
            nearby = true;
            game.ShowInteractionPrompt("Nhấn E để nói chuyện với " + speaker);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<LumiPlayer>() == null) return;
            nearby = false;
            game.HideInteractionPrompt();
        }
    }

    public class LumiGoal : MonoBehaviour
    {
        private LumiGame game;

        public void Initialize(LumiGame owner, Color color)
        {
            game = owner;
            Material glow = LumiFactory.Material("Goal" + color, color, true, true);
            Shader portalShader = Shader.Find("Vanguard/Portal");
            if (portalShader != null)
            {
                Material energy = new Material(portalShader);
                energy.color = new Color(.13f,.78f,1f);
                LumiFactory.Primitive("Portal energy",PrimitiveType.Quad,transform,new Vector3(0,1.6f,0),new Vector3(2.6f,2.6f,1),energy,false);
            }
            Material frame = LumiFactory.Material("Portal frame",new Color(.23f,.3f,.32f));
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 point = new Vector3(Mathf.Cos(angle) * 1.35f, 1.6f + Mathf.Sin(angle) * 1.35f, 0f);
                LumiFactory.Primitive("Portal", PrimitiveType.Sphere, transform, point, Vector3.one * 0.24f, glow, false);
                LumiFactory.Primitive("Portal stone",PrimitiveType.Cube,transform,point*1.08f,new Vector3(.4f,.4f,.45f),frame,false).transform.localRotation=Quaternion.Euler(0,0,i*22.5f);
            }
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.6f, 3.5f, 1.2f);
            collider.center = Vector3.up * 1.6f;
            collider.isTrigger = true;
            CreatePortalParticles(color);
        }

        private void CreatePortalParticles(Color color)
        {
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 1.6f;
            main.startSpeed = 0.35f;
            main.startSize = 0.12f;
            main.startColor = color;
            main.maxParticles = 80;
            var emission = particles.emission;
            emission.rateOverTime = 18f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.45f;
            var renderer = gameObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = LumiFactory.Material("PortalParticles" + color, color, true, true);
        }

        private void Update()
        {
            // The gate stays aligned with the exit; energy animation lives in its shader.
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<LumiPlayer>() != null) game.WinLevel();
        }
    }

    public class LumiHazard : MonoBehaviour
    {
        private float nextDamage;
        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextDamage) return;
            var player = other.GetComponentInParent<LumiPlayer>();
            if (player == null) return;
            nextDamage = Time.time + 1f;
            player.TakeDamage(2, player.transform.position);
        }
    }
}

