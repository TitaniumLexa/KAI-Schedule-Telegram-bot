using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KAI_Schedule.Entities;

namespace KAI_Schedule.Data
{
    public class ScheduleDbEntry
    {
        [Key]
        public int Id { get; set; }
        public string Group { get; set; }
        public DateTime LastUpdate { get; set; }
        public byte[] ScheduleBinary { get; set; }

        [NotMapped]
        public Schedule Schedule
        {
            get => Schedule.Deserialize(ScheduleBinary);
            set => ScheduleBinary = Schedule.Serialize(value);
        }
    }
}
