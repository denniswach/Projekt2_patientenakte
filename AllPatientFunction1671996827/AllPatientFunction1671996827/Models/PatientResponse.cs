using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Functions
{
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Passhash { get; set; }
        public string Name { get; set; }
        public string Vorname { get; set; }
        public DateTime Geburtsdatum { get; set; }
        public string Geschlecht { get; set; }
        public string Adresse { get; set; }
        public int Telefonnummer { get; set; }
        public string Email { get; set; }
        public bool Dsgvo { get; set; }
        public DateTime Letzteaenderung { get; set; }
        public Guid Krankenkasse { get; set; }
        public string Kname { get; set; }
        public string Kstandort { get; set; }
        public bool Kisgesetzlich { get; set; }
    }
}
