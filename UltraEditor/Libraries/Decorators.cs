using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public static class AttributeHelper
{
    public static IEnumerable<(Type type, T attr)> GetTypesWithAttribute<T>() where T : Attribute =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => (type: t, attr: t.GetCustomAttribute<T>()))
            .Where(x => x.attr != null);

    public static IEnumerable<(FieldInfo field, T attr)> GetFieldsWithAttribute<T>() where T : Attribute =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static))
            .Select(f => (field: f, attr: f.GetCustomAttribute<T>()))
            .Where(x => x.attr != null);
}

[AttributeUsage(AttributeTargets.Class)]
public class EditorComp : Attribute
{
    public string description;

    public EditorComp(string description) => this.description = description;
}

[AttributeUsage(AttributeTargets.Field)]
public class VarDescription : Attribute
{
    public string description;

    public VarDescription(string description) => this.description = description;
}

[AttributeUsage(AttributeTargets.Field)]
public class EditorVar: Attribute
{
    public string display;

    public EditorVar(string description) => this.display = description;
}