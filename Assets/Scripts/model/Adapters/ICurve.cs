public interface ICurve
{
    float StartInterpolation { get; set; }
    float EndInterpolation { get; set; }

    void Draw(float duration);
}