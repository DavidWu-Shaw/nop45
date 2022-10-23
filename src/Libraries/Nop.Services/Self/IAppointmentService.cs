using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Self;

namespace Nop.Services.Self
{
    public partial interface IAppointmentService
    {
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task InsertAppointmentAsync(Appointment appointment);
        Task InsertAppointmentsAsync(List<Appointment> appointments);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteAppointmentAsync(Appointment appointment);

        #region Tennis court booking
        Task<List<Appointment>> GetAppointmentsByResourceAsync(DateTime startTime, DateTime endTime, int resourceId);
        Task<List<Appointment>> GetAppointmentsByParentAsync(int parentProductId, DateTime startTime, DateTime endTime);
        Task<List<Appointment>> GetAvailableAppointmentsByCustomerAsync(DateTime startTime, DateTime endTime, int resourceId, int customerId);
        bool IsTaken(int resourceId, DateTime startTime, DateTime endTime);
        #endregion Tennis court booking
    }
}
