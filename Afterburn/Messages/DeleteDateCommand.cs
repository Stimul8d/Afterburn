using System;
using System.Linq;

namespace Afterburn.Messages
{
    public class DeleteDateCommand
    {
        public DeleteDateCommand(DateTime date)
        {
            this.Date = date;
        }

        public DateTime Date { get; private set; }
    }
}
