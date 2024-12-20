using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public interface IPersistable
{
    public uint Id { get; set; }
    public virtual void Save(Writer writer)
    {
        foreach (FieldProperty fieldProperty in FieldProperty.GetFieldProperties(GetType(), this))
        {
            if (fieldProperty.GetCustomAttribute<SaveIDAttribute>() != null)
            {
                SaveId(writer, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                SaveIdCollection(writer, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<IPersistableImplementedAttribute>() != null)
            {
                SaveIPersistable(writer, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
            {
                SaveIPersistableDict(writer, fieldProperty);
                continue;
            }

            dynamic value = fieldProperty.GetValue();
            writer.Write(value);
        }

        static void SaveId(Writer writer, FieldProperty fieldProperty)
        {
            if (fieldProperty.GetValue() == null)
            {
                writer.Write(0);
                return;
            }
            if (fieldProperty.GetValue() is IPersistable persistable)
                writer.Write(persistable.Id);
            else
                throw new InvalidOperationException($"{fieldProperty.Name} does not implement IPersistable");
        }

        static void SaveIPersistable(Writer writer, FieldProperty fieldProperty)
        {
            if (fieldProperty.GetValue() is IPersistable persistable)
                persistable.Save(writer);
            else
                throw new InvalidOperationException($"{fieldProperty.Name} does not implement IPersistable");
        }

        static void SaveIdCollection(Writer writer, FieldProperty fieldProperty)
        {
            if (fieldProperty.GetValue() is IEnumerable<IPersistable> collection)
            {
                writer.Write(collection.Count());
                foreach (IPersistable persistable in collection)
                    writer.Write(persistable.Id);
            }
            else
                throw new InvalidOperationException($"{fieldProperty.Name} is either null or not collection of IPersistable");
        }

        void SaveIPersistableDict(Writer writer, FieldProperty fieldProperty)
        {
            Type type = fieldProperty.Type();
            Type itemType = GetGenericCollectionItemType(type, 1);
            if (itemType.GetInterface("IPersistable") == null)
                throw new InvalidOperationException($"{fieldProperty.Name} item does not implement IPersistable");
            dynamic collection = fieldProperty.GetValue();
            writer.Write(collection.Count);
            foreach (IPersistable item in collection.Values)
            {
                writer.Write(item.Id);
                item.Save(writer);
            }
        }
    }
    public virtual void Load(Reader reader)
    {
        List<FieldProperty> fieldProperties = FieldProperty.GetFieldProperties(GetType(), this);
        foreach (FieldProperty fieldProperty in fieldProperties)
            if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
                reader.Lut[GetGenericCollectionItemType(fieldProperty.Type(), 1)] = new();

        foreach (FieldProperty fieldProperty in fieldProperties)
        {
            if (fieldProperty.GetCustomAttribute<SaveIDAttribute>() != null)
            {
                LoadID(reader, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                LoadIDCollection(reader, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<IPersistableImplementedAttribute>() != null)
            {
                LoadIPersistable(reader, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
            {
                LoadIPersistableDict(reader, fieldProperty);
                continue;
            }

            var readMethod = typeof(Reader).GetMethod("Read");
            var genericReadMethod = readMethod.MakeGenericMethod(fieldProperty.Type());
            var readValue = genericReadMethod.Invoke(reader, null);
            fieldProperty.SetValue(readValue);
        }

        void LoadIDCollection(Reader reader, FieldProperty fieldProperty)
        {
            int count = reader.ReadInt();
            Type type = fieldProperty.Type();
            if (!type.IsGenericType)
                throw new InvalidOperationException("Property is not generic type");
            MethodInfo addMethod = type.GetMethod("Add") ?? throw new InvalidOperationException("Property does not support Add operation");
            Type itemType = GetGenericCollectionItemType(type);
            object collection = Activator.CreateInstance(type);

            for (int i = 0; i < count; i++)
            {
                uint id = reader.ReadUint();
                IPersistable item = reader.CreateInstance(itemType, id);
                addMethod.Invoke(collection, new object[] { item });
            }

            fieldProperty.SetValue(collection);
        }

        static void LoadIPersistable(Reader reader, FieldProperty fieldProperty)
        {
            IPersistable restored = (IPersistable) Activator.CreateInstance(fieldProperty.Type());
            restored.Load(reader);
            fieldProperty.SetValue(restored);
        }

        void LoadID(Reader reader, FieldProperty fieldProperty)
        {
            uint id = reader.ReadUint();
            if (id == 0)
                fieldProperty.SetValue(null);
            else
            {
                IPersistable obj = reader.CreateInstance(fieldProperty.Type(), id);
                fieldProperty.SetValue(obj);
            }
        }

        void LoadIPersistableDict(Reader reader, FieldProperty fieldProperty)
        {
            Type itemType = GetGenericCollectionItemType(fieldProperty.Type(), 1);
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                IPersistable restored = reader.CreateInstance(itemType, reader.ReadUint());
                restored.Load(reader);
            }
            object collection = Activator.CreateInstance(fieldProperty.Type());
            MethodInfo addMethod = fieldProperty.Type().GetMethod("Add")
                ?? throw new InvalidOperationException("Property does not support Add operation");
            foreach (object item in reader.Lut[itemType].Values)
            {
                addMethod.Invoke(collection, new object[] { ((IPersistable)item).Id, item });
            }
            fieldProperty.SetValue(collection);
        }
    }

    public static bool Equals(IPersistable a, IPersistable b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;
        if (a.GetType() != b.GetType())
            return false;
        return a.Id == b.Id;
    }

    Type GetGenericCollectionItemType(Type collectionType, int offset = 0)
    {
        if (collectionType.IsGenericType)
        {
            Type[] typeArguments = collectionType.GetGenericArguments();
            return typeArguments[offset];
        }
        throw new ArgumentException("The provided type is not a generic collection");
    }

}