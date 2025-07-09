namespace Bellight.DataManagement;

public interface IEntity<IdType> where IdType: IEquatable<IdType>
{
    IdType Id { get; set; }
    bool IsDeleted { get; set; }
}