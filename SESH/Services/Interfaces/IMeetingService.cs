using SESH.Models;

namespace SESH.Services.Interfaces
{
    public class MeetingResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Meeting? Meeting { get; set; }

        public static MeetingResult SuccessResult(Meeting meeting)
            => new() { Success = true, Meeting = meeting };

        public static MeetingResult FailureResult(string error)
            => new() { Success = false, ErrorMessage = error };
    }

    public interface IMeetingService
    {
        Task<MeetingResult> BookMeetingAsync(int bookedById, int bookedWithId, int slotId, string title, string description);
        Task<List<AvailabilitySlot>> GetAvailableSlotsAsync(int supervisorId);
        Task<List<Meeting>> GetUserMeetingsAsync(int userId);
        Task<bool> CancelMeetingAsync(int meetingId, int userId);
        Task<List<AvailabilitySlot>> GetSupervisorAvailabilityAsync(int supervisorId);
        Task<AvailabilitySlot> AddAvailabilitySlotAsync(int supervisorId, DateTime startTime, DateTime endTime);
    }
}