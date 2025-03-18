using System.Collections.Generic;
using System.Threading.Tasks;
using Vet_System.Models;

namespace Vet_System.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentItem>> GetAppointmentsAsync();

        Task<AppointmentItem> CreateAppointmentAsync(AppointmentItem appointment);

        Task<bool> UpdateAppointmentAsync(AppointmentItem appointment);

        Task<bool> CancelAppointmentAsync(string appointmentId);

        Task<AppointmentItem> GetAppointmentByIdAsync(string appointmentId);
    }
}