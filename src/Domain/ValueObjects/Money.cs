namespace SellerInventory.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero => new(0);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }
}
