using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class Termin
    {
        public Guid PatientId { get; set; }
        public Guid ArztId { get; set; }
        public DateTime Datum { get; set; }
        public string Behandlung { get; set; }
        public Guid Raum { get; set; }
        public string Anliegen { get; set; }
    }
}
