using System;
using System.Collections.Generic;
using System.Linq;

namespace Afterburn.Model
{
    public class Update
    {
        public DateTime Date { get; set; }
        public IEnumerable<double> Hours { get; set; }
    }
}
