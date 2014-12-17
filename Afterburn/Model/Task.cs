using System;
using System.Collections.Generic;
namespace Afterburn.Model
{
    public class Task
    {
        public Guid Id { get; set; }

        public string Reference { get; set; }

        public string Feature { get; set; }

        public string Name { get; set; }

        public double Hours { get; set; }

        public List<DayUpdate> Updates { get; set; }

        public Task()
        {
            Updates = new List<DayUpdate>();
        }
    }
}