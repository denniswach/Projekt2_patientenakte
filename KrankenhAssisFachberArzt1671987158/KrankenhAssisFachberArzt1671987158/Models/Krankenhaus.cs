using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhAssisFachberArzt1671987158.Models
{
    public class Krankenhaus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Adresse { get; set; }
        public Guid Krankenhauskette { get; set; }
    }
}
