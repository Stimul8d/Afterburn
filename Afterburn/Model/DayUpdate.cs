using System;
using System.Linq;

namespace Afterburn.Model
{
    public class DayUpdate
    {
        public Guid TaskId { get; set; }

        public DateTime Date { get; set; }

        public double Hours { get; set; }
    }
}