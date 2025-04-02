using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Vet_System.Models;

namespace Vet_System.Services
{
    public class PetService : IPetService
    {
        private readonly string _connectionString;

        public PetService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<PetItem>> GetPetsAsync()
        {
            var pets = new List<PetItem>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT p.id, p.name, p.species, p.breed, p.dateOfBirth, 
                                p.ownerId, o.name as owner, p.imageUrl
                        FROM pets p
                        JOIN owners o ON p.ownerId = o.id
                        ORDER BY p.name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            string imageUrl = null;
                            if (reader.HasColumn("imageUrl") && !reader.IsDBNull(reader.GetOrdinal("imageUrl")))
                            {
                                imageUrl = reader["imageUrl"].ToString();
                            }

                            pets.Add(new PetItem(
                                reader["id"].ToString(),
                                reader["name"].ToString(),
                                reader["species"].ToString(),
                                reader["breed"].ToString(),
                                Convert.ToDateTime(reader["dateOfBirth"]),
                                reader["ownerId"].ToString(),
                                reader["owner"].ToString(),
                                imageUrl
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in GetPetsAsync: {ex.Message}");
                    throw; // Re-throw so the calling code can handle it
                }
            }

            return pets;
        }


        public async Task<PetItem> GetPetByIdAsync(string petId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT p.id, p.name, p.species, p.breed, p.dateOfBirth, 
                               p.ownerId, o.name as owner, p.imageUrl
                        FROM pets p
                        JOIN owners o ON p.ownerId = o.id
                        WHERE p.id = @petId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@petId", petId);
                        using var reader = await command.ExecuteReaderAsync();

                        if (await reader.ReadAsync())
                        {
                            string imageUrl = null;
                            if (reader.HasColumn("imageUrl") && !reader.IsDBNull(reader.GetOrdinal("imageUrl")))
                            {
                                imageUrl = reader["imageUrl"].ToString();
                            }

                            return new PetItem(
                                reader["id"].ToString(),
                                reader["name"].ToString(),
                                reader["species"].ToString(),
                                reader["breed"].ToString(),
                                Convert.ToDateTime(reader["dateOfBirth"]),
                                reader["ownerId"].ToString(),
                                reader["owner"].ToString(),
                                imageUrl
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in GetPetByIdAsync: {ex.Message}");
                    throw; // Re-throw so the calling code can handle it
                }
            }

            return null;
        }

        public async Task<IEnumerable<PetItem>> GetPetsByOwnerIdAsync(string ownerId)
        {
            var pets = new List<PetItem>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT p.id, p.name, p.species, p.breed, p.dateOfBirth, 
                               p.ownerId, o.name as owner, p.imageUrl
                        FROM pets p
                        JOIN owners o ON p.ownerId = o.id
                        ORDER BY p.name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ownerId", ownerId);
                        using var reader = await command.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            string imageUrl = null;
                            if (reader.HasColumn("imageUrl") && !reader.IsDBNull(reader.GetOrdinal("imageUrl")))
                            {
                                imageUrl = reader["imageUrl"].ToString();
                            }

                            pets.Add(new PetItem(
                                reader["id"].ToString(),
                                reader["name"].ToString(),
                                reader["species"].ToString(),
                                reader["breed"].ToString(),
                                Convert.ToDateTime(reader["dateOfBirth"]),
                                reader["ownerId"].ToString(),
                                reader["owner"].ToString(),
                                imageUrl
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in GetPetsByOwnerIdAsync: {ex.Message}");
                    throw; // Re-throw so the calling code can handle it
                }
            }
            return pets;
        }
    }

    public static class DbDataReaderExtensions
    {
        public static bool HasColumn(this DbDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
