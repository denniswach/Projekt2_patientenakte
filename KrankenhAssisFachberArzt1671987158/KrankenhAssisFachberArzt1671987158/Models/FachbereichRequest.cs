using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhAssisFachberArzt1671987158.Models
{
    public class FachbereichRequest
    {
        public string Bezeichnung { get; set; }
        public Guid Krankenhaus { get; set; }
    }
}
