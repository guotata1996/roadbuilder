using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class InputSystem : ComponentSystem
{
    protected override void OnUpdate(){
        // Question: can I use ref StreamingLogicConfig?
        Entities.ForEach((ref Translation trans, ref Rotation rot, ref StreamingLogicConfig stream) =>{
            

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){
                float speed = Input.GetKey(KeyCode.W) ? 20 : -20;
                float3 newPos = trans.Value + math.mul(rot.Value, new float3(0,0,1)) * Time.deltaTime * speed;
                trans.Value = newPos;
            }
        });
    }
}
