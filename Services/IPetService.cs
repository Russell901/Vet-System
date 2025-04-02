using System.Collections.Generic;
using System.Threading.Tasks;
using Vet_System.Models;

namespace Vet_System.Services
{
    public interface IPetService
    {
        Task<IEnumerable<PetItem>> GetPetsAsync();
        Task<PetItem> GetPetByIdAsync(string petId);
        Task<IEnumerable<PetItem>> GetPetsByOwnerIdAsync(string ownerId);
    }
}
