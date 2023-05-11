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
    public class BusinessLocationRepository : IBusinessLocationRepository
    {
        private eSyaEnterpriseContext _context { get; set; }
        public BusinessLocationRepository(eSyaEnterpriseContext context)
        {
            _context = context;
        }

        public async Task<List<DO_BusinessLocation>> GetBusinessLocation()
        {
            var bl = await _context.GtEcbsln
                        .Where(w => w.ActiveStatus)
                        .Select(r => new DO_BusinessLocation
                        {
                            BusinessKey = r.BusinessKey,
                            BusinessName = r.BusinessName
                        }).ToListAsync();

            return bl;
        }

        public async Task<DO_BusinessLocation> GetBusinessLocationDetails(int businessKey)
        {

            var bl = await _context.GtEcbsln
                .Join(_context.GtEcbssg,
                    l => l.BusinessId,
                    s => s.BusinessId,
                    (l, s) => new { l, s })
                .Where(w => w.l.BusinessKey == businessKey && w.l.ActiveStatus)
                .Select(r => new DO_BusinessLocation
                {
                    BusinessKey = r.l.BusinessKey,
                    BusinessName = r.l.BusinessName,
                    ISDCode = r.s.Isdcode
                }).FirstOrDefaultAsync();

            return bl;

        }
    }
}
