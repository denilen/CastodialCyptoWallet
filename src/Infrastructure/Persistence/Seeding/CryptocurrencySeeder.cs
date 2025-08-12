using CryptoWallet.Domain.Currencies;

namespace CryptoWallet.Infrastructure.Persistence.Seeding;

/// <summary>
/// Data initial for cryptocurrencies
/// </summary>
public class CryptocurrencySeeder : BaseSeeder
{
    public override async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await ShouldSeedAsync(context.Cryptocurrencies, cancellationToken))
        {
            return;
        }

        var cryptocurrencies = new List<Cryptocurrency>
        {
            new("BTC", "Bitcoin", decimalPlaces: 8),
            new("ETH", "Ethereum", decimalPlaces: 18),
            new("USDT", "Tether", decimalPlaces: 6)
        };

        await context.Cryptocurrencies.AddRangeAsync(cryptocurrencies, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
