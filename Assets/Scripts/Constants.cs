public static class Constants
{
    public const float LaneWidth = 2f;
    public const float BuildSnapTolerance = LaneWidth / 2.5f;
    public const float MinLaneLength = LaneWidth * 5;
    public const float MaxLaneLength = LaneWidth * 25;
    public const float MinSegmentRatio = 0.2f;
    public const float MeshResolution = 10 / LaneWidth;
    public const float RoadOutlineSeparation = LaneWidth / 2;
    public const float VertexDistanceFromRoadEnds = MinLaneLength / 2.1f; 
    public const float RoadArrowSeparation = LaneWidth * 7f;
    public const float MaxRoadBendAngle = 130;
    public const int MaxElevation = 10;
    public const int MinElevation = 0;
    public const int ElevationStep = 2;
    public const float CarAcceleration = 15;
    public const float CarDeceleration = 15f;
    public const float CarMaxSpeed = 20f;
    public const float CarMinSpeed = 0f;
    public const float CarMinimumSeparation = MinLaneLength / 1.2f;
    public const int DestinationCap = 20;
    public const int MaxVertexWaitingCar = 5;
    public const float ZoneDemandSatisfyCooldownSpeed = 20;
    public const float DefaultParallelSpacing = LaneWidth * 4.5f;
}