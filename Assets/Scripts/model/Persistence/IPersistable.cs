public interface IPersistable
{
    public uint Id { get; set; }
    public void Save(Writer writer);
    public void Load(Reader reader);
    public static bool Equals(IPersistable a, IPersistable b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;
        return a.Id == b.Id;
    }
}