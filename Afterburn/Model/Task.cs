using System;
using System.Collections.Generic;

namespace Afterburn.Model
{
    public class Task
    {
        public Task()
        {
            this.Updates = new List<DayUpdate>();
        }

        public Guid Id { get; set; }

        public string Feature { get; set; }

        public string Name { get; set; }

        public double Hours { get; set; }

        public List<DayUpdate> Updates { get; set; }
    }
}