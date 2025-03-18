using System;

namespace Vet_System.Models
{
    public class AppointmentItem
    {
        public string Id { get; set; }
        public string PetName { get; set; }
        public string OwnerName { get; set; }
        public DateTime DateTime { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }

        public AppointmentItem(
            string id,
            string petName,
            string ownerName,
            DateTime dateTime,
            string reason,
            string status = "scheduled")
        {
            Id = id;
            PetName = petName;
            OwnerName = ownerName;
            DateTime = dateTime;
            Reason = reason;
            Status = status;
        }
    }
}