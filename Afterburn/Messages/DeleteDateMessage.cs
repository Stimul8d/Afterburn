using System;
using System.Linq;

namespace Afterburn.Messages
{
    public class DeleteDateMessage
    {
        public DeleteDateMessage(DateTime date)
        {
            this.Date = date;
        }

        public DateTime Date { get; private set; }
    }
}
