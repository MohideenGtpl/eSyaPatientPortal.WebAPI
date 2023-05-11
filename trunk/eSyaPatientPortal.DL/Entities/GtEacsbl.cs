using System;
using System.Collections.Generic;

namespace eSyaPatientPortal.DL.Entities
{
    public partial class GtEacsbl
    {
        public int CustomerId { get; set; }
        public int BusinessKey { get; set; }
        public bool ActiveStatus { get; set; }
        public string FormId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedTerminal { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedTerminal { get; set; }

        public virtual GtEacscc Customer { get; set; }
    }
}
