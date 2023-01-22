using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class StationaererAufenthaltRequest
    {
        public Guid? PatientId { get; set; }
        public Guid? RaumId { get; set; }
        public DateTime? Aufnahmezeitpunkt { get; set; }
        public DateTime? Entlassungszeitraum { get; set; }
        public string? Hinweise { get; set; }
    }
}
