using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKetteVerwVermaFunction1671915804.Models
{
    public class Verwaltungsmitarbeiter
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? Passhash { get; set; }
        public string Name { get; set; }
        public string Vorname { get; set; }
        public DateTime Geburtsdatum { get; set; }
        public string Adresse { get; set; }
        public string Email { get; set; }
        public Guid Verwaltung { get; set; }
    }
}
