using Unity.Mathematics;

public interface ICurve
{
    float StartT { get; set; }
    float EndT { get; set; }
    CurveType CurveType { get; set; }

    void Draw(float duration);
    float3 EvaluatePosition(float t);
    float3 Evaluate2DNormal(float t);
    void RestoreFromDeserialization();
}