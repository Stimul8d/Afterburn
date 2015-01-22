using System;
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
    }
}