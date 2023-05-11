using eSyaPatientPortal.DO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eSyaPatientPortal.IF
{
    public interface IBusinessLocationRepository
    {
        Task<List<DO_BusinessLocation>> GetBusinessLocation();

        Task<DO_BusinessLocation> GetBusinessLocationDetails(int businessKey);
    }
}
