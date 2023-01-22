using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumStatAufTerDok1672068366.Models
{
    public class RaumRequest
    {
        public Guid? Krankenhaus { get; set; }
        public int? Raumnummer { get; set; }
    }
}
