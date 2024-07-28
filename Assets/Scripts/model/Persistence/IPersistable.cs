public interface IPersistable
{
    public void Save(Writer writer);
    public void Load(Reader reader);
}