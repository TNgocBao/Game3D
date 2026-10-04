using UnityEngine;

namespace LumiAdventure
{
    public static class LumiBossArt
    {
        private static GameObject Part(string name,PrimitiveType shape,Transform parent,Vector3 position,Vector3 scale,Material material)=>LumiFactory.Primitive(name,shape,parent,position,scale,material,false);
        private static void AddJoints(Transform root)
        {
            Transform body=new GameObject("Body pivot").transform;body.SetParent(root,false);body.localPosition=new Vector3(0,.9f,0);
            Transform head=new GameObject("Head pivot").transform;head.SetParent(body,false);head.position=root.TransformPoint(new Vector3(0,1.52f,0));
            var children=new System.Collections.Generic.List<Transform>();foreach(Transform child in root)children.Add(child);
            foreach(Transform child in children){if(child==body || child.name.Contains("leg"))continue;bool face=child.localPosition.y>1.5f || child.name.Contains("hair") || child.name.Contains("moustache");child.SetParent(face?head:body,true);}
            foreach(string side in new[]{"Left","Right"}){
                Transform arm=null,leg=null;foreach(Transform t in root.GetComponentsInChildren<Transform>()){if(t.name==side+" arm")arm=t;if(t.name==side+" leg")leg=t;}
                if(arm!=null){Transform elbow=new GameObject(side+" elbow").transform;elbow.SetParent(arm,false);elbow.localPosition=new Vector3(0,-.27f,0);var pieces=new System.Collections.Generic.List<Transform>();foreach(Transform t in arm)pieces.Add(t);foreach(Transform t in pieces)if(t.name=="Hand"){t.SetParent(elbow,true);Transform palm=new GameObject(side+" hand").transform;palm.SetParent(elbow,false);palm.localPosition=new Vector3(0,-.21f,.08f);}foreach(Transform t in pieces)if(t.name=="Sleeve"){t.localPosition=new Vector3(0,-.12f,0);t.localScale=new Vector3(t.localScale.x,.15f,t.localScale.z);var fore=Object.Instantiate(t.gameObject,elbow,false);fore.name="Forearm";fore.transform.localPosition=new Vector3(0,-.09f,0);fore.transform.localScale=new Vector3(t.localScale.x*.85f,.12f,t.localScale.z*.85f);}}
                if(leg!=null){Transform knee=new GameObject(side+" knee").transform;knee.SetParent(leg,false);knee.localPosition=new Vector3(0,-.24f,0);Transform foot=new GameObject(side+" foot").transform;foot.SetParent(knee,false);foot.localPosition=new Vector3(0,-.25f,.1f);Transform sandal=null;foreach(Transform t in leg)if(t.name=="Ninja sandal")sandal=t;if(sandal!=null)sandal.SetParent(foot,true);Transform shin=null;foreach(Transform t in leg)if(t.name=="Shin")shin=t;if(shin!=null){shin.localPosition=new Vector3(0,-.1f,0);shin.localScale=new Vector3(shin.localScale.x,.14f,shin.localScale.z);var calf=Object.Instantiate(shin.gameObject,knee,false);calf.name="Lower shin";calf.transform.localPosition=new Vector3(0,-.12f,0);calf.transform.localScale=new Vector3(shin.localScale.x,.12f,shin.localScale.z);}}
            }
            var accessories=new System.Collections.Generic.List<Transform>();foreach(Transform t in body)if(t.name=="Golden wrist guard" || t.name=="Armour shoulder")accessories.Add(t);
            foreach(Transform t in accessories){string side=root.InverseTransformPoint(t.position).x<0?"Left":"Right";foreach(Transform joint in root.GetComponentsInChildren<Transform>())if(joint.name==side+(t.name=="Golden wrist guard"?" elbow":" arm")){t.SetParent(joint,true);break;}}
        }
        public static GameObject Create(Transform parent,int village)
        {
            Transform root=LumiFactory.WorldObject("Kage chibi 3D",parent,Vector3.zero).transform;
            Color costume=village==1?new Color(.65f,.12f,.11f):village==2?new Color(.55f,.15f,.16f):village==3?new Color(.39f,.48f,.19f):village==4?new Color(.86f,.88f,.83f):new Color(.12f,.3f,.55f);
            Material cloth=LumiFactory.Material("Kage costume"+village,costume);
            Material dark=LumiFactory.Material("Kage dark fabric",new Color(.08f,.1f,.14f));
            Material skin=LumiFactory.Material("Kage skin"+village,village==4?new Color(.53f,.3f,.16f):new Color(.97f,.74f,.57f));
            Material hair=LumiFactory.Material("Kage hair"+village,village==1?new Color(.055f,.045f,.04f):village==2?new Color(.65f,.1f,.075f):village==5?new Color(.55f,.2f,.09f):new Color(.87f,.84f,.67f));
            Part("Torso",PrimitiveType.Capsule,root,new Vector3(0,.94f,0),new Vector3(village==4?1.05f:.76f,.5f,.58f),village==4?skin:cloth);
            Part("Face",PrimitiveType.Sphere,root,new Vector3(0,1.67f,.05f),new Vector3(.82f,.78f,.76f),skin);
            Part("Hair cap",PrimitiveType.Sphere,root,new Vector3(0,1.94f,-.06f),new Vector3(.85f,.36f,.76f),hair);
            for(int side=-1;side<=1;side+=2)
            {
                Transform leg=LumiFactory.WorldObject(side<0?"Left leg":"Right leg",root,new Vector3(side*.22f,.58f,0)).transform;
                Part("Shin",PrimitiveType.Capsule,leg,new Vector3(0,-.2f,0),new Vector3(.3f,.26f,.3f),village==4?cloth:dark);
                Part("Ninja sandal",PrimitiveType.Cube,leg,new Vector3(0,-.49f,.1f),new Vector3(.35f,.15f,.5f),dark);
                Transform arm=LumiFactory.WorldObject(side<0?"Left arm":"Right arm",root,new Vector3(side*(village==4?.6f:.44f),1.15f,0)).transform;
                Part("Sleeve",PrimitiveType.Capsule,arm,new Vector3(0,-.18f,0),new Vector3(village==4?.4f:.27f,.3f,.3f),village==4?skin:cloth);
                Part("Hand",PrimitiveType.Sphere,arm,new Vector3(0,-.48f,.08f),Vector3.one*.27f,skin);
                Part("Eye",PrimitiveType.Sphere,root,new Vector3(side*.17f,1.7f,.401f),new Vector3(.105f,.15f,.035f),dark);
                if(village==1 || village==5)Part("Long hair",PrimitiveType.Capsule,root,new Vector3(side*.36f,1.46f,-.18f),new Vector3(.23f,.6f,.3f),hair);
            }
            if(village==1)
            {
                Part("Long black hair back",PrimitiveType.Cube,root,new Vector3(0,1.44f,-.34f),new Vector3(.72f,1.05f,.25f),hair);
                for(int i=0;i<3;i++)Part("Red lamellar armour",PrimitiveType.Cube,root,new Vector3(0,1.19f-i*.18f,.33f),new Vector3(.8f,.14f,.1f),cloth);
                for(int side=-1;side<=1;side+=2)Part("Armour shoulder",PrimitiveType.Cube,root,new Vector3(side*.5f,1.24f,0),new Vector3(.38f,.24f,.65f),cloth);
                Part("Forehead protector",PrimitiveType.Cube,root,new Vector3(0,1.91f,.32f),new Vector3(.57f,.13f,.09f),LumiFactory.Material("Kage steel",new Color(.65f,.7f,.73f)));
            }
            else if(village==2)
            {
                Part("Sand gourd bottom",PrimitiveType.Sphere,root,new Vector3(.18f,1.06f,-.58f),new Vector3(.78f,.85f,.65f),LumiFactory.Material("Gaara gourd",new Color(.75f,.56f,.28f)));
                Part("Sand gourd top",PrimitiveType.Sphere,root,new Vector3(.18f,1.63f,-.59f),new Vector3(.48f,.49f,.46f),LumiFactory.Material("Gaara gourd",new Color(.75f,.56f,.28f)));
                Part("Gourd cork",PrimitiveType.Cylinder,root,new Vector3(.18f,1.97f,-.59f),new Vector3(.2f,.13f,.2f),dark);
                Part("Cross-body sash",PrimitiveType.Cube,root,new Vector3(0,1.07f,.32f),new Vector3(.15f,.84f,.075f),LumiFactory.Material("Gaara sash",new Color(.68f,.56f,.43f))).transform.localRotation=Quaternion.Euler(0,0,-35);
                for(int i=0;i<7;i++)Part("Red hair tuft",PrimitiveType.Cube,root,new Vector3((i-3)*.11f,2.06f,.05f),new Vector3(.13f,.27f,.26f),hair).transform.localRotation=Quaternion.Euler(0,0,(i-3)*-9);
                Part("Forehead mark",PrimitiveType.Cube,root,new Vector3(.23f,1.89f,.37f),new Vector3(.07f,.12f,.025f),cloth);
            }
            else if(village==3)
            {
                Part("Large red nose",PrimitiveType.Sphere,root,new Vector3(0,1.64f,.48f),new Vector3(.3f,.25f,.24f),LumiFactory.Material("Onoki nose",new Color(.85f,.42f,.32f)));
                for(int side=-1;side<=1;side+=2)Part("White moustache",PrimitiveType.Capsule,root,new Vector3(side*.16f,1.51f,.42f),new Vector3(.33f,.075f,.1f),hair).transform.localRotation=Quaternion.Euler(0,0,90);
                Part("White topknot",PrimitiveType.Sphere,root,new Vector3(0,2.17f,-.12f),Vector3.one*.22f,hair);
                Part("Red cape",PrimitiveType.Cube,root,new Vector3(0,1.1f,-.32f),new Vector3(.92f,.9f,.14f),LumiFactory.Material("Onoki cape",new Color(.59f,.16f,.13f)));
                root.localScale=Vector3.one*.84f;
            }
            else if(village==4)
            {
                Material gold=LumiFactory.Material("Raikage gold guards",new Color(.81f,.61f,.2f));
                for(int side=-1;side<=1;side+=2)Part("Golden wrist guard",PrimitiveType.Cylinder,root,new Vector3(side*.6f,.86f,0),new Vector3(.45f,.15f,.45f),gold);
                Part("Raikage belt",PrimitiveType.Cube,root,new Vector3(0,.66f,.28f),new Vector3(.9f,.16f,.14f),gold);
                for(int side=-1;side<=1;side+=2)Part("Blond moustache",PrimitiveType.Cube,root,new Vector3(side*.12f,1.48f,.41f),new Vector3(.23f,.08f,.075f),hair);
                root.localScale=Vector3.one*1.15f;
            }
            else
            {
                Part("Blue dress",PrimitiveType.Capsule,root,new Vector3(0,.68f,0),new Vector3(.8f,.37f,.6f),cloth);
                Part("Copper hair fringe",PrimitiveType.Capsule,root,new Vector3(.15f,1.85f,.32f),new Vector3(.33f,.26f,.14f),hair).transform.localRotation=Quaternion.Euler(0,0,-25);
                Part("Long copper hair back",PrimitiveType.Capsule,root,new Vector3(0,1.36f,-.35f),new Vector3(.72f,.63f,.3f),hair);
                Part("Mei neckline",PrimitiveType.Cube,root,new Vector3(0,1.29f,.3f),new Vector3(.46f,.16f,.08f),skin);
            }
            AddJoints(root);
            root.gameObject.AddComponent<LumiInfantryMotion>();return root.gameObject;
        }
    }
}
