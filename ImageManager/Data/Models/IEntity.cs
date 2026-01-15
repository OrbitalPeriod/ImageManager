namespace ImageManager.Data.Models;

public interface IEntity<TKey>
{
    TKey Id { get; }
}

public interface IUserOwnedEntity
{
    string UserId { get; }
}
