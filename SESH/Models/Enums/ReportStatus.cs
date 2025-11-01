namespace SESH.Models.Enums
{
    /// <summary>
    /// Represents a students reported status
    /// Values are ordered from best to worst
    /// can be used to drive UI, notifications and triage
    /// </summary>
    public enum ReportStatus
    {
        Thriving,
        Okay,
        Struggling,
        InCrisis
    }
}