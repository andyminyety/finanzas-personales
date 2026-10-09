namespace FinanzasApi.Repositories;

public interface ICounterRepository
{
    Task<long> NextAsync(string name);
}
