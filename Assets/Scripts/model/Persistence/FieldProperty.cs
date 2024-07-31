using System;
using System.Collections.Generic;
using System.Reflection;

public class FieldProperty
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

    public object GetValue(object target)
    {
        if (isField)
            return fieldInfo.GetValue(target);
        return propertyInfo.GetValue(target);
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

    public Type GetGenericCollectionItemType(int offset = 0)
    {
        if (Type().IsGenericType)
        {
            Type[] typeArguments = Type().GetGenericArguments();
            return typeArguments[offset];
        }
        throw new ArgumentException("The provided type is not a generic collection");
    }

    public static List<FieldProperty> GetFieldProperties(Type type, object self)
    {
        PropertyInfo[] properties = type.GetProperties();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        List<FieldProperty> fieldProperties = new();
        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<NotSavedAttribute>() != null || field.Name.Contains("k__BackingField"))
                continue;
            fieldProperties.Add(new(field, self));
        }
        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<NotSavedAttribute>() != null)
                continue;
            fieldProperties.Add(new(prop, self));
        }
        return fieldProperties;
    }
}