public static class Constants
{
    public const float LaneWidth = 5f;
    public const float BuildSnapTolerance = LaneWidth / 2.5f;
    public const float MinimumLaneLength = LaneWidth * 3;
    public const float MaximumLaneLength = LaneWidth * 20;
    public const float RoadDivisionLengthTestTolerance = 5f;
    public const float MeshResolution = LaneWidth / 8;
    public const float LaneSplineResolution = LaneWidth / 6.25f;
    public const float RoadOutlineSeparation = LaneWidth / 2;
    public const float VertexDistanceFromRoadEnds = MinimumLaneLength / 2;
    public const float NumericallyEqualMaxTolerance = 0.01f;
    public const float MaxRoadBendAngle = 130;
    public const int GhostRoadId = -1;
    public const float ElevationOffset = 33;
    public const float ZoneResolution = 0.17f;
    public const int MaxElevation = 30;
    public const float CarAcceleration = 15;
    public const float CarDeceleration = 15f;
    public const float CarMaxSpeed = 20f;
    public const float CarMinSpeed = 0f;
    public const float PathBlockDuration = 0.5f;
    public const float CarMinimumSeparation = 6f;
    public const int ZoneDemandCap = 20;
    public const int MaxVertexWaitingCar = 5;
    public const float ZoneDemandSatisfyCooldown = 10;
}