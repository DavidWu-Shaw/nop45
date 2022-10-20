using System;
using System.Collections.Generic;
using Nop.Core.Domain.Self;

namespace Nop.Services.Self
{
    public partial interface IAppointmentService
    {
        Appointment GetAppointmentById(int appointmentId);
        void InsertAppointment(Appointment appointment);
        void InsertAppointments(List<Appointment> appointments);
        void UpdateAppointment(Appointment appointment);
        void DeleteAppointment(Appointment appointment);
        #region Tennis court booking
        List<Appointment> GetAppointmentsByResource(DateTime startTimeUtc, DateTime endTimeUtc, int resourceId);
        List<Appointment> GetAppointmentsByParent(int parentProductId, DateTime startTimeUtc, DateTime endTimeUtc);
        List<Appointment> GetAvailableAppointmentsByCustomer(DateTime startTimeUtc, DateTime endTimeUtc, int resourceId, int customerId);
        bool IsTaken(int resourceId, DateTime startTimeUtc, DateTime endTimeUtc);
        #endregion Tennis court booking
    }
}
