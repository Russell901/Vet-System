using System;
using System.Windows.Input;

namespace Vet_System.Models
{
    public class PetItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string OwnerId { get; set; }
        public string Owner { get; set; }
        public string ImageUrl { get; set; }

        public PetItem
            (
                string id,
                string name,
                string species,
                string breed,
                DateTime dateOfBirth,
                string ownerId,
                string owner,
                string imageUrl
            )
        {
            Id = id;
            Name = name;
            Species = species;
            Breed = breed;
            DateOfBirth = dateOfBirth;
            OwnerId = ownerId;
            Owner = owner;
            ImageUrl = imageUrl;
        }
    }
}