using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
public interface IPersistable
{
    public uint Id { get; set; }
    public virtual void Save(Writer writer)
    {
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {

            if (field.GetCustomAttribute<NotSavedAttribute>() != null || field.Name.Contains("k__BackingField"))
                continue;
            if (field.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                IEnumerable<IPersistable> referenced = (IEnumerable<IPersistable>)field.GetValue(this);
                if (referenced == null)
                    throw new InvalidOperationException($"Property collection {field.Name} is null");
                writer.Write(referenced.Count());
                foreach (IPersistable item in referenced)
                    writer.Write(item.Id);
                continue;
            }
            if (field.GetCustomAttribute<SortedListSpecificAttribute>() != null)
            {
                SortedList<int, Node> referenced = (SortedList<int, Node>) field.GetValue(this);
                if (referenced == null)
                    throw new InvalidOperationException($"Property collection {field.Name} is null");
                writer.Write(referenced.Count());
                foreach (KeyValuePair<int, Node> item in referenced)
                {
                    writer.Write(item.Key);
                    writer.Write(item.Value.Id);
                }
                continue;
            }
            dynamic value = field.GetValue(this);
            writer.Write(value);
        }

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<NotSavedAttribute>() != null)
                continue;
            if (prop.GetCustomAttribute<SaveIDAttribute>() != null)
            {
                object referenced = prop.GetValue(this);
                if (referenced == null)
                    writer.Write(0);
                else
                    writer.Write(((IPersistable)prop.GetValue(this)).Id);
                continue;
            }
            if (prop.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                IEnumerable<IPersistable> referenced = (IEnumerable<IPersistable>)prop.GetValue(this);
                if (referenced == null)
                    throw new InvalidOperationException($"Property collection {prop.Name} is null");
                writer.Write(referenced.Count());
                foreach (IPersistable item in referenced)
                    writer.Write(item.Id);
                continue;
            }
            if (prop.GetCustomAttribute<SortedListSpecificAttribute>() != null)
            {
                IEnumerable<KeyValuePair<int, IPersistable>> referenced = (IEnumerable<KeyValuePair<int, IPersistable>>) prop.GetValue(this);
                if (referenced == null)
                    throw new InvalidOperationException($"Property collection {prop.Name} is null");
                writer.Write(referenced.Count());
                foreach (KeyValuePair<int, IPersistable> item in referenced)
                {
                    writer.Write(item.Key);
                    writer.Write(item.Value.Id);
                }
                continue;
            }
            dynamic value = prop.GetValue(this);
            writer.Write(value);
        }
    }
    public virtual void Load(Reader reader)
    {
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {

            if (field.GetCustomAttribute<NotSavedAttribute>() != null || field.Name.Contains("k__BackingField"))
                continue;
            if (field.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                int count = reader.ReadInt();
                if (!field.FieldType.IsGenericType)
                    throw new InvalidOperationException("Property is not generic type");
                MethodInfo addMethod = field.FieldType.GetMethod("Add") ?? throw new InvalidOperationException("Property does not support Add operation");
                Type itemType = GetGenericCollectionItemType(field.FieldType);
                object collection = Activator.CreateInstance(field.FieldType);
                
                for (int i = 0; i < count; i++)
                {
                    IPersistable item = (IPersistable) Activator.CreateInstance(itemType);
                    item.Id = reader.ReadUint();
                    addMethod.Invoke(collection, new object[] { item });
                }

                field.SetValue(this, collection);
                continue;
            }
            if (field.GetCustomAttribute<SortedListSpecificAttribute>() != null)
            {
                int count = reader.ReadInt();
                if (!field.FieldType.IsGenericType)
                    throw new InvalidOperationException("Property is not generic type");
                MethodInfo addMethod = field.FieldType.GetMethod("Add") ?? throw new InvalidOperationException("Property does not support Add operation");
                Type itemType = GetGenericCollectionItemType(field.FieldType, 1);
                object collection = Activator.CreateInstance(field.FieldType);
                
                for (int i = 0; i < count; i++)
                {
                    int key = reader.ReadInt();
                    IPersistable item = (IPersistable) Activator.CreateInstance(itemType);
                    item.Id = reader.ReadUint();
                    addMethod.Invoke(collection, new object[] { key, item });
                }

                field.SetValue(this, collection);
                continue;
            }

            var readMethod = typeof(Reader).GetMethod("Read");
            var genericReadMethod = readMethod.MakeGenericMethod(field.FieldType);
            var readValue = genericReadMethod.Invoke(reader, null);
            field.SetValue(this, readValue);
        }

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<NotSavedAttribute>() != null)
                continue;
            if (prop.GetCustomAttribute<SaveIDAttribute>() != null)
            {
                uint id = reader.ReadUint();
                if (id == 0)
                    prop.SetValue(this, null);
                else
                {
                    IPersistable obj = (IPersistable)Activator.CreateInstance(prop.PropertyType);
                    obj.Id = id;
                    prop.SetValue(this, obj);
                }
                continue;
            }
            if (prop.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                int count = reader.ReadInt();
                if (!prop.PropertyType.IsGenericType)
                    throw new InvalidOperationException("Property is not generic type");
                MethodInfo addMethod = prop.PropertyType.GetMethod("Add") ?? throw new InvalidOperationException("Property does not support Add operation");
                Type itemType = GetGenericCollectionItemType(prop.PropertyType);
                object collection = Activator.CreateInstance(prop.PropertyType);
                
                for (int i = 0; i < count; i++)
                {
                    IPersistable item = (IPersistable) Activator.CreateInstance(itemType);
                    item.Id = reader.ReadUint();
                    addMethod.Invoke(collection, new object[] { item });
                }

                prop.SetValue(this, collection);
                continue;
            }
            if (prop.GetCustomAttribute<SortedListSpecificAttribute>() != null)
            {
                int count = reader.ReadInt();
                if (!prop.PropertyType.IsGenericType)
                    throw new InvalidOperationException("Property is not generic type");
                MethodInfo addMethod = prop.PropertyType.GetMethod("Add") ?? throw new InvalidOperationException("Property does not support Add operation");
                Type itemType = GetGenericCollectionItemType(prop.PropertyType);
                object collection = Activator.CreateInstance(prop.PropertyType);
                
                for (int i = 0; i < count; i++)
                {
                    int key = reader.ReadInt();
                    IPersistable item = (IPersistable) Activator.CreateInstance(itemType);
                    item.Id = reader.ReadUint();
                    addMethod.Invoke(collection, new object[] { key, item });
                }

                prop.SetValue(this, collection);
                continue;
            }
            var readMethod = typeof(Reader).GetMethod("Read");
            var genericReadMethod = readMethod.MakeGenericMethod(prop.PropertyType);
            var readValue = genericReadMethod.Invoke(reader, null);
            prop.SetValue(this, readValue);
        }

        static Type GetGenericCollectionItemType(Type collectionType, int offset = 0)
        {
            if (collectionType.IsGenericType)
            {
                Type[] typeArguments = collectionType.GetGenericArguments();
                return typeArguments[offset];
            }
            throw new ArgumentException("The provided type is not a generic collection with a single type argument.");
        }
    }

    public static bool Equals(IPersistable a, IPersistable b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;
        return a.Id == b.Id;
    }
}