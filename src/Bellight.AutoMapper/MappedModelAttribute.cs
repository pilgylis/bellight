namespace Bellight.AutoMapper;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MappedModelAttribute(Type targetType) : Attribute
{
    public Type TargetType { get; } = targetType;
}