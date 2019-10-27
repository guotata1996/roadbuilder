using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[ExecuteAlways]
public class StreamingLogicSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_EntityCommandBufferSystem;
    NativeList<Entity> m_AddRequestList;
    NativeList<Entity> m_RemoveRequestList;

    //[BurstCompile]
    [ExcludeComponent(typeof(RequestSceneLoaded))]
    struct StreamSubScenesIn : IJobForEachWithEntity<SceneData>
    {
        public NativeList<Entity> AddRequestList;
        public float3 CameraPosition;
        public float MaxDistanceSquared;

        public void Execute(Entity entity, int index, [ReadOnly]ref SceneData sceneData)
        {
            var distanceSq = ((AABB)sceneData.BoundingVolume).DistanceSq(CameraPosition);
            if (distanceSq < MaxDistanceSquared)
                AddRequestList.Add(entity);
        }
    }

    //[BurstCompile]
    [RequireComponentTag(typeof(RequestSceneLoaded))]
    struct StreamSubScenesOut : IJobForEachWithEntity<SceneData>
    {
        public NativeList<Entity> RemoveRequestList;
        public float3 CameraPosition;
        public float MaxDistanceSquared;

        public void Execute(Entity entity, int index, [ReadOnly]ref SceneData sceneData)
        {

            var distanceSq = ((AABB)sceneData.BoundingVolume).DistanceSq(CameraPosition);
            if (distanceSq > MaxDistanceSquared)
                RemoveRequestList.Add(entity);
        }
    }

    struct BuildCommandBufferJob : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        public NativeArray<Entity> AddRequestArray;
        public NativeArray<Entity> RemoveRequestArray;

        public void Execute()
        {
            foreach (var entity in AddRequestArray)
            {
                CommandBuffer.AddComponent(entity, default(RequestSceneLoaded));
            }
            foreach (var entity in RemoveRequestArray)
            {
                CommandBuffer.RemoveComponent<RequestSceneLoaded>(entity);
            }
        }
    }

    protected override void OnCreateManager()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();

        m_AddRequestList = new NativeList<Entity>(Allocator.Persistent);
        m_RemoveRequestList = new NativeList<Entity>(Allocator.Persistent);

        RequireSingletonForUpdate<StreamingLogicConfig>();
    }

    protected override void OnDestroyManager()
    {
        m_AddRequestList.Dispose();
        m_RemoveRequestList.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var configEntity = GetSingletonEntity<StreamingLogicConfig>();

        var config = EntityManager.GetComponentData<StreamingLogicConfig>(configEntity);
        var cameraPosition = EntityManager.GetComponentData<LocalToWorld>(configEntity).Position;

        m_AddRequestList.Clear();
        var streamInHandle = new StreamSubScenesIn
        {
            AddRequestList = m_AddRequestList,
            CameraPosition = cameraPosition,
            MaxDistanceSquared = config.DistanceForStreamingIn * config.DistanceForStreamingIn
        }.ScheduleSingle(this, inputDeps);

        m_RemoveRequestList.Clear();
        var streamOutHandle = new StreamSubScenesOut
        {
            RemoveRequestList = m_RemoveRequestList,
            CameraPosition = cameraPosition,
            MaxDistanceSquared = config.DistanceForStreamingOut * config.DistanceForStreamingOut
        }.ScheduleSingle(this, inputDeps);

        var combinedHandle = JobHandle.CombineDependencies(streamInHandle, streamOutHandle);
        var commandHandle = new BuildCommandBufferJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            AddRequestArray = m_AddRequestList.AsDeferredJobArray(),
            RemoveRequestArray = m_RemoveRequestList.AsDeferredJobArray()
        }.Schedule(combinedHandle);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(commandHandle);

        return commandHandle;
    }
}
