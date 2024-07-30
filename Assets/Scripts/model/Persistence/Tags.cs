using System;

public sealed class NotSavedAttribute : Attribute { }
public sealed class SaveIDAttribute : Attribute { }
public sealed class SaveIDCollectionAttribute : Attribute
{
    // public Type CollectionType { get; }
    // public SaveIDCollectionAttribute(Type collectionType)
    // {
    //     CollectionType = collectionType;
    // }
}
public sealed class SortedListSpecificAttribute : Attribute { }