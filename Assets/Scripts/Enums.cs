public enum Side { Start, End, Both }
public enum Direction { In, Out, Both }
public enum Orientation { Left, Right }
public enum BuildMode { Ghost, Actual }
public enum RoadRemovalOption { Default, Divide, Combine, Replace }
public enum CarStatus { Traveling, Canceled, Finished }
public enum RoadProperty { PlayerBuilt, InnateSource, InnateTarget };
public enum HUDLayer
{
    Intersections,
    SupportLines,
    SnapPoints,
    RoadArrows,
    BulkSelector
}
