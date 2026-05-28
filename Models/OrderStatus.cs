namespace Vkeram.Backend.Models;

public enum ConfirmationStatus
{
    Confirmed,
    Unconfirmed,
    Cancelled
}

public enum PaymentStatus
{
    Paid,
    PartiallyPaid,
    Unpaid
}

public enum ShipmentStatus
{
    Shipped,
    PartiallyShipped,
    Unshipped
}
