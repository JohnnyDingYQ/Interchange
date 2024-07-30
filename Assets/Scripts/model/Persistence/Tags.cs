using System;

public sealed class NotSavedAttribute : Attribute { }
public sealed class SaveIDAttribute : Attribute { }
public sealed class SaveIDCollectionAttribute : Attribute { }
public sealed class IPersistableImplementedAttribute : Attribute { }
public sealed class IPersistableDictAttribute : Attribute
{
    public Type ItemType { get; }
    public IPersistableDictAttribute(Type itemType)
    {
        ItemType = itemType;
    }
}