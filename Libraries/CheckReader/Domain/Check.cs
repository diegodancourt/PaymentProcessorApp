using System.Globalization;

namespace CheckReader.Domain
{
    public class Check
    {
        public required Amount Amount { get; init; }
        public required Payee Payee { get; init; }
        public required Micr Micr { get; init; }
        public DateOnly? Date { get; init; }
        public string? RawText { get; init; }

        public override string ToString()
        {
            return $"""

                    Check Details:
                    --------------
                    Check #: {Micr.CheckNumber ?? "Not found"}
                    Pay To: {Payee.Name ?? "Not found"}
                    Amount: {Amount.Value.ToString(CultureInfo.InvariantCulture) ?? "Not found"}
                    Date: {Date?.ToString("O") ?? "Not found"}
                    Routing #: {Micr.RoutingNumber ?? "Not found"}
                    Account #: {Micr.AccountNumber ?? "Not found"}

                    """;
        }
    }

    // Value Objects
    public record Amount(decimal Value, string Currency = "USD");
    public record Micr(string RoutingNumber, string AccountNumber, string CheckNumber);
    public record Payee(string Name);
}