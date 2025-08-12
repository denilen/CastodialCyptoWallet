using CryptoWallet.Domain.Currencies;

namespace CryptoWallet.Infrastructure.Persistence.Seeding;

/// <summary>
/// Data initial for fiat currencies
/// </summary>
public class FiatCurrencySeeder : BaseSeeder
{
    public override async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await ShouldSeedAsync(context.FiatCurrencies, cancellationToken))
        {
            return;
        }

        var fiatCurrencies = new List<FiatCurrency>
        {
            new("USD", "US Dollar", "$", decimalPlaces: 2),
            new("EUR", "Euro", "€", decimalPlaces: 2),
            new("RUB", "Russian Ruble", "₽", decimalPlaces: 2)
        };

        await context.FiatCurrencies.AddRangeAsync(fiatCurrencies, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
