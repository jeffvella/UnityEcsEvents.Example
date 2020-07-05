using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using AnimationCurve = UnityEngine.AnimationCurve;

[RequiresEntityConversion]
public class AnimateTranslationSample : MonoBehaviour, IConvertGameObjectToEntity
{
    public AnimationCurve TranslationCurve;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new AnimateTranslation {TranslationCurve = TranslationCurve.ToAnimationCurveBlobAssetRef()});
    }
}

internal struct AnimateTranslation : IComponentData
{
    public BlobAssetReference<AnimationCurveBlob> TranslationCurve;
}

public class AnimationCurveBlobTranslationSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var time = Mathf.Repeat((float) Time.ElapsedTime, 1.0F);

        return Entities.ForEach((ref Translation position, in AnimateTranslation translation) =>
        {
            var value = AnimationCurveEvaluator.Evaluate(time, translation.TranslationCurve);
            position.Value.y = value;
        }).Schedule(inputDeps);
    }
}