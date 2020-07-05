using Unity.Animation;
using Unity.Entities;

[UpdateInGroup(typeof(PostAnimationSystemGroup))]
[UpdateAfter(typeof(RigComputeMatricesSystem))]
public class BoneRendererSystemGroup : ComponentSystemGroup
{
}

[UpdateInGroup(typeof(BoneRendererSystemGroup))]
public class BoneRendererMatrixSystem : BoneRendererMatrixSystemBase
{
}

[UpdateInGroup(typeof(BoneRendererSystemGroup))]
[UpdateAfter(typeof(BoneRendererMatrixSystem))]
public class BoneRendererRenderingSystem : BoneRendererRenderingSystemBase
{
}