public static class Constants
{
    public const float LaneWidth = 5f;
    public const float BuildSnapTolerance = LaneWidth / 5;
    public const float MinimumLaneLength = LaneWidth * 2;
    public const float MaximumLaneLength = LaneWidth * 20;
    public const float RoadDivisionLengthTestTolerance = 5f;
    public const float MeshResolution = LaneWidth / 8;
    public const float LaneSplineResolution = LaneWidth / 5;
    public const float RoadOutlineSeparation = LaneWidth / 2;
    public const float VertexDistanceFromRoadEnds = MinimumLaneLength / 2;
    public const float NumericallyEqualMaxTolerance = 0.01f;
    public const float MaxRoadBendAngle = 130;
    public const int GhostRoadId = -1;
    public const float ElevationOffset = 32;
}