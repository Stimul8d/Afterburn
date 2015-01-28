using System;
using System.Linq;

namespace Afterburn.Messages
{
    public class ConfirmDateChangedMessage
    {
        public ConfirmDateChangedMessage(DateTime from, DateTime to)
        {
            this.From = from;
            this.To = to;
        }

        public DateTime From { get; private set; }

        public DateTime To { get; private set; }
    }
}
