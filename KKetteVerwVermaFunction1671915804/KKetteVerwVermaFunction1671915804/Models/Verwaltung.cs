using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKetteVerwVermaFunction1671915804.Models
{
    public class Verwaltung
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Adresse { get; set; }
        public Guid Krankenhauskette { get; set; }
    }
}
