﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eSyaPatientPortal.DO
{
    public class DO_Doctor
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyDescription { get; set; }

        public string BusinessLocation { get; set; }

        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Gender { get; set; }
        public string Qualification { get; set; }
        public string DoctorComment { get; set; }
        public string DoctorRemarks { get; set; }

        public List<DO_DoctorSchedule> l_DoctorSchedule { get; set; }

        public string DayOfWeek { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public bool Week1 { get; set; }
        public bool Week2 { get; set; }
        public bool Week3 { get; set; }
        public bool Week4 { get; set; }
        public bool Week5 { get; set; }

        public int NumberofPatients { get; set; }

        public bool IsAvailable { get; set; }
        public bool IsOnLeave { get; set; }
    }

    public class DO_DoctorSchedule
    {
        public string DayOfWeek { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public bool Week1 { get; set; }
        public bool Week2 { get; set; }
        public bool Week3 { get; set; }
        public bool Week4 { get; set; }
        public bool Week5 { get; set; }

        public int NumberofPatients { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DO_DoctorTimeSlot
    {
        public string SlotType { get; set; }
        public TimeSpan TimeSlot { get; set; }
    }
}
