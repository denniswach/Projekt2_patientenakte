using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    
    public class Loeschanfrage
    {
        public Guid Id { get; set; }
        public DateTime Datum { get; set; }
        public string Loeschanfragetext { get; set; }
        public Guid Patient { get; set; }
    }
}
