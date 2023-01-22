using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class TerminResponse
    {
        public Guid PatientId { get; set; }
        public string PVorname { get; set; }
        public string PName { get;set; }
        public Guid ArztId { get; set; }
        public string AVorname { get; set; }
        public string AName { get; set; }
        public DateTime Datum { get; set; }
        public string Behandlung { get; set; }
        public Guid Raum { get; set; }
        public int Raumnummer { get; set; }
        public string Anliegen { get; set; }
    }
}
