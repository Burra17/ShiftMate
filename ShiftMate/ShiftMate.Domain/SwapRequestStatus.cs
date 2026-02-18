namespace ShiftMate.Domain
{
    // Möjliga statusar för en bytesförfrågan
    public enum SwapRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        Cancelled
    }
}
