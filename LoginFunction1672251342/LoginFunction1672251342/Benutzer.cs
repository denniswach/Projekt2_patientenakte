using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginFunction1672251342
{
    public class Benutzer
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Passhash { get; set; }
        public string Email { get; set; }
        public string Rolle { get; set; }
    }
}
