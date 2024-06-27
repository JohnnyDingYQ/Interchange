public static class Constants
{
    public const float LaneWidth = 2f;
    public const float BuildSnapTolerance = LaneWidth / 2.5f;
    public const float MinimumLaneLength = LaneWidth * 3;
    public const float MaximumLaneLength = LaneWidth * 20;
    public const float MeshResolution = 3 / LaneWidth;
    public const float RoadOutlineSeparation = LaneWidth / 2;
    public const float VertexDistanceFromRoadEnds = MinimumLaneLength / 2; 
    public const float MaxRoadBendAngle = 130;
    public const int MaxElevation = 10;
    public const int MinElevation = 0;
    public const float CarAcceleration = 15;
    public const float CarDeceleration = 15f;
    public const float CarMaxSpeed = 20f;
    public const float CarMinSpeed = 0f;
    public const float CarMinimumSeparation = MinimumLaneLength / 1.2f;
    public const int DestinationCap = 20;
    public const int MaxVertexWaitingCar = 5;
    public const float ZoneDemandSatisfyCooldownSpeed = 20;
    public const float DefaultParallelSpacing = LaneWidth * 4.5f;
}