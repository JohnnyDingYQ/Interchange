using System.Collections.Generic;

public class Path
{
    private List<ICurve> curves;
    public List<ICurve> Curves
    {
        get
        {
            curves ??= new();
            return curves;
        }
        set
        {
            curves = value;
        }
    }

    public Path() {}
}