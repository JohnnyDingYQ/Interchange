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
    public const uint GhostRoadId = uint.MaxValue;
    public const float ZoneHeight = -2.1f;
    public const int MaxElevation = 10;
    public const int MinElevation = -2;
    public const float CarAcceleration = 15;
    public const float CarDeceleration = 15f;
    public const float CarMaxSpeed = 20f;
    public const float CarMinSpeed = 0f;
    public const float CarMinimumSeparation = MinimumLaneLength / 5;
    public const int ZoneDemandCap = 20;
    public const int MaxVertexWaitingCar = 5;
    public const float ZoneDemandSatisfyCooldownSpeed = 20;
}