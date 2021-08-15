using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Data
{
    public class Schedule
    {
        public string INTAKE { get; set; }
        public string MODID { get; set; }
        public string MODULE_NAME { get; set; }
        public string DAY { get; set; }
        public string LOCATION { get; set; }
        public string ROOM { get; set; }
        public string LECTID { get; set; }
        public string NAME { get; set; }
        public string SAMACCOUNTNAME { get; set; }
        public string DATESTAMP { get; set; }
        public string DATESTAMP_ISO { get; set; }
        public string TIME_FROM { get; set; }
        public string TIME_TO { get; set; }
        public DateTime TIME_FROM_ISO { get; set; }
        public DateTime TIME_TO_ISO { get; set; }
        public string GROUPING { get; set; }
        public string COLOR { get; set; }
        public string CLASS_CODE { get; set; }
    }


    public class Root
    {
        public List<Schedule> schedules { get; set; }
    }
}
