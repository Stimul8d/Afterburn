using System;
using System.Collections.Generic;
using System.Linq;

namespace Afterburn.Model
{
    public class AfterburnFile
    {
        public IList<Task> Tasks { get; set; }
        public AfterburnFile()
        {
            Tasks = new List<Task>();
        }
    }
}
