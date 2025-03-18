using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

                var query = @"
                    SELECT 
                        a.Id, 
                        a.DateTime, 
                        a.Reason,
                        a.Status,
                        p.Name AS PetName,
                        o.Name AS OwnerName
                    FROM 
                        Appointments a
                    INNER JOIN 
                        Pets p ON a.PetId = p.Id
                    INNER JOIN 
                        Owners o ON p.OwnerId = o.Id
                    ORDER BY 
                        a.DateTime;";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    appointments.Add(new AppointmentItem(
                        reader.GetString("Id"),
                        reader.GetString("PetName"),
                        reader.GetString("OwnerName"),
                        reader.GetDateTime("DateTime"),
                        reader.GetString("Reason"),
                        reader.GetString("Status")
                    ));
                }
            }
            catch (Exception ex)
            {
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

                // First, get the PetId based on the PetName
                int petId = await GetPetIdByNameAsync(connection, appointment.PetName);

                if (petId == 0)
                {
                    throw new Exception($"Pet '{appointment.PetName}' not found");
                }

                // Generate a unique ID if one is not provided
                if (string.IsNullOrEmpty(appointment.Id))
                {
                    appointment.Id = $"apt-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                }

                var query = @"
                    INSERT INTO Appointments (
                        Id, 
                        PetId,
                        DateTime,
                        Reason,
                        Status
                    ) VALUES (
                        @id,
                        @petId,
                        @dateTime,
                        @reason,
                        @status
                    );";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", appointment.Id);
                command.Parameters.AddWithValue("@petId", petId);
                command.Parameters.AddWithValue("@dateTime", appointment.DateTime);
                command.Parameters.AddWithValue("@reason", appointment.Reason);
                command.Parameters.AddWithValue("@status", appointment.Status);

                await command.ExecuteNonQueryAsync();

                // Also update the pet's NextAppointment field if the appointment is in the future
                if (appointment.DateTime > DateTime.Now && appointment.Status == "scheduled")
                {
                    await UpdatePetNextAppointmentAsync(connection, petId, appointment.DateTime);
                }

                return appointment;
            }
            catch (Exception ex)
            {
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
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // First, get the current appointment to check if anything changed
                var currentAppointment = await GetAppointmentByIdAsync(appointment.Id);
                if (currentAppointment == null)
                {
                    return false;
                }

                // Get the PetId based on the PetName
                int petId = await GetPetIdByNameAsync(connection, appointment.PetName);

                if (petId == 0)
                {
                    throw new Exception($"Pet '{appointment.PetName}' not found");
                }

                var query = @"
                    UPDATE Appointments SET
                        PetId = @petId,
                        DateTime = @dateTime,
                        Reason = @reason,
                        Status = @status
                    WHERE Id = @id;";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", appointment.Id);
                command.Parameters.AddWithValue("@petId", petId);
                command.Parameters.AddWithValue("@dateTime", appointment.DateTime);
                command.Parameters.AddWithValue("@reason", appointment.Reason);
                command.Parameters.AddWithValue("@status", appointment.Status);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                // If the appointment date/time or status changed, update the pet's NextAppointment
                if (currentAppointment.DateTime != appointment.DateTime ||
                    currentAppointment.Status != appointment.Status)
                {
                    await UpdatePetNextAppointmentAsync(connection, petId);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync("Database Error",
                        $"Error updating appointment: {ex.Message}");
                }
                throw;
            }
        }

        public async Task<bool> CancelAppointmentAsync(string appointmentId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // First, get the current appointment
                var currentAppointment = await GetAppointmentByIdAsync(appointmentId);
                if (currentAppointment == null)
                {
                    return false;
                }

                // Get the PetId based on the PetName
                int petId = await GetPetIdByNameAsync(connection, currentAppointment.PetName);

                var query = @"
                    UPDATE Appointments SET
                        Status = 'cancelled'
                    WHERE Id = @id;";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", appointmentId);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Update the pet's NextAppointment field since this one is cancelled
                if (rowsAffected > 0)
                {
                    await UpdatePetNextAppointmentAsync(connection, petId);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync("Database Error",
                        $"Error cancelling appointment: {ex.Message}");
                }
                throw;
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
                        p.Name AS PetName,
                        o.Name AS OwnerName
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
                        reader.GetString("Id"),
                        reader.GetString("PetName"),
                        reader.GetString("OwnerName"),
                        reader.GetDateTime("DateTime"),
                        reader.GetString("Reason"),
                        reader.GetString("Status")
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
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
            var query = "SELECT Id FROM Pets WHERE Name = @petName LIMIT 1;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@petName", petName);

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
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
            }
        }
    }
}