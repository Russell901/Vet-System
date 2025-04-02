using MySql.Data.MySqlClient;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data;
using Vet_System.Models;

namespace Vet_System.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private XamlRoot _xamlRoot;
        private readonly DatabaseInitializer _initializer;
        private readonly DialogService _dialogService;
        private AppointmentService _appointmentService;

        public IAppointmentService AppointmentService =>
            _appointmentService ??= new AppointmentService(_connectionString, _dialogService);



        public static readonly string DefaultConnectionString =
            "Server=localhost;" +
            "Database=vet_system;" +
            "Uid=root;";
        private readonly OwnerInfo? ownerInfo;

        public DatabaseService(XamlRoot xamlRoot)
        {
            if (xamlRoot == null)
            {
                throw new ArgumentNullException(nameof(xamlRoot));
            }

            _connectionString = DefaultConnectionString;
            _xamlRoot = xamlRoot;
            _dialogService = new DialogService(xamlRoot);
            _initializer = new DatabaseInitializer(_connectionString, _dialogService);
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _initializer.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(
                    "Database Error",
                    $"Failed to initialize database: {ex.Message}");
                throw;
            }
        }

        public async Task<ObservableCollection<PetItem>> GetAllPetsAsync()
        {
            var pets = new ObservableCollection<PetItem>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT
                        p.*,
                        o.Name as OwnerName,
                        o.Phone as OwnerPhone,
                        (SELECT MIN(a.DateTime)
                         FROM Appointments a
                         WHERE a.PetId = p.Id AND a.DateTime > NOW()) AS NextAppointmentDate
                    FROM Pets p
                    LEFT JOIN Owners o ON p.OwnerId = o.Id
                    ORDER BY p.Name;";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    DateTime nextAppointment = DateTime.MinValue;
                    if (!reader.IsDBNull(reader.GetOrdinal("NextAppointmentDate")))
                    {
                        nextAppointment = reader.GetDateTime("NextAppointmentDate");
                    }

                    pets.Add(new PetItem(
                         reader.GetInt32("Id").ToString(),
                         reader.GetString("Name"),
                         reader.GetString("Species"),
                         reader.GetString("Breed"),
                         reader.GetDateTime("DateOfBirth"),
                         reader.GetInt32("OwnerId").ToString(),
                         reader.GetString("OwnerName"),
                         reader.GetString("ImageUrl"),
                         nextAppointment
                     ));
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Database Error",
                    $"Error retrieving pets: {ex.Message}");
                throw;
            }

            return pets;
        }

        public async Task AddPetAsync(PetItem pet)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var owner = ownerInfo ?? new OwnerInfo { Name = pet.Owner };

                var ownerId = await AddOrUpdateOwnerAsync(connection, transaction, owner);

                // Then add the pet
                var addPetQuery = @"
                    INSERT INTO Pets (
                        Name, Species, Breed, DateOfBirth,
                        OwnerId, ImageUrl, NextAppointment
                    ) VALUES (
                        @name, @species, @breed, @dateOfBirth,
                        @ownerId, @imageUrl, @nextAppointment
                    );";

                using var command = new MySqlCommand(addPetQuery, connection, transaction as MySqlTransaction);
                command.Parameters.AddWithValue("@name", pet.Name);
                command.Parameters.AddWithValue("@species", pet.Species);
                command.Parameters.AddWithValue("@breed", pet.Breed);
                command.Parameters.AddWithValue("@dateOfBirth", DateTime.Now.AddYears(-1)); // Default to 1 year old
                command.Parameters.AddWithValue("@ownerId", ownerId);
                command.Parameters.AddWithValue("@imageUrl", pet.ImageUrl.ToString());
                command.Parameters.AddWithValue("@nextAppointment", DBNull.Value);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _dialogService.ShowErrorAsync("Database Error",
                    $"Error adding pet: {ex.Message}");
                throw;
            }
        }

        private async Task<int> AddOrUpdateOwnerAsync(
            MySqlConnection connection,
            IDbTransaction transaction,
            OwnerInfo owner)
        {
            var query = @"
                INSERT INTO Owners (Name, Phone)
                VALUES (@name, @phone)
                ON DUPLICATE KEY UPDATE
                    Phone = VALUES(Phone);
                SELECT Id FROM Owners WHERE Name = @name;";

            using var command = new MySqlCommand(query, connection, transaction as MySqlTransaction);
            command.Parameters.AddWithValue("@name", owner.Name);
            command.Parameters.AddWithValue("@phone", owner.Phone);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}