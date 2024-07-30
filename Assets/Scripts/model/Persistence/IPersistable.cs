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
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        List<FieldProperty> fieldProperties = new();
        foreach (FieldInfo field in fields)
            fieldProperties.Add(new(field, this));
        foreach (PropertyInfo prop in properties)
            fieldProperties.Add(new(prop, this));

        foreach (FieldProperty fieldProperty in fieldProperties)
        {
            if (fieldProperty.GetCustomAttribute<NotSavedAttribute>() != null || fieldProperty.Name.Contains("k__BackingField"))
                continue;
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
            dynamic value = fieldProperty.GetValue();
            writer.Write(value);
        }

        static void SaveId(Writer writer, FieldProperty fieldProperty)
        {
            object referenced = fieldProperty.GetValue();
            if (referenced == null)
                writer.Write(0);
            else
                writer.Write(((IPersistable)fieldProperty.GetValue()).Id);
        }

        static void SaveIPersistable(Writer writer, FieldProperty fieldProperty)
        {
            object val = fieldProperty.GetValue();
            MethodInfo saveMethod = fieldProperty.Type().GetMethod("Save")
                ?? throw new InvalidOperationException("Type does not support Save method");
            saveMethod.Invoke(val, new object[] { writer });
        }

        static void SaveIdCollection(Writer writer, FieldProperty fieldProperty)
        {
            IEnumerable<IPersistable> referenced = (IEnumerable<IPersistable>)fieldProperty.GetValue();
            if (referenced == null)
                throw new InvalidOperationException($"Property collection {fieldProperty.Name} is null");
            writer.Write(referenced.Count());
            foreach (IPersistable item in referenced)
                writer.Write(item.Id);
        }

    }
    public virtual void Load(Reader reader)
    {
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        List<FieldProperty> fieldProperties = new();
        foreach (FieldInfo field in fields)
            fieldProperties.Add(new(field, this));
        foreach (PropertyInfo prop in properties)
            fieldProperties.Add(new(prop, this));

        foreach (FieldProperty fieldProperty in fieldProperties)
        {
            if (fieldProperty.GetCustomAttribute<NotSavedAttribute>() != null || fieldProperty.Name.Contains("k__BackingField"))
                continue;
            if (fieldProperty.GetCustomAttribute<SaveIDAttribute>() != null)
            {
                uint id = reader.ReadUint();
                if (id == 0)
                    fieldProperty.SetValue(null);
                else
                {
                    IPersistable obj = (IPersistable)Activator.CreateInstance(fieldProperty.Type());
                    obj.Id = id;
                    fieldProperty.SetValue(obj);
                }
                continue;
            }
            if (fieldProperty.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
            {
                SaveIDCollection(reader, fieldProperty);
                continue;
            }
            if (fieldProperty.GetCustomAttribute<IPersistableImplementedAttribute>() != null)
            {
                LoadIPersistable(reader, fieldProperty);
                continue;
            }

            var readMethod = typeof(Reader).GetMethod("Read");
            var genericReadMethod = readMethod.MakeGenericMethod(fieldProperty.Type());
            var readValue = genericReadMethod.Invoke(reader, null);
            fieldProperty.SetValue(readValue);
        }

        static Type GetGenericCollectionItemType(Type collectionType, int offset = 0)
        {
            if (collectionType.IsGenericType)
            {
                Type[] typeArguments = collectionType.GetGenericArguments();
                return typeArguments[offset];
            }
            throw new ArgumentException("The provided type is not a generic collection");
        }

        static void SaveIDCollection(Reader reader, FieldProperty fieldProperty)
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
                IPersistable item = (IPersistable)Activator.CreateInstance(itemType);
                item.Id = reader.ReadUint();
                addMethod.Invoke(collection, new object[] { item });
            }

            fieldProperty.SetValue(collection);
        }

        static void LoadIPersistable(Reader reader, FieldProperty fieldProperty)
        {
            object restored = Activator.CreateInstance(fieldProperty.Type());
            MethodInfo loadMethod = fieldProperty.Type().GetMethod("Load")
                ?? throw new InvalidOperationException("Type does not support Load method");
            loadMethod.Invoke(restored, new object[] { reader });
            fieldProperty.SetValue(restored);
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

    private class FieldProperty
    {
        readonly FieldInfo fieldInfo;
        readonly PropertyInfo propertyInfo;
        readonly object obj;
        readonly bool isField;
        public string Name { get => isField ? fieldInfo.Name : propertyInfo.Name; }
        public FieldProperty(PropertyInfo p, object s)
        {
            propertyInfo = p;
            obj = s;
            isField = false;
        }

        public FieldProperty(FieldInfo f, object s)
        {
            fieldInfo = f;
            obj = s;
            isField = true;
        }

        public void SetValue(object value)
        {
            if (isField)
                fieldInfo.SetValue(obj, value);
            else
                propertyInfo.SetValue(obj, value);
        }

        public object GetValue()
        {
            if (isField)
                return fieldInfo.GetValue(obj);
            return propertyInfo.GetValue(obj);
        }

        public Type Type()
        {
            if (isField)
                return fieldInfo.FieldType;
            return propertyInfo.PropertyType;
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            if (isField)
                return fieldInfo.GetCustomAttribute<T>();
            return propertyInfo.GetCustomAttribute<T>();
        }
    }
}