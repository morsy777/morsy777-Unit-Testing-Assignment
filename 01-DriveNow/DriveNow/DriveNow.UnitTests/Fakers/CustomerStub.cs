using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Repositories;

namespace DriveNow.UnitTests.Fakers;

public class CustomerStub : ICustomerRepository
{
    public List<Customer> GetAll()
    {
        return [];
    }

    public Customer? GetById(int id)
    {
        return null;
    }

    public void Update(Customer customer)
    {
        
    }
}
