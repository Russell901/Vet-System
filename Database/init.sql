CREATE DATABASE IF NOT EXISTS vet_system;
USE vet_system;

-- Owners table
CREATE TABLE IF NOT EXISTS Owners (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY uk_owner_name (Name)
);

-- Pets table
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
);

-- Seeding data
INSERT INTO Owners (Name, Phone) VALUES
('Tama Chiwaya', '(555) 345-6789');

INSERT INTO Pets (Name, Species, Breed, DateOfBirth, OwnerId, ImageUrl) VALUES
('Buddy', 'dog', 'German Shepherd', '2020-03-04', 1, 'ms-appx:///Assets/Pets/buddy.jpg');