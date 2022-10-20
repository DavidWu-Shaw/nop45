using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core;
using Nop.Data;
using Nop.Core.Domain.Self;
using System.Linq;

namespace Nop.Services.Self
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointmentRepository;

        public AppointmentService(
            IRepository<Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public virtual Appointment GetAppointmentById(int appointmentId)
        {
            if (appointmentId == 0)
                return null;

            return _appointmentRepository.GetById(appointmentId);
        }

        /// <summary>
        /// Inserts a appointment
        /// </summary>
        /// <param name="appointment">Appointment</param>
        public virtual void InsertAppointment(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            //insert
            _appointmentRepository.Insert(appointment);
        }

        public virtual void InsertAppointments(List<Appointment> appointments)
        {
            //insert
            _appointmentRepository.Insert(appointments);
        }

        /// <summary>
        /// Updates the appointment
        /// </summary>
        /// <param name="appointment">Appointment</param>
        public virtual void UpdateAppointment(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            //update
            _appointmentRepository.Update(appointment);

            //event notification
            _eventPublisher.EntityUpdated(appointment);
        }

        public virtual void DeleteAppointment(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            //update
            _appointmentRepository.Delete(appointment);

            //event notification
            _eventPublisher.EntityDeleted(appointment);
        }

        public virtual List<Appointment> GetAvailableAppointmentsByCustomer(DateTime startTimeUtc, DateTime endTimeUtc, int resourceId, int customerId)
        {
            var query = _appointmentRepository.Table
                .Where(x => x.ResourceId == resourceId)
                .Where(x => !x.CustomerId.HasValue || x.CustomerId == customerId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return query.ToList();
        }

        #region Tennis court booking

        public virtual List<Appointment> GetAppointmentsByResource(DateTime startTimeUtc, DateTime endTimeUtc, int resourceId)
        {
            var query = _appointmentRepository.Table
                .Where(x => x.ResourceId == resourceId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return query.ToList();
        }

        /// <summary>
        /// Get appointments of grouped products by parent product id
        /// </summary>
        /// <param name="parentProductId"></param>
        /// <param name="startTimeUtc"></param>
        /// <param name="endTimeUtc"></param>
        /// <returns></returns>
        public virtual List<Appointment> GetAppointmentsByParent(int parentProductId, DateTime startTimeUtc, DateTime endTimeUtc)
        {
            var query = _appointmentRepository.Table
                .Where(x => x.ParentProductId == parentProductId)
                .Where(x => x.StartTimeUtc >= startTimeUtc && x.StartTimeUtc < endTimeUtc);

            return query.ToList();
        }

        /// <summary>
        /// Check if an appointment record has been created for the resource and the time slot
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="startTimeUtc"></param>
        /// <param name="endTimeUtc"></param>
        /// <returns></returns>
        public virtual bool IsTaken(int resourceId, DateTime startTimeUtc, DateTime endTimeUtc)
        {
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
