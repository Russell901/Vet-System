using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace Vet_System.Services
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _serverConnectionString;
        private readonly DialogService _dialogService;

        public DatabaseInitializer(string connectionString, DialogService dialogService)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            var builder = new MySqlConnectionStringBuilder(connectionString);
            builder.Database = "";
            _serverConnectionString = builder.ConnectionString;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Current Date and Time (UTC): 2025-03-05 18:47:12");
                System.Diagnostics.Debug.WriteLine("Current User's Login: Russell901");

                await EnsureDatabaseExistsAsync();
                await CreateTablesAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(
                    "Database Initialization Error",
                    $"Failed to initialize database structure: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            using var connection = new MySqlConnection(_serverConnectionString);
            await connection.OpenAsync();

            var builder = new MySqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.Database;

            var checkDbCommand = new MySqlCommand(
                "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @dbName",
                connection);
            checkDbCommand.Parameters.AddWithValue("@dbName", databaseName);

            var dbExists = await checkDbCommand.ExecuteScalarAsync() != null;

            if (!dbExists)
            {
                var createDbCommand = new MySqlCommand(
                    $"CREATE DATABASE `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
                    connection);
                await createDbCommand.ExecuteNonQueryAsync();
            }
        }

        private async Task CreateTablesAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Create Owners table
            var createOwnersTable = @"
                CREATE TABLE IF NOT EXISTS Owners (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Phone VARCHAR(20),
                    Email VARCHAR(100),
                    Address TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    UNIQUE KEY uk_owner_name (Name)
                );";

            // Create Pets table
            var createPetsTable = @"
                CREATE TABLE IF NOT EXISTS Pets (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Species VARCHAR(50) NOT NULL,
                    Breed VARCHAR(100),
                    DateOfBirth DATE,
                    OwnerId INT,
                    ImageUrl VARCHAR(255),
                    NextAppointment DATETIME,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (OwnerId) REFERENCES Owners(Id)
                );";

            // Execute creation commands
            await ExecuteCommandAsync(connection, createOwnersTable);
            await ExecuteCommandAsync(connection, createPetsTable);

            // Insert sample data if needed
            await InsertSampleDataIfNeededAsync(connection);
        }

        private async Task ExecuteCommandAsync(MySqlConnection connection, string commandText)
        {
            using var command = new MySqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertSampleDataIfNeededAsync(MySqlConnection connection)
        {
            var checkCommand = new MySqlCommand("SELECT COUNT(*) FROM Owners", connection);
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Insert sample owner
                    var insertOwner = @"
                        INSERT INTO Owners (Name, Phone) 
                        VALUES (@name, @phone)";

                    var ownerCommand = new MySqlCommand(insertOwner, connection, transaction as MySqlTransaction);
                    ownerCommand.Parameters.AddWithValue("@name", "Tama Chi");
                    ownerCommand.Parameters.AddWithValue("@phone", "(555) 345-6789");
                    await ownerCommand.ExecuteNonQueryAsync();

                    var ownerId = (int)ownerCommand.LastInsertedId;

                    // Insert sample pet
                    var insertPet = @"
                        INSERT INTO Pets (Name, Species, Breed, DateOfBirth, OwnerId, ImageUrl)
                        VALUES (@name, @species, @breed, @dateOfBirth, @ownerId, @imageUrl)";

                    var petCommand = new MySqlCommand(insertPet, connection, transaction as MySqlTransaction);
                    petCommand.Parameters.AddWithValue("@name", "Buddy");
                    petCommand.Parameters.AddWithValue("@species", "dog");
                    petCommand.Parameters.AddWithValue("@breed", "Labrador");
                    petCommand.Parameters.AddWithValue("@dateOfBirth", "2020-03-04");
                    petCommand.Parameters.AddWithValue("@ownerId", ownerId);
                    petCommand.Parameters.AddWithValue("@imageUrl", "ms-appx:///Assets/Pets/buddy.jpg");
                    await petCommand.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}