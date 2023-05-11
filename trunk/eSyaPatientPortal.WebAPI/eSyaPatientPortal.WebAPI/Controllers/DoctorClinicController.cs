using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSyaPatientPortal.DL.Repository;
using eSyaPatientPortal.DO;
using eSyaPatientPortal.IF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eSyaPatientPortal.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DoctorClinicController : ControllerBase
    {
        private readonly IDoctorClinicRepository _doctorClinicRepository;
        private readonly IAppointmentBookingRepository _appointmentBookingRepository;

        public DoctorClinicController(IDoctorClinicRepository doctorClinicRepository, IAppointmentBookingRepository appointmentBookingRepository)
        {
            _doctorClinicRepository = doctorClinicRepository;
            _appointmentBookingRepository = appointmentBookingRepository;
        }

        public async Task<IActionResult> GetSpecialty(int businessKey)
        {
            var ds = await _doctorClinicRepository.GetSpecialty(businessKey);
            return Ok(ds);
        }

        public async Task<IActionResult> GetDoctorsBySearchText(int businessKey, string searchText)
        {
            var ds = await _doctorClinicRepository.GetDoctorsBySearchText(businessKey, searchText);

            return Ok(ds);
        }

        public async Task<IActionResult> GetDoctorsBySearchCriteria(int businessKey, int specialtyId, int doctorId, string gender, string preferedTimeSlot)
        {
            var ds = await _doctorClinicRepository.GetDoctorsBySearchCriteria(businessKey, specialtyId, doctorId, gender, preferedTimeSlot);

            return Ok(ds);
        }

        public async Task<IActionResult> GetDoctorAvailableTimeSlotForGivenDate(int businessKey, int clinicType, int consultationType, int specialtyId, int doctorId, DateTime appointmentDate)
        {
            var ds = await _doctorClinicRepository.GetDoctorScheduleTimeForAppointmentDate(businessKey, clinicType, consultationType, specialtyId, doctorId, appointmentDate);
            var appBooked = await _appointmentBookingRepository.GetAppointmentBookedSlotByDoctorDate(businessKey, specialtyId, doctorId, appointmentDate);

            List<DO_DoctorTimeSlot> l_timeSlot = new List<DO_DoctorTimeSlot>();
            foreach (var t in ds)
            {
                TimeSpan startTime = t.FromTime;
                while (startTime < t.ToTime)
                {
                    TimeSpan duration = TimeSpan.FromMinutes(5);
                    TimeSpan endTime = startTime.Add(duration);

                    var app_exists = appBooked.Where(w =>
                             (w.AppointmentFromTime <= startTime
                                 && w.AppointmentFromTime.Add(TimeSpan.FromMinutes(w.Duration)) > startTime)
                             || (startTime <= w.AppointmentFromTime
                                 && endTime > w.AppointmentFromTime)
                             ).Count();

                    if (app_exists <= 0)
                    {
                        DO_DoctorTimeSlot sl = new DO_DoctorTimeSlot();
                        sl.TimeSlot = startTime;
                        if (startTime < new TimeSpan(12, 00, 00))
                            sl.SlotType = "M";
                        if (startTime >= new TimeSpan(12, 00, 00) && startTime < new TimeSpan(18, 00, 00))
                            sl.SlotType = "A";
                        if (startTime >= new TimeSpan(18, 00, 00))
                            sl.SlotType = "E";
                        l_timeSlot.Add(sl);
                    }

                    startTime = endTime;
                }
            }

            return Ok(l_timeSlot);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessKey"></param>
        /// <param name="specialtyId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDoctorSchedule(int businessKey, int specialtyId, int? doctorId)
        {
            var ds = await _doctorClinicRepository.GetDoctorSchedule(businessKey, specialtyId, doctorId);

            return Ok(ds);
        }

        public async Task<IActionResult> GetDoctorScheduleByDate(int businessKey, int specialtyId, int doctorId, DateTime fromDate, DateTime toDate)
        {
            var ds = await _doctorClinicRepository.GetDoctorScheduleByDate(businessKey, specialtyId, doctorId, fromDate, toDate);

            return Ok(ds);
        }

    }
}