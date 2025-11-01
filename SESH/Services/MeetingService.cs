using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Services.Interfaces;

namespace SESH.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly ApplicationDbContext _context;

        public MeetingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MeetingResult> BookMeetingAsync(int bookedById, int bookedWithId, int slotId, string title, string description)
        {
            var slot = await _context.AvailabilitySlots
                .Include(s => s.PersonalSupervisor)
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsBooked);

            if (slot == null)
                return MeetingResult.FailureResult("The selected time slot is no longer available.");

            var bookedBy = await _context.Users.FindAsync(bookedById);
            var bookedWith = await _context.Users.FindAsync(bookedWithId);

            if (bookedBy == null || bookedWith == null)
                return MeetingResult.FailureResult("Invalid user specified.");

            var meeting = new Meeting
            {
                Title = title,
                Description = description,
                ScheduledAt = slot.StartTime,
                Status = Models.Enums.MeetingStatus.Scheduled,
                BookedById = bookedById,
                BookedWithId = bookedWithId
            };

            slot.IsBooked = true;

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            return MeetingResult.SuccessResult(meeting);
        }

        public async Task<List<AvailabilitySlot>> GetAvailableSlotsAsync(int supervisorId)
        {
            return await _context.AvailabilitySlots
                .Where(s => s.PersonalSupervisorId == supervisorId && !s.IsBooked && s.StartTime > DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Meeting>> GetUserMeetingsAsync(int userId)
        {
            return await _context.Meetings
                .Include(m => m.BookedBy)
                .Include(m => m.BookedWith)
                .Where(m => m.BookedById == userId || m.BookedWithId == userId)
                .Where(m => m.Status == Models.Enums.MeetingStatus.Scheduled)
                .OrderBy(m => m.ScheduledAt)
                .ToListAsync();
        }

        public async Task<bool> CancelMeetingAsync(int meetingId, int userId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.BookedBy)
                .FirstOrDefaultAsync(m => m.Id == meetingId && (m.BookedById == userId || m.BookedWithId == userId));

            if (meeting == null) return false;

            meeting.Status = Models.Enums.MeetingStatus.Cancelled;

            var slot = await _context.AvailabilitySlots
                .FirstOrDefaultAsync(s => s.PersonalSupervisorId == meeting.BookedWithId && s.StartTime == meeting.ScheduledAt);

            if (slot != null) slot.IsBooked = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AvailabilitySlot>> GetSupervisorAvailabilityAsync(int supervisorId)
        {
            return await _context.AvailabilitySlots
                .Where(s => s.PersonalSupervisorId == supervisorId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<AvailabilitySlot> AddAvailabilitySlotAsync(int supervisorId, DateTime startTime, DateTime endTime)
        {
            var slot = new AvailabilitySlot
            {
                PersonalSupervisorId = supervisorId,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = false
            };

            _context.AvailabilitySlots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }
    }
}