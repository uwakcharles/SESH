using SESH.Models;
using SESH.Models.Enums;
using SESH.Services;
using SESH.Services.Interfaces;

namespace SESH.Tests
{
	public class ReportServiceTests : TestBase
	{
		private readonly ReportService _reportService;

		public ReportServiceTests()
		{
			_reportService = new ReportService(_context);
		}

		[Fact]
		public async Task SubmitReport_ValidData_ReturnsSuccess()
		{
			// Arrange
			var studentId = 2;
			var status = ReportStatus.Okay;
			var notes = "Feeling good this week";

			// Act
			var result = await _reportService.SubmitReportAsync(studentId, status, notes);

			// Assert
			Assert.True(result.Success);
			Assert.NotNull(result.Report);
			Assert.Equal(status, result.Report.Status);
			Assert.Equal(notes, result.Report.Notes);
			Assert.Equal(studentId, result.Report.StudentId);
		}

		[Fact]
		public async Task SubmitReport_Within7Days_ReturnsFailure()
		{
			// Arrange
			var studentId = 2;

			// Act - Try to submit another report within 7 days
			var result = await _reportService.SubmitReportAsync(studentId, ReportStatus.Struggling, "Struggling now");

			// Assert
			Assert.False(result.Success);
			Assert.Contains("once per week", result.ErrorMessage);
		}

		[Fact]
		public async Task SubmitReport_NotesExceed500Chars_ReturnsFailure()
		{
			// Arrange
			var studentId = 2;
			var longNotes = new string('a', 501); // 501 characters

			// Act
			var result = await _reportService.SubmitReportAsync(studentId, ReportStatus.Okay, longNotes);

			// Assert
			Assert.False(result.Success);
			Assert.Contains("500 characters", result.ErrorMessage);
		}

		[Theory]
		[InlineData(ReportStatus.Thriving)]
		[InlineData(ReportStatus.Okay)]
		[InlineData(ReportStatus.Struggling)]
		[InlineData(ReportStatus.InCrisis)]
		public async Task SubmitReport_AllStatusTypes_WorksCorrectly(ReportStatus status)
		{
			// Arrange - Create a new student without reports
			var newStudent = new Student
			{
				Name = "New Test Student",
				Email = "new.student@edu.hud.ac.uk",
				StudentId = "S99999",
				PersonalSupervisorId = 1,
				Role = UserRole.Student
			};
			newStudent.SetPassword("password123");
			_context.Students.Add(newStudent);
			await _context.SaveChangesAsync();

			// Act
			var result = await _reportService.SubmitReportAsync(newStudent.Id, status, "Test notes");

			// Assert
			Assert.True(result.Success);
			Assert.Equal(status, result.Report.Status);
		}

		[Fact]
		public async Task GetStudentReports_ReturnsCorrectReports()
		{
			// Arrange
			var studentId = 2;

			// Act
			var reports = await _reportService.GetStudentReportsAsync(studentId);

			// Assert
			Assert.Single(reports);
			Assert.All(reports, r => Assert.Equal(studentId, r.StudentId));
		}

		[Fact]
		public async Task GetReportsForSupervisor_ReturnsAssignedStudentsReports()
		{
			// Arrange
			var supervisorId = 1;

			// Act
			var reports = await _reportService.GetReportsForSupervisorAsync(supervisorId);

			// Assert
			Assert.NotEmpty(reports);
			Assert.All(reports, r => Assert.Equal(supervisorId, r.Student.PersonalSupervisorId));
		}

		[Fact]
		public async Task CanSubmitReport_NoPreviousReport_ReturnsTrue()
		{
			// Arrange - Create a new student without any reports
			var newStudent = new Student
			{
				Name = "Fresh Student",
				Email = "fresh@edu.hud.ac.uk",
				StudentId = "S11111",
				PersonalSupervisorId = 1,
				Role = UserRole.Student
			};
			newStudent.SetPassword("password123");
			_context.Students.Add(newStudent);
			await _context.SaveChangesAsync();

			// Act
			var canSubmit = await _reportService.CanSubmitReportAsync(newStudent.Id);

			// Assert
			Assert.True(canSubmit);
		}

		[Fact]
		public async Task CanSubmitReport_ReportOlderThan7Days_ReturnsTrue()
		{
			// Arrange - Create a student with an old report
			var studentWithOldReport = new Student
			{
				Name = "Old Report Student",
				Email = "old@edu.hud.ac.uk",
				StudentId = "S22222",
				PersonalSupervisorId = 1,
				Role = UserRole.Student
			};
			studentWithOldReport.SetPassword("password123");
			_context.Students.Add(studentWithOldReport);

			var oldReport = new WellBeingReport
			{
				StudentId = studentWithOldReport.Id,
				Status = ReportStatus.Okay,
				Notes = "Old report",
				SubmittedAt = DateTime.UtcNow.AddDays(-8) // 8 days ago
			};
			_context.WellBeingReports.Add(oldReport);

			await _context.SaveChangesAsync();

			// Act
			var canSubmit = await _reportService.CanSubmitReportAsync(studentWithOldReport.Id);

			// Assert
			Assert.True(canSubmit);
		}
	}
}