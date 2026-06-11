using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace TMG.AnimationCurves
{
    public class AccelerationCurveAuthoring : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _accelerationCurve;
        [SerializeField] private int _numberOfSamples;
            
        class Baker : Baker<AccelerationCurveAuthoring>
        {
            public override void Bake(AccelerationCurveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AccelerationCurveReference accelerationCurveReference = new AccelerationCurveReference();

                BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
                ref SampledCurve sampledCurve = ref blobBuilder.ConstructRoot<SampledCurve>();
                sampledCurve.NumberOfSamples = authoring._numberOfSamples;

                BlobBuilderArray<float> sampledCurveArray = blobBuilder.Allocate<float>(
                    ref sampledCurve.SampledPoints, authoring._numberOfSamples);
                
                for (var i = 0; i < authoring._numberOfSamples; i++)
                {
                    var samplePoint = (float)i / (authoring._numberOfSamples - 1);
                    var sampleValue = authoring._accelerationCurve.Evaluate(samplePoint);
                    sampledCurveArray[i] = sampleValue;
                }
                accelerationCurveReference.Value = blobBuilder.CreateBlobAssetReference<SampledCurve>(Allocator.Persistent);
                blobBuilder.Dispose();
                AddBlobAsset(ref accelerationCurveReference.Value, out Unity.Entities.Hash128 hash);
                AddComponent(entity,  accelerationCurveReference);
            }
        }
    }
    public struct AccelerationCurveReference : IComponentData
    {
        public BlobAssetReference<SampledCurve> Value;
        public readonly float GetValueAtTime(float time) => Value.Value.GetValueAtTime(time);
    }
    public struct SampledCurve
    {
        public BlobArray<float> SampledPoints;
        public int NumberOfSamples;
        
        public float GetValueAtTime(float time)
        {
            time = math.clamp(time, 0f, 1f);  
            var approxSampleIndex = (NumberOfSamples - 1) * time;
            var sampleIndexBelow = (int)math.floor(approxSampleIndex);
            if (sampleIndexBelow >= NumberOfSamples - 1)
            {
                return SampledPoints[NumberOfSamples - 1];
            }
            var indexRemainder = approxSampleIndex - sampleIndexBelow;
            return math.lerp(SampledPoints[sampleIndexBelow], SampledPoints[sampleIndexBelow + 1], indexRemainder);
        }
    }
}