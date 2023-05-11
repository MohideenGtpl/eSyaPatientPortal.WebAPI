using System;
using System.Collections.Generic;

namespace eSyaPatientPortal.DL.Entities
{
    public partial class GtEfoppp
    {
        public int BusinessKey { get; set; }
        public long RUhid { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? RegistrationChargesValidTill { get; set; }
        public bool ActiveStatus { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedTerminal { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedTerminal { get; set; }
    }
}
