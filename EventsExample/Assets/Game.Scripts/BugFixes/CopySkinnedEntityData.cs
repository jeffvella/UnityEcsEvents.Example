using Unity.Entities;
using Unity.Rendering;

// https://answers.unity.com/questions/1695108/boneindexoffset-error-with-usdz-model-and-ecs.html

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class BugFixCopySkinnedEntityData : SystemBase
{
    /* Suppresses the error: "ArgumentException: A component with type:BoneIndexOffset 
    * has not been added to the entity.", until the Unity bug is fixed. */

    protected override void OnCreate()
    {
        World.GetOrCreateSystem<CopySkinnedEntityDataToRenderEntity>().Enabled = false;
        Enabled = false;
        World.DestroySystem(this);
    }

    protected override void OnUpdate() => throw new System.NotImplementedException();
}


