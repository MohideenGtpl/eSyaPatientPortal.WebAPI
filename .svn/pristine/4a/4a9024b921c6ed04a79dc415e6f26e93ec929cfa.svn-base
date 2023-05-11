using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSyaPatientPortal.DO;
using eSyaPatientPortal.IF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eSyaPatientPortal.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppointmentBookingController : ControllerBase
    {
        private readonly IAppointmentBookingRepository _appointmentBookingRepository;

        public AppointmentBookingController(IAppointmentBookingRepository appointmentBookingRepository)
        {
            _appointmentBookingRepository = appointmentBookingRepository;
        }

        [HttpPost]
        public async Task<IActionResult> InsertIntoDoctorSlotBooking(DO_PatientAppointmentDetail obj)
        {
            var ds = await _appointmentBookingRepository.InsertIntoDoctorSlotBooking(obj);
            return Ok(ds);
        }

        [HttpPost]
        public async Task<IActionResult> InsertIntoPatientAppointmentDetail(DO_PatientAppointmentDetail obj)
        {
            var ds = await _appointmentBookingRepository.InsertIntoPatientAppointmentDetail(obj);
            return Ok(ds);
        }

        public async Task<IActionResult> GetApplicationCodesByCodeType(int codeType)
        {
            var ds = await _appointmentBookingRepository.GetApplicationCodesByCodeType(codeType);
            return Ok(ds);
        }

        public async Task<IActionResult> GetCustomer(int businessKey)
        {
            var ds = await _appointmentBookingRepository.GetCustomer(businessKey);
            return Ok(ds);
        }


    }
}