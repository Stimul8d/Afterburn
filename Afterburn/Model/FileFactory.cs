using System;
using System.Collections.Generic;
using System.Linq;
using Afterburn.ViewModel;

namespace Afterburn.Model
{
    public class FileFactory
    {
        public AfterburnFile Create(MainViewModel mvm)
        {
            var file = new AfterburnFile();
            foreach (var task in mvm.Tasks)
            {
                var fileTask = new Task
                {
                    Id = task.Id,
                    Feature = task.Feature,
                    Hours = task.Hours,
                    Name = task.Name,
                    Reference = task.Reference
                };

                file.Tasks.Add(fileTask);
                foreach (var update in task.Updates)
                {
                    fileTask.Updates.Add(new DayUpdate
                    {
                        Date = update.Date,
                        Hours = update.Hours
                    });  
                }
            }

            return file;
        }

        private List<DayUpdate> GetDayUpdates(MainViewModel mvm)
        {
            var dateTotals = mvm.Tasks.SelectMany(t => t.Updates)
                                  .GroupBy(t => t.Date.Date)
                                  .Select(g => new DayUpdate
                                  {
                                      Date = g.Key,
                                      Hours = g.Sum(x => x.Hours)
                                  })
                                  .OrderBy(x => x.Date)
                                  .ToList();
            return dateTotals;
        }
    }
}
