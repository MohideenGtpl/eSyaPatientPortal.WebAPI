using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSyaPatientPortal.IF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eSyaPatientPortal.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PatientInfoController : ControllerBase
    {
        private readonly IPatientInfoRepository _patientInfoRepository;

        public PatientInfoController(IPatientInfoRepository patientInfoRepository)
        {
            _patientInfoRepository = patientInfoRepository;
        }

        public async Task<IActionResult> GetPatientInfoByMobileNumber(int businessKey, string mobileNumber)
        {
            var ds = await _patientInfoRepository.GetPatientInfoByMobileNumber(businessKey, mobileNumber);
            return Ok(ds);
        }

        public async Task<IActionResult> GetPatientInfoByUHID(int businessKey, int UHID)
        {
            var ds = await _patientInfoRepository.GetPatientInfoByUHID(businessKey, UHID);
            return Ok(ds);
        }
    }
}