using SESH.Models;
using SESH.Models.Enums;

namespace SESH.Services.Interfaces
{
    public class ReportResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public WellBeingReport? Report { get; set; }

        public static ReportResult SuccessResult(WellBeingReport report)
            => new() { Success = true, Report = report };

        public static ReportResult FailureResult(string error)
            => new() { Success = false, ErrorMessage = error };
    }

    public interface IReportService
    {
        Task<ReportResult> SubmitReportAsync(int studentId, ReportStatus status, string notes);
        Task<List<WellBeingReport>> GetStudentReportsAsync(int studentId);
        Task<List<WellBeingReport>> GetReportsForSupervisorAsync(int supervisorId);
        Task<bool> CanSubmitReportAsync(int studentId);
    }
}