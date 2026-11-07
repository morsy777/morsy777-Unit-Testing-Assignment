using DriveNow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Repositories;

public interface IPromoCodeRepository
{
    PromoCode? GetByCode(string code);
    public void Update(PromoCode promo);
}
