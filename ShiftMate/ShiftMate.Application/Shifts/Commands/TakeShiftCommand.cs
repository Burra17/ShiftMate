using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands
{
    public class TakeShiftCommand : IRequest<bool>
    {
        public Guid ShiftId { get; set; }
        public Guid UserId { get; set; }

        [JsonIgnore]
        public Guid OrganizationId { get; set; }
    }
}
