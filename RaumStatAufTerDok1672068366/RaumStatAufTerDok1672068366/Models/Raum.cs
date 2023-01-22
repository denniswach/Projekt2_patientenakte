using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class Raum
    {
        public Guid Id { get; set; }
        public Guid Krankenhaus { get; set; }
        public int Raumnummer { get; set; }
    }
}
