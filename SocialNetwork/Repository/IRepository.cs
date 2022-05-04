namespace SocialNetwork.Repository;

public interface IRepository<TItem, TId>
{
    public IEnumerable<TItem> GetAll();
    public TItem Get(TId id);
    public void Add(TItem item);
    public void Update(TItem item);
    public void Delete(TId id);
}