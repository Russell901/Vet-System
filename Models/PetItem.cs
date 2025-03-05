using System;
using System.Windows.Input;

namespace Vet_System.Models
{
    public class PetItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string SpeciesBreed { get; set; } = string.Empty;
        public Uri ImageUrl { get; set; } = null!;
        public OwnerInfo Owner { get; set; } = null!;
        public string NextAppointmentDate { get; set; } = string.Empty;
        public ICommand ViewDetailsCommand { get; set; } = null!;
    }
}