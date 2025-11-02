using SESH.Models.Enums;

namespace SESH.Tests
{
    public class IntegrationTests : TestBase
    {
        [Fact]
        public async Task CompleteStudentWorkflow_SubmitReportAndBookMeeting_Success()
        {
            // Arrange
            var reportService = new ReportService(_context);
            var meetingService = new MeetingService(_context);
            var studentId = 2;
            var supervisorId = 1;

            // Act - Submit a report
            var reportResult = await reportService.SubmitReportAsync(
                studentId, ReportStatus.Struggling, "Need help with coursework");

            // Assert report was created
            Assert.True(reportResult.Success);

            // Act - Book a meeting
            var availableSlots = await meetingService.GetAvailableSlotsAsync(supervisorId);
            var meetingResult = await meetingService.BookMeetingAsync(
                studentId, supervisorId, availableSlots[0].Id, "Help Session", "Need assistance");

            // Assert meeting was booked
            Assert.True(meetingResult.Success);

            // Act - Get student meetings
            var studentMeetings = await meetingService.GetUserMeetingsAsync(studentId);

            // Assert meeting appears in list
            Assert.Single(studentMeetings);
            Assert.Equal("Help Session", studentMeetings[0].Title);
        }

        [Fact]
        public async Task SupervisorWorkflow_RegisterStudentAndMonitor_Success()
        {
            // Arrange
            var registrationService = new UserRegistrationService(_context);
            var reportService = new ReportService(_context);
            var supervisorId = 1;

            // Act - Register a new student
            var registrationResult = await registrationService.RegisterStudentAsync(
                "Workflow Student", "workflow@edu.hud.ac.uk", "S55555", "password123", supervisorId);

            // Assert registration successful
            Assert.True(registrationResult.Success);

            // Act - Student submits report
            var newStudentId = registrationResult.User!.Id;
            var reportResult = await reportService.SubmitReportAsync(
                newStudentId, ReportStatus.InCrisis, "Urgent help needed");

            // Assert report created and supervisor can see it
            Assert.True(reportResult.Success);

            var supervisorReports = await reportService.GetReportsForSupervisorAsync(supervisorId);
            var studentReport = supervisorReports.FirstOrDefault(r => r.StudentId == newStudentId);

            Assert.NotNull(studentReport);
            Assert.Equal(ReportStatus.InCrisis, studentReport.Status);
        }
    }
}