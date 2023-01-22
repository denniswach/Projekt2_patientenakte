using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    public class PatientRequest
    {
        public string? Username { get; set; }
        public string? Passhash { get; set; }
        public string? Name { get; set; }
        public string? Vorname { get; set; }
        public DateTimeOffset? Geburtsdatum { get; set; }
        public string? Geschlecht { get; set; }
        public string? Adresse { get; set; }
        public int? Telefonnummer { get; set; }
        public string? Email { get; set; }
        public bool? Dsgvo { get; set; }
        public Guid? Krankenkasse { get; set; }
    }
}
