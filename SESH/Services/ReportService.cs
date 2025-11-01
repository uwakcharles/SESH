using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Models.Enums;
using SESH.Services.Interfaces;

namespace SESH.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportResult> SubmitReportAsync(int studentId, ReportStatus status, string notes)
        {
            if (!await CanSubmitReportAsync(studentId))
                return ReportResult.FailureResult("You can only submit one well-being report per week.");

            if (notes.Length > 500)
                return ReportResult.FailureResult("Notes cannot exceed 500 characters.");

            var report = new WellBeingReport
            {
                StudentId = studentId,
                Status = status,
                Notes = notes,
                SubmittedAt = DateTime.UtcNow
            };

            _context.WellBeingReports.Add(report);
            await _context.SaveChangesAsync();

            if (status == ReportStatus.Struggling || status == ReportStatus.InCrisis)
                await FlagReportForSupervisor(report);

            return ReportResult.SuccessResult(report);
        }

        public async Task<List<WellBeingReport>> GetStudentReportsAsync(int studentId)
        {
            return await _context.WellBeingReports
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<WellBeingReport>> GetReportsForSupervisorAsync(int supervisorId)
        {
            return await _context.WellBeingReports
                .Include(r => r.Student)
                .Where(r => r.Student.PersonalSupervisorId == supervisorId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> CanSubmitReportAsync(int studentId)
        {
            var lastReport = await _context.WellBeingReports
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.SubmittedAt)
                .FirstOrDefaultAsync();

            return lastReport == null || lastReport.SubmittedAt < DateTime.UtcNow.AddDays(-7);
        }

        private Task FlagReportForSupervisor(WellBeingReport report)
        {
            // simple sync work — return completed task to avoid CS1998
            Console.WriteLine($"ALERT: Student {report.StudentId} reported status: {report.Status}");
            return Task.CompletedTask;
        }
    }
}