using Unity.Mathematics;

public class DataInputImpl : IDataInputBoundary
{
    public float3 GetCursorPos()
    {
        return GameWrapper.MouseWorldPos;
    }
}