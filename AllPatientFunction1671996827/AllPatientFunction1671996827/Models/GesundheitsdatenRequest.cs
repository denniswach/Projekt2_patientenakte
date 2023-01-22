using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    public class GesundheitsdatenRequest
    {
        public string Vorerkrankungen { get; set; }
        public string Allergien { get; set; }
        public double? GewichtKG { get; set; }
        public int? GroeßeCM { get; set; }
        public string Ops { get; set; }
        public bool? Patientenverfuegung { get; set; }
        public bool? Vorsorgevollmacht { get; set; }
        public Guid Patient { get; set; }
    }
}
