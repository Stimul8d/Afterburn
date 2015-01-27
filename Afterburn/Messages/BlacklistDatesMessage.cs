using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterburn.Messages
{
    public class BlacklistDatesMessage
    {
        public BlacklistDatesMessage(IEnumerable<DateTime> blacklistDates)
        {
            this.BlacklistDates = blacklistDates;
        }

        public IEnumerable<DateTime> BlacklistDates { get; private set; }
    }
}
