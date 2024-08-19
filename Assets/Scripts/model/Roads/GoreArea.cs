using System;

public class GoreArea
{
    public Road Left;
    public Road Right;
    public Side Side;

    public GoreArea(Road left, Road right, Side side)
    {
        Left = left;
        Right = right;
        Side = side;
    }

    public override bool Equals(object obj)
    {
        if (obj is GoreArea other)
            return Equals(Left, other.Left) && Equals(Right, other.Right) && Equals(Side, other.Side);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Left, Right, Side);
    }
}