using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhAssisFachberArzt1671987158.Models
{
    public class ArztRequest
    {
        public string? Username { get; set; }
        public string? Passhash { get; set; }
        public string Name { get; set; }
        public string Vorname { get; set; }
        public DateTime Geburtsdatum { get; set; }
        public string Geschlecht { get; set; }
        public string Adresse { get; set; }
        public string Email { get; set; }
        public string Spezialisierung { get; set; }
        public string Jobtitel { get; set; }
        public Guid Fachbereich { get; set; }
        public Guid Krankenhaus { get; set; }
    }
}
