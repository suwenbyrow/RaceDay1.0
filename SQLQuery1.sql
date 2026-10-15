-- Delete the database if it already exists
USE master;
GO

IF DB_ID('ArtGalleryDB') IS NOT NULL
BEGIN
    DROP DATABASE ArtGalleryDB;
END
GO

-- Create the database
CREATE DATABASE ArtGalleryDB;
GO

-- Select the database
USE ArtGalleryDB;
GO

-- Create the Artist table
CREATE TABLE Artist (
    ArtistID INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    Surname VARCHAR(50) NOT NULL
);

-- Create the Genre table
CREATE TABLE Genre (
    GenreID INT PRIMARY KEY,
    Description VARCHAR(100) NOT NULL
);

-- Create the Artwork table
CREATE TABLE Artwork (
    ArtworkID INT PRIMARY KEY,
    GenreID INT NOT NULL,
    ArtistID INT NOT NULL,
    Title VARCHAR(100) NOT NULL,
    FOREIGN KEY (GenreID) REFERENCES Genre(GenreID),
    FOREIGN KEY (ArtistID) REFERENCES Artist(ArtistID)
);

-- Create the Exhibition table
CREATE TABLE Exhibition (
    ExhibitionID INT PRIMARY KEY,
    Description VARCHAR(100) NOT NULL
);

-- Create the Entry table
CREATE TABLE Entry (
    EntryID INT PRIMARY KEY,
    ArtworkID INT NOT NULL,
    ExhibitionID INT NOT NULL,
    FOREIGN KEY (ArtworkID) REFERENCES Artwork(ArtworkID),
    FOREIGN KEY (ExhibitionID) REFERENCES Exhibition(ExhibitionID)
);

-- Add the artists
INSERT INTO Artist VALUES
(1, 'Pablo', 'Picasso'),
(2, 'Vincent', 'Van Gogh'),
(3, 'Leonardo', 'Da Vinci'),
(4, 'Claude', 'Monet'),
(5, 'Salvador', 'Dali');

-- Add the genres
INSERT INTO Genre VALUES
(1, 'Abstract'),
(2, 'Landscape'),
(3, 'Portrait');

-- Add the artworks
INSERT INTO Artwork VALUES
(1, 1, 1, 'Blue Dreams'),
(2, 1, 1, 'Cubist Vision'),
(3, 2, 2, 'Starry Night'),
(4, 2, 2, 'Sunflower Field'),
(5, 3, 3, 'Mona Lisa'),
(6, 3, 3, 'Lady Portrait'),
(7, 2, 4, 'Water Lilies'),
(8, 2, 4, 'Morning Garden'),
(9, 1, 5, 'Melting Time'),
(10, 1, 5, 'Dreamscape'),
(11, 3, 1, 'Face of Mystery'),
(12, 2, 2, 'Golden Sunset'),
(13, 1, 3, 'Abstract Soul'),
(14, 3, 4, 'Royal Lady'),
(15, 2, 5, 'Desert View'),
(16, 1, 1, 'Modern Chaos'),
(17, 2, 2, 'Green Hills'),
(18, 3, 3, 'Ancient Smile'),
(19, 1, 4, 'Colour Burst'),
(20, 2, 5, 'Ocean Reflection');

-- Add the exhibitions
INSERT INTO Exhibition VALUES
(1, 'Summer Exhibition'),
(2, 'Modern Art Expo'),
(3, 'European Masters'),
(4, 'Portrait Showcase'),
(5, 'Nature Collection'),
(6, 'International Gallery'),
(7, 'Classic Art Event'),
(8, 'Creative Minds'),
(9, 'Fine Arts Festival'),
(10, 'Historical Display'),
(11, 'City Gallery Show'),
(12, 'Artists of the World'),
(13, 'Visual Wonders'),
(14, 'Golden Collection'),
(15, 'Masterpiece Showcase');

-- Add artworks to exhibitions
INSERT INTO Entry VALUES
(1, 1, 1),
(2, 2, 2),
(3, 3, 1),
(4, 4, 5),
(5, 5, 3),
(6, 6, 4),
(7, 7, 5),
(8, 8, 6),
(9, 9, 2),
(10, 10, 8),
(11, 11, 4),
(12, 12, 9),
(13, 13, 2),
(14, 14, 10),
(15, 15, 11),
(16, 16, 12),
(17, 17, 13),
(18, 18, 14),
(19, 19, 15),
(20, 20, 6),
(21, 1, 7),
(22, 1, 8);

-- Change the title of artwork 1
UPDATE Artwork
SET Title = 'Blue Dream Reimagined'
WHERE ArtworkID = 1;

-- Remove entry 22
DELETE FROM Entry
WHERE EntryID = 22;

-- Show artworks with their artist and genre
SELECT
    Artwork.Title,
    Artist.Name,
    Artist.Surname,
    Genre.Description AS Genre
FROM Artwork
INNER JOIN Artist
    ON Artwork.ArtistID = Artist.ArtistID
INNER JOIN Genre
    ON Artwork.GenreID = Genre.GenreID
ORDER BY
    Genre.Description ASC,
    Artwork.Title ASC;

-- Count artworks in each genre
SELECT
    Genre.Description,
    COUNT(*) AS TotalArtworks
FROM Artwork
INNER JOIN Genre
    ON Artwork.GenreID = Genre.GenreID
GROUP BY Genre.Description;

-- Find artists with more than 3 artworks
SELECT
    Artist.Name,
    Artist.Surname,
    COUNT(Artwork.ArtworkID) AS TotalArtworks
FROM Artist
INNER JOIN Artwork
    ON Artist.ArtistID = Artwork.ArtistID
GROUP BY
    Artist.Name,
    Artist.Surname
HAVING COUNT(Artwork.ArtworkID) > 3;

-- Show artworks and their exhibitions
SELECT
    Artwork.Title,
    Exhibition.Description AS Exhibition
FROM Entry
INNER JOIN Artwork
    ON Entry.ArtworkID = Artwork.ArtworkID
INNER JOIN Exhibition
    ON Entry.ExhibitionID = Exhibition.ExhibitionID
ORDER BY Artwork.Title;