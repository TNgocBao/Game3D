using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    // Uses the built-in AI module; no package or second movement controller.
    public sealed class LumiNavigation : MonoBehaviour
    {
        private NavMeshDataInstance instance;
        private NavMeshData data;
        public void Build(Transform root,float width,float depth)
        {
            var sources=new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(root,~0,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
            NavMeshBuildSettings settings=NavMesh.GetSettingsByID(0);
            settings.agentRadius=.65f;settings.agentHeight=2.6f;settings.agentClimb=.35f;
            settings.overrideTileSize=true;settings.tileSize=128;
            data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(width+4,16,depth+4)),Vector3.zero,Quaternion.identity);
            if(data!=null)instance=NavMesh.AddNavMeshData(data);
        }
        private void OnDisable(){if(instance.valid)instance.Remove();}
        private void OnDestroy(){if(instance.valid)instance.Remove();if(data!=null)Destroy(data);}
    }
}
