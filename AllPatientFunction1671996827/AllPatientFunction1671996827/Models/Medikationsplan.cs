using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    public class Medikationsplan
    {
        public Guid PatientId { get; set; }
        public Guid MedikamentId { get; set; }
        public DateTime Von { get; set; }
        public DateTime Bis { get; set; }
        public string Staerke { get; set; }
        public string Hinweise { get; set; }
        public string Grund { get; set; }
        public Guid Arzt { get; set; }
    }
}
