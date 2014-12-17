using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterburn.Model
{
    public class DayUpdate
    {
        public Guid TaskId { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
    }
}
