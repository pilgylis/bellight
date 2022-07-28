namespace Bellight.DataManagement;
public interface IEntity
{ 
    bool IsDeleted { get; set; }
}

public interface IEntity<IdType>: IEntity
{
    IdType? Id { get; set; }
}
