using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models.Enums;

namespace SESH.Services
{
    public class AnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CohortAnalytics> GetCohortAnalyticsAsync()
        {
            var reports = await _context.WellBeingReports
                .Where(r => r.SubmittedAt >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            var meetings = await _context.Meetings
                .Where(m => m.ScheduledAt >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            return new CohortAnalytics
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalReports = reports.Count,
                StatusDistribution = reports.GroupBy(r => r.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TotalMeetings = meetings.Count(m => m.Status == Models.Enums.MeetingStatus.Completed),
                AverageResponseTime = 1.5
            };
        }

        public async Task<List<SupervisorEngagement>> GetSupervisorEngagementAsync()
        {
            var supervisors = await _context.PersonalSupervisors
                .Include(ps => ps.AssignedStudents)
                .ThenInclude(s => s.Reports)
                .ToListAsync();

            return supervisors.Select(ps => new SupervisorEngagement
            {
                SupervisorName = ps.Name,
                StudentCount = ps.AssignedStudents.Count,
                MeetingsCount = ps.MeetingsWith.Count(m => m.ScheduledAt >= DateTime.UtcNow.AddDays(-30)),
                AverageResponseTime = 1.2
            }).ToList();
        }
    }

    public class CohortAnalytics
    {
        public int TotalStudents { get; set; }
        public int TotalReports { get; set; }
        public Dictionary<ReportStatus, int> StatusDistribution { get; set; } = new();
        public int TotalMeetings { get; set; }
        public double AverageResponseTime { get; set; }
    }

    public class SupervisorEngagement
    {
        public string SupervisorName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int MeetingsCount { get; set; }
        public double AverageResponseTime { get; set; }
    }
}