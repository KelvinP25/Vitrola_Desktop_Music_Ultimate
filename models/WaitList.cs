using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Vitrola_Desktop_Music_Ultimate.models
{
    public class WaitList
    {
        public int id { get; set; }
        public string artist { get; set; }
        public string titulo { get; set; }
        public string track { get; set; }
    }
}
