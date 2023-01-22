using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class DokumentResponse
    {
        public Guid Id { get; set; }
        public Guid Patient { get; set; }
        public string? Bezeichnung { get; set; }
        public string? DokumentName { get; set; }
        public string ContentType { get; set; }
        public DateTime Datum { get; set; }
        public string? DokumentLink { get; set; }
    }
}
