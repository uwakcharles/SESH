using SESH.Models;
using SESH.Services;

namespace SESH.Tests
{
    public class MeetingServiceTests : TestBase
    {
        private readonly MeetingService _meetingService;

        public MeetingServiceTests()
        {
            _meetingService = new MeetingService(_context);
        }

        [Fact]
        public async Task BookMeeting_ValidSlot_ReturnsSuccess()
        {
            // Arrange
            var studentId = 2;
            var supervisorId = 1;
            var slotId = 1;
            var title = "Progress Discussion";
            var description = "Weekly progress check";

            // Act
            var result = await _meetingService.BookMeetingAsync(studentId, supervisorId, slotId, title, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Meeting);
            Assert.Equal(title, result.Meeting.Title);
            Assert.Equal(Models.Enums.MeetingStatus.Scheduled, result.Meeting.Status);

            // Verify slot is now booked
            var slot = await _context.AvailabilitySlots.FindAsync(slotId);
            Assert.True(slot?.IsBooked);
        }

        [Fact]
        public async Task BookMeeting_AlreadyBookedSlot_ReturnsFailure()
        {
            // Arrange
            var slotId = 1;
            var slot = await _context.AvailabilitySlots.FindAsync(slotId);
            slot!.IsBooked = true; // Mark as already booked
            await _context.SaveChangesAsync();

            // Act
            var result = await _meetingService.BookMeetingAsync(2, 1, slotId, "Test", "Test");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("no longer available", result.ErrorMessage);
        }

        [Fact]
        public async Task BookMeeting_InvalidUser_ReturnsFailure()
        {
            // Arrange
            var invalidUserId = 999; // Non-existent user

            // Act
            var result = await _meetingService.BookMeetingAsync(invalidUserId, 1, 1, "Test", "Test");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid user", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsOnlyUnbookedFutureSlots()
        {
            // Arrange
            var supervisorId = 1;

            // Add a booked slot and a past slot
            var bookedSlot = new AvailabilitySlot
            {
                PersonalSupervisorId = supervisorId,
                StartTime = DateTime.UtcNow.AddDays(2),
                EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
                IsBooked = true
            };

            var pastSlot = new AvailabilitySlot
            {
                PersonalSupervisorId = supervisorId,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
                IsBooked = false
            };

            _context.AvailabilitySlots.AddRange(bookedSlot, pastSlot);
            await _context.SaveChangesAsync();

            // Act
            var availableSlots = await _meetingService.GetAvailableSlotsAsync(supervisorId);

            // Assert
            Assert.Single(availableSlots); // Only the original test slot should be available
            Assert.All(availableSlots, slot =>
            {
                Assert.False(slot.IsBooked);
                Assert.True(slot.StartTime > DateTime.UtcNow);
            });
        }

        [Fact]
        public async Task GetUserMeetings_ReturnsUserMeetings()
        {
            // Arrange - Create a meeting
            var meeting = new Meeting
            {
                Title = "Test Meeting",
                Description = "Test Description",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Status = Models.Enums.MeetingStatus.Scheduled,
                BookedById = 2,
                BookedWithId = 1
            };
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Act
            var studentMeetings = await _meetingService.GetUserMeetingsAsync(2); // Student
            var supervisorMeetings = await _meetingService.GetUserMeetingsAsync(1); // Supervisor

            // Assert
            Assert.Single(studentMeetings);
            Assert.Single(supervisorMeetings);
        }

        [Fact]
        public async Task CancelMeeting_ValidMeeting_CancelsSuccessfully()
        {
            // Arrange
            var meeting = new Meeting
            {
                Title = "Test Meeting",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Status = Models.Enums.MeetingStatus.Scheduled,
                BookedById = 2,
                BookedWithId = 1
            };
            _context.Meetings.Add(meeting);

            var slot = await _context.AvailabilitySlots.FindAsync(1);
            slot!.IsBooked = true;

            await _context.SaveChangesAsync();

            // Act
            var result = await _meetingService.CancelMeetingAsync(meeting.Id, 2); // Student cancelling

            // Assert
            Assert.True(result);

            var cancelledMeeting = await _context.Meetings.FindAsync(meeting.Id);
            Assert.Equal(Models.Enums.MeetingStatus.Cancelled, cancelledMeeting?.Status);

            var freedSlot = await _context.AvailabilitySlots.FindAsync(1);
            Assert.False(freedSlot?.IsBooked);
        }

        [Fact]
        public async Task CancelMeeting_InvalidUser_ReturnsFalse()
        {
            // Arrange
            var meeting = new Meeting
            {
                Title = "Test Meeting",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Status = Models.Enums.MeetingStatus.Scheduled,
                BookedById = 2,
                BookedWithId = 1
            };
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Act - Try to cancel with unauthorized user
            var result = await _meetingService.CancelMeetingAsync(meeting.Id, 999); // Invalid user

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddAvailabilitySlot_ValidTimes_CreatesSuccessfully()
        {
            // Arrange
            var supervisorId = 1;
            var startTime = DateTime.UtcNow.AddDays(3);
            var endTime = startTime.AddHours(1);

            // Act
            var slot = await _meetingService.AddAvailabilitySlotAsync(supervisorId, startTime, endTime);

            // Assert
            Assert.NotNull(slot);
            Assert.Equal(supervisorId, slot.PersonalSupervisorId);
            Assert.Equal(startTime, slot.StartTime);
            Assert.Equal(endTime, slot.EndTime);
            Assert.False(slot.IsBooked);
        }
    }
}