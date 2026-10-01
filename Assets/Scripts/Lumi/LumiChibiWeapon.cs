using UnityEngine;

namespace LumiAdventure
{
    public static class LumiChibiWeapon
    {
        public static GameObject Pistol(Transform parent,bool hostile)
        {
            GameObject gun=LumiFactory.WorldObject("Chibi service pistol",parent,Vector3.zero);
            Material slide=LumiFactory.Material("Pistol brushed steel",new Color(.66f,.73f,.78f));
            Material dark=LumiFactory.Material("Pistol graphite",new Color(.075f,.105f,.13f));
            Material grip=LumiFactory.Material("Pistol grip",hostile?new Color(.46f,.21f,.15f):new Color(.2f,.31f,.38f));
            Material trim=LumiFactory.Material("Pistol accent",hostile?new Color(1,.39f,.2f):new Color(.3f,.9f,1),true);
            Transform upper=LumiFactory.Primitive("Rounded slide",PrimitiveType.Cube,gun.transform,new Vector3(0,.075f,.1f),new Vector3(.3f,.24f,.68f),slide,false).transform;
            LumiFactory.Primitive("Slide top",PrimitiveType.Cube,upper,Vector3.up*.47f,new Vector3(.84f,.22f,.9f),slide,false);
            LumiFactory.Primitive("Barrel opening",PrimitiveType.Cylinder,gun.transform,new Vector3(0,.06f,.46f),new Vector3(.17f,.045f,.17f),dark,false).transform.localRotation=Quaternion.Euler(90,0,0);
            LumiFactory.Primitive("Frame",PrimitiveType.Cube,gun.transform,new Vector3(0,-.07f,.035f),new Vector3(.25f,.14f,.55f),dark,false);
            LumiFactory.Primitive("Grip",PrimitiveType.Cube,gun.transform,new Vector3(0,-.25f,-.13f),new Vector3(.25f,.35f,.23f),grip,false).transform.localRotation=Quaternion.Euler(-14,0,0);
            for(int s=-1;s<=1;s+=2)
            {
                LumiFactory.Primitive("Grip inset",PrimitiveType.Cube,gun.transform,new Vector3(s*.13f,-.25f,-.12f),new Vector3(.025f,.23f,.14f),dark,false);
                LumiFactory.Primitive("Trigger guard side",PrimitiveType.Cube,gun.transform,new Vector3(s*.105f,-.2f,.13f),new Vector3(.04f,.055f,.24f),dark,false);
                LumiFactory.Primitive("Slide accent",PrimitiveType.Cube,gun.transform,new Vector3(s*.155f,.075f,.2f),new Vector3(.02f,.045f,.23f),trim,false);
            }
            LumiFactory.Primitive("Front sight",PrimitiveType.Cube,gun.transform,new Vector3(0,.22f,.36f),new Vector3(.055f,.07f,.08f),dark,false);
            Transform muzzle=LumiFactory.WorldObject("Muzzle",gun.transform,new Vector3(0,.06f,.53f)).transform;
            GameObject flash=LumiFactory.Primitive("Muzzle flash",PrimitiveType.Sphere,muzzle,new Vector3(0,0,.07f),new Vector3(.18f,.18f,.32f),LumiFactory.Material("Pistol flash",new Color(1,.78f,.24f),true),false);
            gun.AddComponent<LumiWeaponVisual>().Initialize(upper,muzzle,flash);
            return gun;
        }

        public static void Melee(Transform parent,bool heavy)
        {
            Material handle=LumiFactory.Material("Melee handle",new Color(.3f,.19f,.12f));
            Material steel=LumiFactory.Material("Melee steel",new Color(.48f,.55f,.58f));
            LumiFactory.Primitive("Weapon handle",PrimitiveType.Cylinder,parent,new Vector3(0,0,.28f),new Vector3(.12f,.45f,.12f),handle,false).transform.localRotation=Quaternion.Euler(65,0,0);
            LumiFactory.Primitive(heavy?"Chibi hammer":"Chibi baton",PrimitiveType.Cube,parent,new Vector3(0,.28f,.67f),heavy?new Vector3(.72f,.35f,.35f):new Vector3(.23f,.25f,.32f),steel,false);
        }
    }

    public sealed class LumiWeaponVisual:MonoBehaviour
    {
        public Transform Muzzle{get;private set;}
        private Transform slide;
        private Vector3 rest;
        private GameObject flash;
        private float recoil,flashUntil;
        public void Initialize(Transform top,Transform muzzle,GameObject burst)
        {slide=top;rest=top.localPosition;Muzzle=muzzle;flash=burst;flash.SetActive(false);}
        public void Fire(){recoil=.08f;flashUntil=Time.time+.065f;flash.SetActive(true);}
        private void LateUpdate()
        {
            recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*.65f);slide.localPosition=rest-Vector3.forward*recoil;
            if(Time.time>=flashUntil)flash.SetActive(false);
        }
    }
}
