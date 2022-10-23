using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core;
using Nop.Data;
using Nop.Core.Domain.Self;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Helpers;

namespace Nop.Services.Self
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IDateTimeHelper _dateTimeHelper;

        public AppointmentService(IRepository<Appointment> appointmentRepository,
            IDateTimeHelper dateTimeHelper)
        {
            _appointmentRepository = appointmentRepository;
            _dateTimeHelper = dateTimeHelper;
        }

        public virtual async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _appointmentRepository.GetByIdAsync(appointmentId, cache => default);
        }

        /// <summary>
        /// Inserts a appointment
        /// </summary>
        /// <param name="appointment">Appointment</param>
        public virtual async Task InsertAppointmentAsync(Appointment appointment)
        {
            await _appointmentRepository.InsertAsync(appointment);
        }

        public virtual async Task InsertAppointmentsAsync(List<Appointment> appointments)
        {
            //insert
            await _appointmentRepository.InsertAsync(appointments);
        }

        /// <summary>
        /// Updates the appointment
        /// </summary>
        /// <param name="appointment">Appointment</param>
        public virtual async Task UpdateAppointmentAsync(Appointment appointment)
        {
            await _appointmentRepository.UpdateAsync(appointment);
        }

        public virtual async Task DeleteAppointmentAsync(Appointment appointment)
        {
            await _appointmentRepository.DeleteAsync(appointment);
        }

        public virtual async Task<List<Appointment>> GetAvailableAppointmentsByCustomerAsync(DateTime startTime, DateTime endTime, int resourceId, int customerId)
        {
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(startTime);
            var endTimeUtc = _dateTimeHelper.ConvertToUtcTime(endTime);

            var query = _appointmentRepository.Table
                .Where(x => x.ResourceId == resourceId)
                .Where(x => !x.CustomerId.HasValue || x.CustomerId == customerId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return await query.ToListAsync();
        }

        #region Tennis court booking

        public virtual async Task<List<Appointment>> GetAppointmentsByResourceAsync(DateTime startTime, DateTime endTime, int resourceId)
        {
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(startTime);
            var endTimeUtc = _dateTimeHelper.ConvertToUtcTime(endTime);

            var query = _appointmentRepository.Table
                .Where(x => x.ResourceId == resourceId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get appointments of grouped products by parent product id
        /// </summary>
        /// <param name="parentProductId"></param>
        /// <param name="startTimeUtc"></param>
        /// <param name="endTimeUtc"></param>
        /// <returns></returns>
        public virtual async Task<List<Appointment>> GetAppointmentsByParentAsync(int parentProductId, DateTime startTime, DateTime endTime)
        {
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(startTime);
            var endTimeUtc = _dateTimeHelper.ConvertToUtcTime(endTime);

            var query = _appointmentRepository.Table
                .Where(x => x.ParentProductId == parentProductId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Check if an appointment record has been created for the resource and the time slot
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="startTimeUtc"></param>
        /// <param name="endTimeUtc"></param>
        /// <returns></returns>
        public virtual bool IsTaken(int resourceId, DateTime startTime, DateTime endTime)
        {
            var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(startTime);
            var endTimeUtc = _dateTimeHelper.ConvertToUtcTime(endTime);

            var query = _appointmentRepository.Table
                .Where(x => x.ResourceId == resourceId)
                .Where(x => (x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc) 
                            || (x.EndTimeUtc > startTimeUtc && x.EndTimeUtc <= endTimeUtc)
                            || (x.StartTimeUtc < startTimeUtc && x.EndTimeUtc > endTimeUtc));
            int count = query.Count();
            return count > 0;
        }

        #endregion Tennis court booking
    }
}
