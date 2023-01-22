using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class DokumentRequest
    {
        public Guid? Patient { get; set; }
        public string? Bezeichnung { get; set; }
        public DateTime? Datum { get; set; }
    }
}
