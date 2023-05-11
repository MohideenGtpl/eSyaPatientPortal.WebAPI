using eSyaPatientPortal.DL.Entities;
using eSyaPatientPortal.DO;
using eSyaPatientPortal.DO.StaticVariables;
using eSyaPatientPortal.IF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eSyaPatientPortal.DL.Repository
{
    public class AppointmentBookingRepository : IAppointmentBookingRepository
    {
        private eSyaEnterpriseContext _context { get; set; }
        public AppointmentBookingRepository(eSyaEnterpriseContext context)
        {
            _context = context;
        }

        public async Task<DO_ReturnParameter> InsertIntoDoctorSlotBooking(DO_PatientAppointmentDetail obj)
        {

            try
            {
                bool warning = false;
                string warningMessage = "";

                var wk = obj.AppointmentDate.Date.DayOfWeek.ToString();
                var wk_No = CommonMethod.GetWeekOfMonth(obj.AppointmentDate.Date);
                var dc = await _context.GtEsdos1.Where(w => w.BusinessKey == obj.BusinessKey
                            && w.DoctorId == obj.DoctorID
                            && w.DayOfWeek.ToUpper() == wk.ToUpper()
                            && ((wk_No == 1 && w.Week1) || (wk_No == 2 && w.Week2)
                                || (wk_No == 3 && w.Week3) || (wk_No == 4 && w.Week4)
                                || (wk_No == 5 && w.Week5) || (wk_No == 6 && w.Week5))
                            && (bool)w.ActiveStatus).FirstOrDefaultAsync();

                if (dc != null && dc.NoOfPatients > 0)
                {
                    var patientBooked = await _context.GtEopaph.Where(w => w.BusinessKey == obj.BusinessKey
                           && w.DoctorId == obj.DoctorID
                           && w.AppointmentDate.Date == obj.AppointmentDate.Date
                           && !w.UnScheduleWorkOrder
                           && w.AppointmentStatus != "CN").CountAsync();

                    if (patientBooked >= dc.NoOfPatients)
                    {
                        warning = true;
                        warningMessage = "Patient Limit for the day is " + dc.NoOfPatients.ToString() + ".Already booked patient count is " + patientBooked;
                    }
                }

                var endTimeSlot = obj.AppointmentFromTime.Add(new TimeSpan(0, obj.Duration, 0));

                var is_TimeSlotExits = await _context.GtEopaph.Where(w => w.BusinessKey == obj.BusinessKey
                            && w.DoctorId == obj.DoctorID
                            && w.AppointmentDate.Date == obj.AppointmentDate.Date
                            && w.AppointmentFromTime >= obj.AppointmentFromTime
                            && w.AppointmentFromTime < endTimeSlot
                            && !w.UnScheduleWorkOrder
                            && w.AppointmentStatus != StatusVariables.Appointment.Cancelled).CountAsync();
                if (is_TimeSlotExits > 0)
                {
                    return new DO_ReturnParameter() { Warning = warning, WarningMessage = warningMessage, Success = false, Message = "The Slot has been already booked" };
                }

                var slotBlocked = await _context.GtEopaps.Where(w => w.BusinessKey == obj.BusinessKey
                           && w.DoctorId == obj.DoctorID
                           && w.AppointmentDate.Date == obj.AppointmentDate.Date
                           && w.AppointmentFromTime >= obj.AppointmentFromTime
                           && w.AppointmentFromTime < endTimeSlot
                           && w.AppointmentStatus == "SL"
                           && w.CreatedBy != obj.UserID
                           && w.CreatedOn.AddMinutes(2) > System.DateTime.Now).CountAsync();
                if (slotBlocked > 0)
                {
                    return new DO_ReturnParameter() { Warning = warning, WarningMessage = warningMessage, Success = false, Message = "The Slot has been blocked." };
                }

                var qs_apSL = new GtEopaps
                {
                    BusinessKey = obj.BusinessKey,
                    SpecialtyId = obj.SpecialtyID,
                    DoctorId = obj.DoctorID,
                    AppointmentDate = obj.AppointmentDate,
                    AppointmentFromTime = obj.AppointmentFromTime,
                    Duration = obj.Duration,
                    AppointmentStatus = "SL",
                    ActiveStatus = true,
                    FormId = obj.FormID,
                    CreatedBy = obj.UserID,
                    CreatedOn = System.DateTime.Now,
                    CreatedTerminal = obj.TerminalID
                };
                _context.GtEopaps.Add(qs_apSL);
                await _context.SaveChangesAsync();

                return new DO_ReturnParameter { Warning = warning, WarningMessage = warningMessage, Success = true };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<DO_ReturnParameter> InsertIntoPatientAppointmentDetail(DO_PatientAppointmentDetail obj)
        {

            var dbContext = _context.Database.BeginTransaction();
            {
                try
                {
                    var endTimeSlot = obj.AppointmentFromTime.Add(new TimeSpan(0, obj.Duration, 0));

                    var is_TimeSlotExits = await _context.GtEopaph.Where(w => w.BusinessKey == obj.BusinessKey
                                && w.DoctorId == obj.DoctorID
                                && w.AppointmentDate.Date == obj.AppointmentDate.Date
                                && w.AppointmentFromTime >= obj.AppointmentFromTime
                                && w.AppointmentFromTime < endTimeSlot
                                && !w.UnScheduleWorkOrder
                                && w.AppointmentStatus != StatusVariables.Appointment.Cancelled).CountAsync();
                    if (is_TimeSlotExits > 0)
                    {
                        return new DO_ReturnParameter() { Success = false, Message = "The Slot has been already booked" };
                    }

                    if (obj.UHID > 0)
                    {
                        var isAlreadyBooked = await _context.GtEopaph
                            .Join(_context.GtEopapd,
                                h => new { h.BusinessKey, h.AppointmentKey },
                                d => new { d.BusinessKey, d.AppointmentKey },
                                (h, d) => new { h, d })
                            .Where(w => w.h.BusinessKey == obj.BusinessKey
                                    && w.d.Uhid == obj.UHID
                                    && w.h.DoctorId != obj.DoctorID
                                    && w.h.AppointmentDate.Date == obj.AppointmentDate.Date
                                    && w.h.AppointmentFromTime >= obj.AppointmentFromTime
                                    && w.h.AppointmentFromTime < endTimeSlot
                                    && !w.h.UnScheduleWorkOrder
                                    && w.h.AppointmentStatus != StatusVariables.Appointment.Cancelled
                                ).CountAsync();
                        if (isAlreadyBooked > 0)
                        {
                            return new DO_ReturnParameter() { Success = false, Message = "The Same MRN is booked for different doctor for same time slot." };
                        }
                    }

                    var financialYear = _context.GtEcclco.Where(w =>
                                                       System.DateTime.Now.Date >= w.FromDate.Date
                                                    && System.DateTime.Now.Date <= w.TillDate.Date)
                                        .First().FinancialYear;
                    obj.FinancialYear = (int)financialYear;

                    var dc_ap = await _context.GtDncn01
                                   .Where(w => w.BusinessKey == obj.BusinessKey
                                       && w.FinancialYear == financialYear
                                       && w.DocumentId == DocumentIdValues.op_AppointmentId).FirstOrDefaultAsync();
                    dc_ap.CurrentDocNumber = dc_ap.CurrentDocNumber + 1;
                    dc_ap.CurrentDocDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    obj.DocumentID = 1;
                    obj.DocumentNumber = dc_ap.CurrentDocNumber;

                    var appointmentKey = long.Parse(obj.FinancialYear.ToString().Substring(2, 2) + 
                        obj.BusinessKey.ToString().PadLeft(2, '0') +
                        dc_ap.DocumentId.ToString().PadLeft(3, '0') +
                        obj.DocumentNumber.ToString());

                    obj.AppointmentKey = appointmentKey;

                    string appType = "CA";
                    if (obj.IsSponsored)
                        appType = "SA";

                    var rp = await GetAppointmentQueueToken(_context, obj);
                    if (!rp.Success)
                        return rp;
                    var qTokenKey = obj.QueueTokenKey;

                    var qs_apTk = new GtEopapq
                    {
                        BusinessKey = obj.BusinessKey,
                        TokenDate = obj.AppointmentDate.Date,
                        QueueTokenKey = qTokenKey,
                        AppointmentFromTime = obj.AppointmentFromTime,
                        AppointmentKey = obj.AppointmentKey,
                        SequeueNumber = 1,
                        PatientType = appType,
                        SpecialtyId = obj.SpecialtyID,
                        DoctorId = obj.DoctorID,
                        Uhid = obj.UHID,
                        TokenStatus = StatusVariables.Appointment.Booked,
                        ActiveStatus = true,
                        FormId = obj.FormID,
                        CreatedBy = obj.UserID,
                        CreatedOn = System.DateTime.Now,
                        CreatedTerminal = obj.TerminalID
                    };
                    _context.GtEopapq.Add(qs_apTk);
                    await _context.SaveChangesAsync();

                    var app_hd = new GtEopaph
                    {
                        BusinessKey = obj.BusinessKey,
                        FinancialYear = obj.FinancialYear,
                        DocumentId = obj.DocumentID,
                        DocumentNumber = obj.DocumentNumber,
                        AppointmentKey = obj.AppointmentKey,
                        SpecialtyId = obj.SpecialtyID,
                        DoctorId = obj.DoctorID,
                        AppointmentDate = obj.AppointmentDate,
                        AppointmentFromTime = obj.AppointmentFromTime,
                        Duration = obj.Duration,
                        AppointmentStatus = StatusVariables.Appointment.Booked,
                        ReasonforAppointment = obj.ReasonforAppointment,
                        QueueTokenKey = qTokenKey,
                        VisitType = obj.VisitType,
                        ReferredBy = obj.ReferredBy,
                        UnScheduleWorkOrder = false,
                        ActiveStatus = true,
                        FormId = obj.FormID,
                        CreatedBy = obj.UserID,
                        CreatedOn = System.DateTime.Now,
                        CreatedTerminal = obj.TerminalID
                    };
                    _context.GtEopaph.Add(app_hd);
                    await _context.SaveChangesAsync();

                    var app_dt = new GtEopapd
                    {
                        BusinessKey = obj.BusinessKey,
                        AppointmentKey = obj.AppointmentKey,
                        Uhid = obj.UHID,
                        PatientFirstName = obj.PatientFirstName,
                        PatientMiddleName = obj.PatientMiddleName,
                        PatientLastName = obj.PatientLastName,
                        Gender = obj.Gender,
                        DateOfBirth = obj.DateOfBirth,
                        MobileNumber = obj.PatientMobileNumber,
                        EmailId = obj.PatientEmailID,
                        IsSponsored = obj.IsSponsored,
                        CustomerId = obj.CustomerID,
                        ActiveStatus = true,
                        FormId = obj.FormID,
                        CreatedBy = obj.UserID,
                        CreatedOn = System.DateTime.Now,
                        CreatedTerminal = obj.TerminalID
                    };
                    _context.GtEopapd.Add(app_dt);

                    await _context.SaveChangesAsync();
                    dbContext.Commit();

                    return new DO_ReturnParameter { Success = true, ID = obj.AppointmentKey, Key = qTokenKey };
                }
                catch (DbUpdateException ex)
                {
                    dbContext.Rollback();
                    throw ex;
                }
                catch (Exception ex)
                {
                    dbContext.Rollback();
                    throw ex;
                }
            }

        }

        public async Task<DO_ReturnParameter> GetAppointmentQueueToken(eSyaEnterpriseContext db, DO_PatientAppointmentDetail obj)
        {
            var doctorCode = obj.DoctorID.ToString();

            string appType = "CA";
            if (obj.IsSponsored)
                appType = "SA";

            TimeSpan totalDuration = new TimeSpan();
            var appointmentSchedule = await GetDoctorScheduleByID(obj.BusinessKey, obj.DoctorID, obj.AppointmentDate);
            foreach (var s in appointmentSchedule)
            {
                if (s.FromTime <= obj.AppointmentFromTime && s.ToTime >= obj.AppointmentFromTime)
                {
                    totalDuration += obj.AppointmentFromTime.Subtract(s.FromTime);
                    break;
                }
                else
                    totalDuration += s.ToTime.Subtract(s.FromTime);
            }

            var totalIntervalinMinutes = totalDuration.Days * 24 * 60 +
                                         totalDuration.Hours * 60 +
                                         totalDuration.Minutes;
            var slotQueue = Math.Ceiling(totalIntervalinMinutes / 5.0) + 1;

            while (true)
            {
                var qTokenKey = doctorCode + "-" + slotQueue.ToString().PadLeft(2, '0');

                var q_exists = await db.GtEopapq.Where(w => w.BusinessKey == obj.BusinessKey && w.DoctorId == obj.DoctorID
                    && w.TokenDate.Date == obj.AppointmentDate.Date
                    && w.QueueTokenKey == qTokenKey
                    //&& w.QueueTokenKey.Substring(5, w.QueueTokenKey.Length - 5) == slotQueue.ToString().PadLeft(2, '0')
                    ).FirstOrDefaultAsync();
                if (q_exists != null)
                {
                    slotQueue++;
                    continue;
                }
                else
                {
                    //var qTokenKey = appType + doctorCode + slotQueue.ToString().PadLeft(2, '0');
                    obj.QueueTokenKey = qTokenKey;
                    break;
                }
            }

            return new DO_ReturnParameter
            {
                Success = true
            };
        }

        public async Task<List<DO_Doctor>> GetDoctorScheduleByID(int businessKey,
                 int doctorID, DateTime fromDate)
        {

            List<DO_Doctor> l_sc = new List<DO_Doctor>();
            var wk = fromDate.DayOfWeek.ToString();
            var wk_No = CommonMethod.GetWeekOfMonth(fromDate);

            var l_ds_1 = await _context.GtEsdocd
                    .GroupJoin(_context.GtEsdos1.Where(w => w.DayOfWeek.ToUpper() == wk.ToUpper()
                            && ((wk_No == 1 && w.Week1) || (wk_No == 2 && w.Week2)
                                || (wk_No == 3 && w.Week3) || (wk_No == 4 && w.Week4)
                                || (wk_No == 5 && w.Week5) || (wk_No == 6 && w.Week5))
                            && w.ActiveStatus),
                        d => d.DoctorId,
                        s => s.DoctorId,
                        (d, s) => new { d, s }).DefaultIfEmpty()
                    .SelectMany(d => d.s.DefaultIfEmpty(), (d, s) => new { d.d, s })
                    .GroupJoin(_context.GtEsdold.Where(w =>
                            w.ActiveStatus),
                        ds => ds.d.DoctorId,
                        l => l.DoctorId,
                        (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
                    .Where(w => w.ds.d.ActiveStatus && w.ds.d.DoctorId == doctorID
                            && !_context.GtEsdos2.Any(r => r.BusinessKey == businessKey
                                   && r.DoctorId == doctorID
                                   && r.ScheduleDate.Date == fromDate.Date
                                   && r.ActiveStatus))
                    .AsNoTracking()
                    .Select(x => new DO_Doctor
                    {
                        DoctorId = x.ds.d.DoctorId,
                        DoctorName = x.ds.d.DoctorName,
                        DoctorRemarks = x.ds.d.DoctorRemarks,
                        DayOfWeek = x.ds.s != null ? x.ds.s.DayOfWeek : "",
                        ScheduleDate = fromDate,
                        NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                        FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                        ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                        IsAvailable = x.ds.s != null ? true : false,
                        IsOnLeave = x.l != null ? x.l.ActiveStatus : false
                    }).OrderBy(o => o.FromTime).ToListAsync();


            var l_ds_2 = await _context.GtEsdocd
                  .GroupJoin(_context.GtEsdos2.Where(w => w.BusinessKey == businessKey
                          && w.ScheduleDate.Date == fromDate.Date
                          && w.ActiveStatus),
                      d => d.DoctorId,
                      s => s.DoctorId,
                      (d, s) => new { d, s = s.FirstOrDefault() }).DefaultIfEmpty()
                  .GroupJoin(_context.GtEsdold.Where(w =>
                          w.ActiveStatus),
                      ds => ds.d.DoctorId,
                      l => l.DoctorId,
                      (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
              .Where(w => w.ds.d.ActiveStatus && w.ds.d.DoctorId == doctorID)
              .AsNoTracking()
              .Select(x => new DO_Doctor
              {
                  DoctorId = x.ds.d.DoctorId,
                  DoctorName = x.ds.d.DoctorName,
                  DoctorRemarks = x.ds.d.DoctorRemarks,
                  DayOfWeek = "",
                  ScheduleDate = fromDate,
                  NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                  FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                  ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                  IsAvailable = x.ds.s != null ? true : false,
                  IsOnLeave = x.l != null ? x.l.ActiveStatus : false
              }).OrderBy(o => o.FromTime).ToListAsync();

            l_sc = l_ds_1.Union(l_ds_2).ToList();

            return l_sc;
        }


        public async Task<List<DO_Doctor>> GetDoctorScheduleByBKeyDoctorDate(int businessKey, int clinicType, int consultationType, int specialtyId, int doctorId, DateTime appointmentDate)
        {

            var wk = appointmentDate.DayOfWeek.ToString();
            var wk_No = CommonMethod.GetWeekOfMonth(appointmentDate);

            var l_ds_1 = await _context.GtEsdocd
                .GroupJoin(_context.GtEsdos1.Where(w => w.BusinessKey == businessKey
                            && w.DayOfWeek.ToUpper() == wk.ToUpper()
                            && w.ConsultationId == consultationType
                            && w.ClinicId == clinicType
                            && w.SpecialtyId == specialtyId
                            && ((wk_No == 1 && w.Week1) || (wk_No == 2 && w.Week2)
                                || (wk_No == 3 && w.Week3) || (wk_No == 4 && w.Week4)
                                || (wk_No == 5 && w.Week5) || (wk_No == 6 && w.Week5))
                            && w.ActiveStatus),
                    d => d.DoctorId,
                    s => s.DoctorId,
                    (d, s) => new { d, s }).DefaultIfEmpty()
                .SelectMany(d => d.s.DefaultIfEmpty(), (d, s) => new { d.d, s })
                .GroupJoin(_context.GtEsdold.Where(w =>
                            appointmentDate.Date >= w.OnLeaveFrom.Date
                            && appointmentDate.Date <= w.OnLeaveTill.Date
                            && w.ActiveStatus),
                    ds => ds.d.DoctorId,
                    l => l.DoctorId,
                    (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
                .Where(w => w.ds.d.ActiveStatus
                            && !_context.GtEsdos2.Any(r => r.BusinessKey == businessKey
                                   && r.ConsultationId == consultationType
                                   && r.ClinicId == clinicType
                                   && r.SpecialtyId == specialtyId
                                   && r.DoctorId == w.ds.d.DoctorId
                                   && r.ScheduleDate.Date == appointmentDate.Date
                                   && r.ActiveStatus))
                .AsNoTracking()
                .Select(x => new DO_Doctor
                {
                    DoctorId = x.ds.d.DoctorId,
                    DoctorName = x.ds.d.DoctorName,
                    DoctorRemarks = x.ds.d.DoctorRemarks,
                    DayOfWeek = x.ds.s != null ? x.ds.s.DayOfWeek : "",
                    ScheduleDate = appointmentDate,
                    NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                    FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                    ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                    IsAvailable = x.ds.s != null ? true : false,
                    IsOnLeave = x.l != null ? x.l.ActiveStatus : false
                }).ToListAsync();


            var l_ds_2 = await _context.GtEsdocd
               .GroupJoin(_context.GtEsdos2.Where(w => w.BusinessKey == businessKey
                           && w.ConsultationId == consultationType
                           && w.ClinicId == clinicType
                           && w.SpecialtyId == specialtyId
                           && w.ScheduleDate.Date == appointmentDate.Date
                           && w.ActiveStatus),
                   d => d.DoctorId,
                   s => s.DoctorId,
                   (d, s) => new { d, s }).DefaultIfEmpty()
               .SelectMany(d => d.s.DefaultIfEmpty(), (d, s) => new { d.d, s })
               .GroupJoin(_context.GtEsdold.Where(w =>
                           appointmentDate.Date >= w.OnLeaveFrom.Date
                           && appointmentDate.Date <= w.OnLeaveTill.Date
                           && w.ActiveStatus),
                   ds => ds.d.DoctorId,
                   l => l.DoctorId,
                   (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
               .Where(w => w.ds.d.ActiveStatus)
               .AsNoTracking()
               .Select(x => new DO_Doctor
               {
                   DoctorId = x.ds.d.DoctorId,
                   DoctorName = x.ds.d.DoctorName,
                   DoctorRemarks = x.ds.d.DoctorRemarks,
                   DayOfWeek = "",
                   ScheduleDate = appointmentDate,
                   NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                   FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                   ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                   IsAvailable = x.ds.s != null ? true : false,
                   IsOnLeave = x.l != null ? x.l.ActiveStatus : false
               }).ToListAsync();

            var l_ds = l_ds_1.Union(l_ds_2);

            return l_ds.Where(w => w.IsAvailable).ToList();

        }


        public async Task<List<DO_PatientAppointmentDetail>> GetAppointmentBookedSlotByDoctorDate(int businessKey, int specialtyId,
           int doctorId, DateTime scheduleDate)
        {
            try
            {
                var ds = await _context.GtEopaph
                    .Where(w => w.BusinessKey == businessKey && w.SpecialtyId == specialtyId
                                && w.DoctorId == doctorId
                                && w.AppointmentDate.Date == scheduleDate.Date
                                && w.AppointmentStatus != StatusVariables.Appointment.Cancelled
                                && !w.UnScheduleWorkOrder
                                && w.ActiveStatus)
                    .AsNoTracking()
                    .Select(r => new DO_PatientAppointmentDetail
                    {
                        AppointmentDate = r.AppointmentDate,
                        AppointmentFromTime = r.AppointmentFromTime,
                        Duration = r.Duration,
                        StartDate = r.AppointmentDate.Date.Add(r.AppointmentFromTime),
                        EndDate = r.AppointmentDate.Date.Add(r.AppointmentFromTime).AddMinutes(r.Duration),
                    }).ToListAsync();

                var ts = await _context.GtEopaps
                    .Where(w => w.BusinessKey == businessKey && w.SpecialtyId == specialtyId
                                && w.DoctorId == doctorId
                                && w.AppointmentDate.Date == scheduleDate.Date
                                && w.AppointmentStatus == "SL"
                                && w.CreatedOn.AddMinutes(2) > System.DateTime.Now
                                && w.ActiveStatus)
                    .AsNoTracking()
                    .Select(r => new DO_PatientAppointmentDetail
                    {
                        AppointmentDate = r.AppointmentDate,
                        AppointmentFromTime = r.AppointmentFromTime,
                        Duration = r.Duration,
                        StartDate = r.AppointmentDate.Date.Add(r.AppointmentFromTime),
                        EndDate = r.AppointmentDate.Date.Add(r.AppointmentFromTime).AddMinutes(r.Duration),
                    }).ToListAsync();

                var l_ds = ds.Union(ts);
                return l_ds.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DO_ApplicationCodes>> GetApplicationCodesByCodeType(int codeType)
        {
            try
            {
                var ds = _context.GtEcapcd
                    .Where(w => w.CodeType == codeType && w.ActiveStatus)
                    .Select(r => new DO_ApplicationCodes
                    {
                        ApplicationCode = r.ApplicationCode,
                        CodeDesc = r.CodeDesc
                    }).OrderBy(o => o.CodeDesc).ToListAsync();

                return await ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DO_Customer>> GetCustomer(int businessKey)
        {
            try
            {
                var ds = _context.GtEacscc
                    .Where(w => w.ActiveStatus
                     && _context.GtEacsbl.Any(l => l.BusinessKey == businessKey && l.CustomerId == w.CustomerId && l.ActiveStatus))
                    .Select(r => new DO_Customer
                    {
                        CustomerId = r.CustomerId,
                        CustomerName = r.CustomerName
                    }).OrderBy(o => o.CustomerName).ToListAsync();

                return await ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
