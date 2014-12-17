using System;
using System.Collections.Generic;
using System.Linq;

namespace Afterburn.Model
{
    public class AfterburnFile
    {
        public AfterburnFile()
        {
            this.Tasks = new List<Task>();
        }

        public IList<Task> Tasks { get; set; }
    }
}