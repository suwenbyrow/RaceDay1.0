-- Drop and recreate the database cleanly
IF DB_ID('RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL
);

CREATE TABLE Organisers (
    OrganiserID INT PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE Participants (
    ParticipantID INT PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    RouteDescription VARCHAR(255),
    OrganiserID INT NOT NULL,
    FOREIGN KEY (OrganiserID) REFERENCES Organisers(OrganiserID)
);

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(50) NOT NULL,
    EventID INT NOT NULL,
    FOREIGN KEY (EventID) REFERENCES Events(EventID)
);

CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATE NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ParticipantID) REFERENCES Participants(ParticipantID),
    FOREIGN KEY (EventID) REFERENCES Events(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime VARCHAR(20),
    Position INT,
    FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);

-- ================================
-- Sample Data
-- ================================

USE RaceDay;
GO

-- Users (2 organisers + 2 participants)
INSERT INTO Users (FirstName, LastName, Email, Password) VALUES
('Alice', 'Morgan', 'alice.morgan@example.com', 'Password123'),
('Ben', 'Carter', 'ben.carter@example.com', 'Password123'),
('Chloe', 'Nguyen', 'chloe.nguyen@example.com', 'Password123'),
('David', 'Smith', 'david.smith@example.com', 'Password123');

-- Organisers (Alice = UserID 1, Ben = UserID 2)
INSERT INTO Organisers (OrganiserID, UserID) VALUES
(1, 1),
(2, 2);

-- Participants (Chloe = UserID 3, David = UserID 4)
INSERT INTO Participants (ParticipantID, UserID) VALUES
(1, 3),
(2, 4);

-- Events (3 events, created by the 2 organisers)
INSERT INTO Events (EventName, EventDate, Location, RouteDescription, OrganiserID) VALUES
('City Marathon', '2026-11-15', 'Cape Town', 'Coastal road route along the promenade', 1),
('Trail Run Challenge', '2026-10-05', 'Stellenbosch', 'Off-road forest trail with elevation gain', 1),
('Fun Run 5K', '2026-09-20', 'Johannesburg', 'Flat loop around the park', 2);

-- Categories (at least one per event)
INSERT INTO Categories (CategoryName, EventID) VALUES
('Full Marathon', 1),
('Half Marathon', 1),
('10km Trail', 2),
('21km Trail', 2),
('5km Fun Run', 3);

-- Enrolments (sample participants entering events)
INSERT INTO Enrolments (ParticipantID, EventID, CategoryID, EnrolmentDate) VALUES
(1, 1, 1, '2026-08-01'),
(2, 1, 2, '2026-08-02'),
(1, 3, 5, '2026-08-10'),
(2, 2, 3, '2026-08-12');

-- Results (sample results for some enrolments)
INSERT INTO Results (EnrolmentID, FinishTime, Position) VALUES
(1, '03:45:12', 15),
(2, '01:52:30', 8),
(3, '00:24:10', 3);
