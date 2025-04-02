using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Vet_System.Models;

namespace Vet_System.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly string _connectionString;
        private readonly DialogService _dialogService;

        public AppointmentService(string connectionString, DialogService dialogService = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _dialogService = dialogService;
        }

        public AppointmentService(string connectionString) : this(connectionString, null)
        {
        }

        public async Task<IEnumerable<AppointmentItem>> GetAppointmentsAsync()
        {
            var appointments = new List<AppointmentItem>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"
                   SELECT a.Id, p.Name AS pet_name, o.Name AS owner_name, a.DateTime,
                   a.Reason, a.Status
                   FROM Appointments a
                   INNER JOIN Pets p ON a.PetId = p.Id
                   INNER JOIN Owners o ON p.OwnerId = o.Id
                   ORDER BY a.DateTime DESC";

                using (var command = new MySqlCommand(query, connection))
                {
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        appointments.Add(new AppointmentItem(
                            reader["Id"].ToString(),
                            reader["pet_name"].ToString(),
                            reader["owner_name"].ToString(),
                            Convert.ToDateTime(reader["DateTime"]),
                            reader["Reason"].ToString(),
                            reader["Status"].ToString()
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync("Database Error",
                        $"Error retrieving appointments: {ex.Message}");
                }
                throw;
            }

            return appointments;
        }


        public async Task<AppointmentItem> CreateAppointmentAsync(AppointmentItem appointment)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                int petId = await GetPetIdByNameAsync(connection, appointment.PetName);

                if (petId == 0)
                {
                    throw new Exception($"Pet '{appointment.PetName}' not found");
                }

                if (string.IsNullOrEmpty(appointment.Id))
                {
                    appointment.Id = $"apt-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                }

                var query = @"
                    INSERT INTO Appointments
                    (Id, PetId, DateTime, Reason, Status)
                    VALUES
                    (@id, @petId, @dateTime, @reason, @status);";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", appointment.Id);
                    command.Parameters.AddWithValue("@petId", petId);
                    command.Parameters.AddWithValue("@dateTime", appointment.DateTime);
                    command.Parameters.AddWithValue("@reason", appointment.Reason);
                    command.Parameters.AddWithValue("@status", appointment.Status);

                    await command.ExecuteNonQueryAsync();
                }

                if (appointment.DateTime > DateTime.Now && appointment.Status == "scheduled")
                {
                    await UpdatePetNextAppointmentAsync(connection, petId, appointment.DateTime);
                }

                return appointment;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating appointment: {ex.Message}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync("Database Error",
                        $"Error creating appointment: {ex.Message}");
                }
                throw;
            }
        }


        public async Task<bool> UpdateAppointmentAsync(AppointmentItem appointment)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int petId = await GetPetIdByNameAsync(connection, appointment.PetName);

                if (petId == 0)
                {
                    throw new Exception($"Pet '{appointment.PetName}' not found");
                }

                string query = @"
                    UPDATE Appointments
                    SET PetId = @petId,
                        DateTime = @dateTime,
                        Reason = @reason,
                        Status = @status
                    WHERE Id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", appointment.Id);
                    command.Parameters.AddWithValue("@petId", petId);
                    command.Parameters.AddWithValue("@dateTime", appointment.DateTime);
                    command.Parameters.AddWithValue("@reason", appointment.Reason);
                    command.Parameters.AddWithValue("@status", appointment.Status);

                    int result = await command.ExecuteNonQueryAsync();

                    if (appointment.DateTime > DateTime.Now && appointment.Status == "scheduled")
                    {
                        await UpdatePetNextAppointmentAsync(connection, petId, appointment.DateTime);
                    }

                    return result > 0;
                }
            }
        }

        public async Task<bool> CancelAppointmentAsync(string appointmentId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int petId = 0;
                string petIdQuery = "SELECT PetId FROM Appointments WHERE Id = @id";
                using (var petIdCommand = new MySqlCommand(petIdQuery, connection))
                {
                    petIdCommand.Parameters.AddWithValue("@id", appointmentId);
                    var result = await petIdCommand.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        petId = Convert.ToInt32(result);
                    }
                }

                string query = @"
                    UPDATE Appointments
                    SET Status = 'cancelled'
                    WHERE Id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", appointmentId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0 && petId > 0)
                    {
                        await UpdatePetNextAppointmentAsync(connection, petId);
                    }

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteAppointmentAsync(string appointmentId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int petId = 0;
                string petIdQuery = "SELECT PetId FROM Appointments WHERE Id = @id";
                using (var petIdCommand = new MySqlCommand(petIdQuery, connection))
                {
                    petIdCommand.Parameters.AddWithValue("@id", appointmentId);
                    var result = await petIdCommand.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        petId = Convert.ToInt32(result);
                    }
                }

                string query = @"
                    DELETE FROM Appointments
                    WHERE Id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", appointmentId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Update the pet's next appointment since we just deleted one
                    if (rowsAffected > 0 && petId > 0)
                    {
                        await UpdatePetNextAppointmentAsync(connection, petId);
                    }

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<AppointmentItem> GetAppointmentByIdAsync(string appointmentId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT
                        a.Id,
                        a.DateTime,
                        a.Reason,
                        a.Status,
                        p.Name AS pet_name,
                        o.Name AS owner_name
                    FROM
                        Appointments a
                    INNER JOIN
                        Pets p ON a.PetId = p.Id
                    INNER JOIN
                        Owners o ON p.OwnerId = o.Id
                    WHERE
                        a.Id = @id;";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", appointmentId);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new AppointmentItem(
                        reader["Id"].ToString(),
                        reader["pet_name"].ToString(),
                        reader["owner_name"].ToString(),
                        reader.GetDateTime("DateTime"),
                        reader["Reason"].ToString(),
                        reader["Status"].ToString()
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving appointment: {ex.Message}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync("Database Error",
                        $"Error retrieving appointment: {ex.Message}");
                }
                throw;
            }
        }

        private async Task<int> GetPetIdByNameAsync(MySqlConnection connection, string petName)
        {
            try
            {
                var query = "SELECT Id FROM Pets WHERE Name = @petName LIMIT 1;";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@petName", petName);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                    return Convert.ToInt32(result);

                // If exact match not found, try case-insensitive match
                query = "SELECT Id FROM Pets WHERE LOWER(Name) = LOWER(@petName) LIMIT 1;";
                using var command2 = new MySqlCommand(query, connection);
                command2.Parameters.AddWithValue("@petName", petName);

                result = await command2.ExecuteScalarAsync();

                if (result != null)
                    return Convert.ToInt32(result);

                // If still not found, log which pets are available
                using var cmdListPets = new MySqlCommand("SELECT Id, Name FROM Pets", connection);
                using var reader = await cmdListPets.ExecuteReaderAsync();
                System.Diagnostics.Debug.WriteLine("Available pets in database:");
                while (await reader.ReadAsync())
                {
                    Debug.WriteLine($"- Pet ID: {reader["Id"]}, Name: {reader["Name"]}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPetIdByNameAsync: {ex.Message}");
                throw;
            }
        }

        private async Task UpdatePetNextAppointmentAsync(MySqlConnection connection, int petId, DateTime? specificDate = null)
        {
            try
            {
                object nextAppointment = DBNull.Value;

                if (specificDate.HasValue)
                {
                    nextAppointment = specificDate.Value;
                }
                else
                {
                    var nextApptQuery = @"
                        SELECT MIN(DateTime)
                        FROM Appointments
                        WHERE PetId = @petId
                          AND Status = 'scheduled'
                          AND DateTime > NOW();";

                    using var nextApptCmd = new MySqlCommand(nextApptQuery, connection);
                    nextApptCmd.Parameters.AddWithValue("@petId", petId);

                    var result = await nextApptCmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        nextAppointment = result;
                    }
                }

                var updateQuery = @"
                    UPDATE Pets
                    SET NextAppointment = @nextAppointment
                    WHERE Id = @petId;";

                using var updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@petId", petId);
                updateCmd.Parameters.AddWithValue("@nextAppointment", nextAppointment);

                await updateCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating pet's next appointment: {ex.Message}");
                // Just log the error but don't throw - this is secondary functionality
            }
        }

        private async Task UpdatePetNextAppointmentAsync(MySqlConnection connection, int petId)
        {
            await UpdatePetNextAppointmentAsync(connection, petId, null);
        }


    }
}