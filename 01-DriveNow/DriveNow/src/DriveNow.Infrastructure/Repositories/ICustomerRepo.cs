using DriveNow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Repositories;

public interface ICustomerRepo
{
    Customer? GetById(int id);
    List<Customer> GetAll();
    void Update(Customer customer);
}
