public static class Progression
{
    public static void CheckProgression()
    {
        District prev = null;
        foreach (District district in Game.Districts.Values)
        {
            if (!district.Enabled)
                if (prev.Connectedness > 20)
                    district.Enable();
            prev = district;
        }
    }
}