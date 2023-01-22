using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    public class ImpfungRequest
    {
        public Guid PatientId { get; set; }
        public Guid ImpfstoffId { get; set; }
        public DateTime? Datum { get; set; } 
        public Guid? Arzt { get; set; }
        public int? Anzahl { get; set; }
    }
}
