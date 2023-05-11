using System;
using System.Collections.Generic;

namespace eSyaPatientPortal.DL.Entities
{
    public partial class GtEfopvd
    {
        public GtEfopvd()
        {
            GtEfoppi = new HashSet<GtEfoppi>();
        }

        public int BusinessKey { get; set; }
        public long RUhid { get; set; }
        public long Opnumber { get; set; }
        public DateTime VisitDate { get; set; }
        public int NoOfVisit { get; set; }
        public string RegistrationType { get; set; }
        public string PatientClass { get; set; }
        public string VisitType { get; set; }
        public long AppointmentKey { get; set; }
        public bool IsVip { get; set; }
        public bool IsMlc { get; set; }
        public bool IsAntenatal { get; set; }
        public int? ReferredDr { get; set; }
        public bool ActiveStatus { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedTerminal { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedTerminal { get; set; }

        public virtual GtEfopnk GtEfopnk { get; set; }
        public virtual GtEfoppd GtEfoppd { get; set; }
        public virtual ICollection<GtEfoppi> GtEfoppi { get; set; }
    }
}
