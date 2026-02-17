namespace ServiceName.Domain.Entities;

public abstract class BaseEntity
{
    // Guid can be changed to other datatypes, depending on business requirements
    public Guid Id { get; set; }
}
