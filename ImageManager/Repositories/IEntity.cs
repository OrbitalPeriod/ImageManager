namespace ImageManager.Repositories;

public interface IEntity<TKey>
{
    TKey Id { get; }
}