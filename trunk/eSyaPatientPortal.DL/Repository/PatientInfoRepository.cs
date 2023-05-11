using eSyaPatientPortal.DL.Entities;
using eSyaPatientPortal.DO;
using eSyaPatientPortal.IF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eSyaPatientPortal.DL.Repository
{
    public class PatientInfoRepository : IPatientInfoRepository
    {
        public async Task<List<DO_PatientInfo>> GetPatientInfoByMobileNumber(int businessKey, string mobileNumber)
        {
            using (var db = new eSyaEnterpriseContext())
            {
                var pm = await db.GtEfoppr
                    .Where(w => w.MobileNumber.ToString() == mobileNumber && w.ActiveStatus)
                    .AsNoTracking()
                    .Select(r => new DO_PatientInfo
                    {
                        UHID = r.RUhid,
                        PatientId = r.PatientId,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Gender = r.Gender,
                        DateOfBirth = r.DateOfBirth,
                        MobileNumber = r.MobileNumber,
                        EmailId = r.EMailId,
                        // CustomerId = r.CustomerId

                    }).ToListAsync();

                return pm;
            }
        }

        public async Task<DO_PatientInfo> GetPatientInfoByUHID(int businessKey, int UHID)
        {
            using (var db = new eSyaEnterpriseContext())
            {
                var pm = await db.GtEfoppr
                    .Where(w => w.RUhid == UHID && w.ActiveStatus)
                    .AsNoTracking()
                    .Select(r => new DO_PatientInfo
                    {
                        UHID = r.RUhid,
                        PatientId = r.PatientId,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Gender = r.Gender,
                        DateOfBirth = r.DateOfBirth,
                        MobileNumber = r.MobileNumber,
                        EmailId = r.EMailId,
                        // CustomerId = r.CustomerId

                    }).FirstOrDefaultAsync();

                return pm;
            }
        }
    }
}
