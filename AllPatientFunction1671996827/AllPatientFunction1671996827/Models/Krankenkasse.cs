using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPatientFunction1671996827.Models
{
    public class Krankenkasse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Standort { get; set; }
        public bool IsGesetzlich { get; set; }
    }
}
