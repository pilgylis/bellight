namespace Bellight.AutoMapper;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MappedModelAttribute : Attribute
{
    public Type TargetType { get; }

    public MappedModelAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}