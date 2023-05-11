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
    public class DoctorClinicRepository : IDoctorClinicRepository
    {
        private eSyaEnterpriseContext _context { get; set; }
        public DoctorClinicRepository(eSyaEnterpriseContext context)
        {
            _context = context;
        }

        public async Task<List<DO_Specialty>> GetSpecialty(int businessKey)
        {
            try
            {

                var ag = await _context.GtEsspcd
                    .Join(_context.GtEsspbl,
                        s => s.SpecialtyId,
                        b => b.SpecialtyId,
                        (s, b) => new { s, b })
                    .GroupJoin(_context.GtEssppa.Where(w => w.BusinessKey == businessKey && w.ParameterId == AppParameter.Specialty_IsTopSpecialty
                                    && w.ParmAction && w.ActiveStatus),
                        sb => new { sb.b.BusinessKey, sb.b.SpecialtyId },
                        p => new { p.BusinessKey, p.SpecialtyId },
                        (sb, p) => new { sb, p = p.FirstOrDefault() }).DefaultIfEmpty()
                    .Where(w => w.sb.b.BusinessKey == businessKey
                        && _context.GtEssppa.Any(p => p.BusinessKey == w.sb.b.BusinessKey && p.SpecialtyId == w.sb.b.SpecialtyId && p.ParameterId == AppParameter.Specialty_AllowConsulation
                        && p.ParmAction && p.ActiveStatus)
                        && (bool)w.sb.s.ActiveStatus && (bool)w.sb.b.ActiveStatus)
                    .Select(x => new DO_Specialty
                    {
                        SpecialtyId = x.sb.s.SpecialtyId,
                        SpecialtyDescription = x.sb.s.SpecialtyDesc,
                        IsTopSpecialty = x.p != null ? true : false,
                        MedicalIcon = x.sb.s.MedicalIcon
                    }).ToListAsync();

                return ag;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<DO_Doctor>> GetDoctorsBySearchText(int businessKey, string searchText)
        {
            try
            {

                var ag = await _context.GtEsdocd
                    .Join(_context.GtEsdobl,
                        d => d.DoctorId,
                        l => l.DoctorId,
                        (d, l) => new { d, l })
                    .Join(_context.GtEsdosp,
                        dl => new { dl.l.DoctorId },
                        s => new { s.DoctorId },
                        (dl, s) => new { dl, s })
                    .Join(_context.GtEsspcd,
                        dls => dls.s.SpecialtyId,
                        p => p.SpecialtyId,
                        (dls, p) => new { dls, p })
                    .Where(w => w.dls.dl.l.BusinessKey == businessKey && w.dls.dl.d.AllowConsultation
                         && w.dls.dl.d.DoctorName.ToUpper().Contains(searchText))
                    .Select(x => new DO_Doctor
                    {
                        DoctorId = x.dls.dl.d.DoctorId,
                        DoctorName = x.dls.dl.d.DoctorName,
                        SpecialtyId = x.dls.s.SpecialtyId,
                        SpecialtyDescription = x.p.SpecialtyDesc,
                        Qualification = x.dls.dl.d.Qualification,
                    }).Distinct().ToListAsync();

                return ag;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DO_Doctor>> GetDoctorsBySearchCriteria(int businessKey, int specialtyId, int doctorId, string gender, string preferedTimeSlot)
        {
            try
            {

                var bl = _context.GtEcbsln.Where(w => w.BusinessKey == businessKey).FirstOrDefault().BusinessName;

                var dm = await _context.GtEsdocd
                    .Join(_context.GtEsdobl,
                        d => d.DoctorId,
                        l => l.DoctorId,
                        (d, l) => new { d, l })
                    .Join(_context.GtEsdosp,
                        dl => new { dl.l.DoctorId },
                        s => new { s.DoctorId },
                        (dl, s) => new { dl, s })
                    .Join(_context.GtEsspcd,
                        dls => dls.s.SpecialtyId,
                        p => p.SpecialtyId,
                        (dls, p) => new { dls, p })
                    .Where(w => w.dls.dl.l.BusinessKey == businessKey
                         && w.dls.dl.d.AllowConsultation
                         && (w.dls.s.SpecialtyId == specialtyId || specialtyId == 0)
                         && (w.dls.dl.d.DoctorId == doctorId || doctorId == 0)
                         && (w.dls.dl.d.Gender == gender || gender == "A")
                         )
                    .Select(x => new DO_Doctor
                    {
                        DoctorId = x.dls.dl.d.DoctorId,
                        DoctorName = x.dls.dl.d.DoctorName,
                        SpecialtyId = x.dls.s.SpecialtyId,
                        SpecialtyDescription = x.p.SpecialtyDesc,
                        Qualification = x.dls.dl.d.Qualification,
                    }).Distinct().OrderBy(o => o.DoctorName).ToListAsync();

                var ds = dm
                    .Where(w => _context.GtEsdos1.Any(v => v.BusinessKey == businessKey
                                    && v.DoctorId == w.DoctorId && v.SpecialtyId == w.SpecialtyId && (bool)v.ActiveStatus
                                    && (preferedTimeSlot == "" || preferedTimeSlot == null
                                    || (preferedTimeSlot == "M"
                                        && v.ScheduleFromTime > new TimeSpan(00, 00, 01)
                                        && (v.ScheduleToTime <= new TimeSpan(12, 00, 00) || v.ScheduleFromTime < new TimeSpan(12, 00, 00)))
                                    || (preferedTimeSlot == "A"
                                        && (v.ScheduleFromTime >= new TimeSpan(12, 00, 00)
                                                && (v.ScheduleToTime <= new TimeSpan(18, 00, 00) || v.ScheduleFromTime < new TimeSpan(18, 00, 00)))
                                            || v.ScheduleFromTime < new TimeSpan(12, 00, 00))
                                    || (preferedTimeSlot == "E"
                                        && (v.ScheduleFromTime >= new TimeSpan(18, 00, 00)
                                                && (v.ScheduleToTime <= new TimeSpan(23, 59, 00) || v.ScheduleFromTime < new TimeSpan(23, 59, 00)))
                                             || v.ScheduleFromTime < new TimeSpan(12, 00, 00))
                                )))
                    .Select(x => new DO_Doctor
                    {
                        DoctorId = x.DoctorId,
                        DoctorName = x.DoctorName,
                        SpecialtyId = x.SpecialtyId,
                        SpecialtyDescription = x.SpecialtyDescription,
                        BusinessLocation = bl,
                        Qualification = x.Qualification,
                        l_DoctorSchedule = _context.GtEsdos1
                                            .Where(v => v.BusinessKey == businessKey
                                                        && v.DoctorId == x.DoctorId
                                                        && v.SpecialtyId == x.SpecialtyId
                                                        && (bool)v.ActiveStatus)
                                            .Select(v => new DO_DoctorSchedule
                                            {
                                                DayOfWeek = v.DayOfWeek,
                                                FromTime = v.ScheduleFromTime,
                                                ToTime = v.ScheduleToTime
                                            }).ToList()
                    }).ToList();


                return ds;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DO_DoctorSchedule>> GetDoctorScheduleTimeForAppointmentDate(int businessKey,
            int clinicType, int consultationType,
            int specialtyId, int doctorId, DateTime scheduleDate)
        {

            var wk = scheduleDate.DayOfWeek.ToString();
            var wk_No = CommonMethod.GetWeekOfMonth(scheduleDate);

            List<DO_DoctorSchedule> l_sc = new List<DO_DoctorSchedule>();
            var isLeave = _context.GtEsdold.Any(l => l.DoctorId == doctorId
                                   && scheduleDate.Date >= l.OnLeaveFrom.Date
                                   && scheduleDate.Date <= l.OnLeaveTill.Date
                                   && l.ActiveStatus);
            if (!isLeave)
            {

                var l_ds = await _context.GtEsdocd
                    .Join(_context.GtEsdosc.Where(w => w.BusinessKey == businessKey
                            && w.DoctorId == doctorId && w.SpecialtyId == specialtyId
                            && w.ClinicId == clinicType
                            && w.ConsultationId == consultationType && w.ActiveStatus
                            && w.ScheduleChangeDate.Date == scheduleDate.Date),
                        d => d.DoctorId,
                        s => s.DoctorId,
                        (d, s) => new { d, s })
                     .Select(x => new DO_DoctorSchedule
                     {
                         DayOfWeek = "",
                         ScheduleDate = scheduleDate,
                         NumberofPatients = 0,
                         FromTime = x.s.ScheduleFromTime,
                         ToTime = x.s.ScheduleToTime,
                         IsAvailable = true
                     }).ToListAsync();

                l_sc.AddRange(l_ds);

                if (l_ds.Count() <= 0)
                {

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
                         .GroupJoin(_context.GtEsdold.Where(w => w.DoctorId == doctorId
                                    && scheduleDate.Date >= w.OnLeaveFrom.Date
                                    && scheduleDate.Date <= w.OnLeaveTill.Date
                                    && w.ActiveStatus),
                            ds => ds.d.DoctorId,
                            l => l.DoctorId,
                            (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
                        .Where(w => w.ds.d.ActiveStatus && w.ds.d.DoctorId == doctorId
                                       && !_context.GtEsdos2.Any(r => r.BusinessKey == businessKey
                                       && r.ConsultationId == consultationType
                                       && r.ClinicId == clinicType
                                       && r.SpecialtyId == specialtyId
                                       && r.DoctorId == doctorId
                                       && r.ScheduleDate.Date == scheduleDate.Date
                                       && r.ActiveStatus))
                        .AsNoTracking()
                        .Select(x => new DO_DoctorSchedule
                        {
                            DayOfWeek = x.ds.s != null ? x.ds.s.DayOfWeek : "",
                            ScheduleDate = scheduleDate,
                            NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                            FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                            ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                            IsAvailable = x.ds.s != null ? true : false
                        }).ToListAsync();

                    var l_ds_2 = await _context.GtEsdocd
                        .GroupJoin(_context.GtEsdos2.Where(w => w.BusinessKey == businessKey
                               && w.ConsultationId == consultationType
                               && w.ClinicId == clinicType
                               && w.SpecialtyId == specialtyId
                               && w.ScheduleDate.Date == scheduleDate.Date
                               && w.ActiveStatus),
                            d => d.DoctorId,
                            s => s.DoctorId,
                            (d, s) => new { d, s }).DefaultIfEmpty()
                         .SelectMany(d => d.s.DefaultIfEmpty(), (d, s) => new { d.d, s })
                         .GroupJoin(_context.GtEsdold.Where(w => w.DoctorId == doctorId
                                    && scheduleDate.Date >= w.OnLeaveFrom.Date
                                    && scheduleDate.Date <= w.OnLeaveTill.Date
                                    && w.ActiveStatus),
                            ds => ds.d.DoctorId,
                            l => l.DoctorId,
                            (ds, l) => new { ds, l = l.FirstOrDefault() }).DefaultIfEmpty()
                        .Where(w => w.ds.d.ActiveStatus && w.ds.d.DoctorId == doctorId
                                       && !_context.GtEsdos2.Any(r => r.BusinessKey == businessKey
                                           && r.ConsultationId == consultationType
                                           && r.ClinicId == clinicType
                                           && r.SpecialtyId == specialtyId
                                           && r.DoctorId == doctorId
                                           && r.ScheduleDate.Date == scheduleDate.Date
                                           && r.ActiveStatus))
                        .AsNoTracking()
                        .Select(x => new DO_DoctorSchedule
                        {
                            DayOfWeek = "",
                            ScheduleDate = scheduleDate,
                            NumberofPatients = x.ds.s != null ? x.ds.s.NoOfPatients : 0,
                            FromTime = x.ds.s != null ? x.ds.s.ScheduleFromTime : new TimeSpan(9, 00, 00),
                            ToTime = x.ds.s != null ? x.ds.s.ScheduleToTime : new TimeSpan(18, 00, 00),
                            IsAvailable = x.ds.s != null ? true : false
                        }).ToListAsync();

                    l_sc.AddRange(l_ds_1);
                    l_sc.AddRange(l_ds_2);

                    return l_sc.Where(w => w.IsAvailable).ToList();
                }
            }
            return l_sc;
        }



        public async Task<List<DO_DoctorSchedule>> GetDoctorScheduleTime(int businessKey, int clinicType, int consultationType, int specialtyId, int doctorId, DateTime scheduleDate)
        {
            try
            {

                var wk = scheduleDate.DayOfWeek.ToString();
                var wk_No = CommonMethod.GetWeekOfMonth(scheduleDate);

                var l_ds = await _context.GtEsdos1
                    .Where(w => w.BusinessKey == businessKey
                         && w.SpecialtyId == specialtyId
                         && w.DoctorId == doctorId
                         && w.DayOfWeek == wk
                         && w.ConsultationId == consultationType
                         && w.ClinicId == clinicType
                         && ((wk_No == 1 && w.Week1) || (wk_No == 2 && w.Week2)
                            || (wk_No == 3 && w.Week3) || (wk_No == 4 && w.Week4)
                            || (wk_No == 5 && w.Week5) || (wk_No == 6 && w.Week5))
                         && (bool)w.ActiveStatus)
                    .AsNoTracking()
                    .Select(x => new DO_DoctorSchedule
                    {
                        DayOfWeek = x.DayOfWeek,
                        FromTime = x.ScheduleFromTime,
                        ToTime = x.ScheduleToTime,
                        Week1 = x.Week1,
                        Week2 = x.Week2,
                        Week3 = x.Week3,
                        Week4 = x.Week4,
                        Week5 = x.Week5
                    }).ToListAsync();

                List<DO_DoctorSchedule> l_sc = new List<DO_DoctorSchedule>();

                foreach (var s in l_ds)
                {
                    DO_DoctorSchedule sc = new DO_DoctorSchedule();
                    sc.ScheduleDate = scheduleDate;
                    sc.FromTime = s.FromTime;
                    sc.ToTime = s.ToTime;
                    sc.NumberofPatients = s.NumberofPatients;

                    var ds_changed = _context.GtEsdosc.Where(w => w.BusinessKey == businessKey
                            && w.DoctorId == doctorId && w.SpecialtyId == specialtyId
                            && w.ScheduleChangeDate.Date == scheduleDate.Date);
                    foreach (var c in ds_changed)
                    {
                        sc.FromTime = c.ScheduleFromTime;
                        sc.ToTime = c.ScheduleToTime;
                    }
                    l_sc.Add(sc);
                }

                var ds_ch = _context.GtEsdosc.Where(w => w.BusinessKey == businessKey
                    && w.DoctorId == doctorId && w.SpecialtyId == specialtyId
                    && w.ScheduleChangeDate.Date == scheduleDate.Date);
                foreach (var c in ds_ch)
                {
                    var dc = l_ds.FirstOrDefault();
                    if (dc == null)
                    {
                        var ch = new DO_DoctorSchedule
                        {
                            FromTime = c.ScheduleFromTime,
                            ToTime = c.ScheduleToTime,
                        };

                        l_sc.Add(ch);
                    }
                }

                l_sc = l_sc.Where(p => !_context.GtEsdold.Any(l => l.DoctorId == doctorId
                                           && scheduleDate.Date >= l.OnLeaveFrom.Date
                                           && scheduleDate.Date <= l.OnLeaveTill.Date
                                           && l.ActiveStatus)).ToList();

                return l_sc.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public async Task<List<DO_Doctor>> GetDoctorSchedule(int businessKey, int specialtyId, int? doctorId)
        {
            try
            {

                var ds = await _context.GtEsdocd
                    .Join(_context.GtEsdobl,
                        d => d.DoctorId,
                        l => l.DoctorId,
                        (d, l) => new { d, l })
                    .Join(_context.GtEsdosp,
                       dl => new { dl.l.DoctorId },
                        s => new { s.DoctorId },
                        (dl, s) => new { dl, s })
                    .Join(_context.GtEsspcd,
                        dls => dls.s.SpecialtyId,
                        p => p.SpecialtyId,
                        (dls, p) => new { dls, p })
                    .Join(_context.GtEsdos1,
                        dlsp => new { dlsp.dls.dl.l.BusinessKey, dlsp.dls.dl.d.DoctorId, dlsp.dls.s.SpecialtyId },
                        v => new { v.BusinessKey, v.DoctorId, v.SpecialtyId },
                        (dlsp, v) => new { dlsp, v })
                    .Where(w => w.dlsp.dls.dl.l.BusinessKey == businessKey
                         && (w.dlsp.dls.s.SpecialtyId == specialtyId || specialtyId == 0)
                         && (w.dlsp.dls.dl.d.DoctorId == doctorId || doctorId == 0)
                         && (bool)w.dlsp.dls.dl.d.ActiveStatus && (bool)w.dlsp.dls.dl.l.ActiveStatus
                         && (bool)w.dlsp.dls.s.ActiveStatus && (bool)w.v.ActiveStatus)
                    .AsNoTracking()
                    .Select(x => new DO_Doctor
                    {
                        SpecialtyId = x.v.SpecialtyId,
                        SpecialtyDescription = x.dlsp.p.SpecialtyDesc,
                        DoctorId = x.dlsp.dls.dl.d.DoctorId,
                        DoctorName = x.dlsp.dls.dl.d.DoctorName,
                        Qualification = x.dlsp.dls.dl.d.Qualification,
                        DayOfWeek = x.v.DayOfWeek,
                        FromTime = x.v.ScheduleFromTime,
                        ToTime = x.v.ScheduleToTime
                    }).OrderBy(o => o.DoctorName).ToListAsync();


                return ds;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DO_Doctor>> GetDoctorScheduleByDate(int businessKey, int specialtyId, int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {

                DateTime fromDate = startDate;

                var l_ds = await _context.GtEsdocd
                    .Join(_context.GtEsdobl,
                        d => d.DoctorId,
                        l => l.DoctorId,
                        (d, l) => new { d, l })
                    .Join(_context.GtEsdosp,
                        dl => new { dl.l.DoctorId },
                        s => new { s.DoctorId },
                        (dl, s) => new { dl, s })
                    .Join(_context.GtEsdos1,
                        dls => new { dls.dl.l.BusinessKey, dls.dl.d.DoctorId, dls.s.SpecialtyId },
                        v => new { v.BusinessKey, v.DoctorId, v.SpecialtyId },
                        (dls, v) => new { dls, v })
                    .Where(w => w.dls.dl.l.BusinessKey == businessKey
                         && w.dls.s.SpecialtyId == specialtyId
                         && w.dls.s.DoctorId == doctorId
                         && (bool)w.dls.dl.d.ActiveStatus && (bool)w.dls.dl.l.ActiveStatus
                         && (bool)w.dls.s.ActiveStatus && (bool)w.v.ActiveStatus)
                    .AsNoTracking()
                    .Select(x => new DO_Doctor
                    {
                        DoctorId = x.dls.dl.d.DoctorId,
                        DoctorName = x.dls.dl.d.DoctorName,
                        DayOfWeek = x.v.DayOfWeek,
                        FromTime = x.v.ScheduleFromTime,
                        ToTime = x.v.ScheduleToTime,
                        Week1 = x.v.Week1,
                        Week2 = x.v.Week2,
                        Week3 = x.v.Week3,
                        Week4 = x.v.Week4,
                        Week5 = x.v.Week5
                    }).ToListAsync();


                List<DO_Doctor> l_sc = new List<DO_Doctor>();
                while (startDate <= endDate)
                {
                    var wk = startDate.DayOfWeek.ToString();
                    var wk_No = CommonMethod.GetWeekOfMonth(startDate);

                    var ds = l_ds.Where(w => w.DayOfWeek == wk
                            && ((wk_No == 1 && w.Week1) || (wk_No == 2 && w.Week2)
                            || (wk_No == 3 && w.Week3) || (wk_No == 4 && w.Week4)
                            || (wk_No == 5 && w.Week5) || (wk_No == 6 && w.Week5))).AsQueryable();

                    foreach (var s in ds)
                    {
                        DO_Doctor sc = new DO_Doctor();
                        sc.DoctorId = s.DoctorId;
                        sc.DoctorName = s.DoctorName;
                        sc.ScheduleDate = startDate;
                        sc.FromTime = s.FromTime;
                        sc.ToTime = s.ToTime;
                        sc.NumberofPatients = s.NumberofPatients;

                        var ds_changed = _context.GtEsdosc.Where(w => w.BusinessKey == businessKey
                                && w.DoctorId == doctorId && w.SpecialtyId == specialtyId
                                && w.ScheduleChangeDate.Date == startDate.Date);
                        foreach (var c in ds_changed)
                        {
                            sc.FromTime = c.ScheduleFromTime;
                            sc.ToTime = c.ScheduleToTime;
                        }
                        l_sc.Add(sc);
                    }


                    // 
                    var ds_ch = _context.GtEsdosc.Where(w => w.BusinessKey == businessKey
                        && w.DoctorId == doctorId && w.SpecialtyId == specialtyId
                        && w.ScheduleChangeDate.Date == startDate);
                    foreach (var c in ds_ch)
                    {
                        var dc = ds.Where(w => w.DoctorId == c.DoctorId).FirstOrDefault();
                        if (dc == null)
                        {
                            var dm = _context.GtEsdocd.Where(w => w.DoctorId == c.DoctorId).FirstOrDefault();
                            var ch = new DO_Doctor
                            {
                                DoctorId = dm.DoctorId,
                                DoctorName = dm.DoctorName,
                                DayOfWeek = wk,
                                ScheduleDate = startDate,
                                FromTime = c.ScheduleFromTime,
                                ToTime = c.ScheduleToTime,
                                NumberofPatients = 0,
                            };

                            l_sc.Add(ch);
                        }
                    }


                    startDate = startDate.AddDays(1);
                }

                l_sc = l_sc.Where(p => !_context.GtEsdold.Any(l => l.DoctorId == p.DoctorId
                                           && fromDate.Date >= l.OnLeaveFrom.Date
                                           && fromDate.Date <= l.OnLeaveTill.Date
                                           && l.ActiveStatus)).ToList();

                return l_sc.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
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

    }
}
