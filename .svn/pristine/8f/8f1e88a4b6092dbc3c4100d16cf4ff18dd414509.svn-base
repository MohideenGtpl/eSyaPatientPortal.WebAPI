using eSyaPatientPortal.DO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eSyaPatientPortal.IF
{
    public interface IDoctorClinicRepository
    {
        Task<List<DO_Specialty>> GetSpecialty(int businessKey);

        Task<List<DO_Doctor>> GetDoctorsBySearchText(int businessKey, string searchText);

        Task<List<DO_Doctor>> GetDoctorsBySearchCriteria(int businessKey, int specialtyId, int doctorId, string gender, string preferedTimeSlot);

        Task<List<DO_DoctorSchedule>> GetDoctorScheduleTimeForAppointmentDate(int businessKey,
            int clinicType, int consultationType,
            int specialtyId, int doctorId, DateTime scheduleDate);

        Task<List<DO_DoctorSchedule>> GetDoctorScheduleTime(int businessKey, int clinicType, int consultationType, int specialtyId, int doctorId, DateTime scheduleDate);

        Task<List<DO_Doctor>> GetDoctorSchedule(int businessKey, int specialtyId, int? doctorId);

        Task<List<DO_Doctor>> GetDoctorScheduleByDate(int businessKey, int specialtyId, int doctorId, DateTime startDate, DateTime endDate);

        Task<List<DO_Doctor>> GetDoctorScheduleByID(int businessKey,
                  int doctorID, DateTime fromDate);
    }
}
