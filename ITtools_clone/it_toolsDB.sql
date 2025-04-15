DROP DATABASE IF EXISTS defaultdb;
CREATE DATABASE defaultdb;
USE defaultdb;

-- Bảng người dùng
CREATE TABLE Users (
    usid INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    premium BOOLEAN DEFAULT FALSE,
    is_admin BOOLEAN DEFAULT FALSE,
    request_premium Boolean default FALSE
);

-- Bảng công cụ (tools)
CREATE TABLE Tools (
    tid INT PRIMARY KEY AUTO_INCREMENT,
    tool_name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NOT NULL,
    enabled BOOLEAN DEFAULT TRUE,
    premium_required BOOLEAN DEFAULT FALSE,
    category_name VARCHAR(100) NOT NULL,
    file_name VARCHAR(100) NOT NULL);

-- Bảng công cụ yêu thích (Favorites)
CREATE TABLE Favorites (
    usid INT,
    tid INT,
    PRIMARY KEY (usid, tid),
    FOREIGN KEY (usid) REFERENCES Users(usid) ON DELETE CASCADE,
    FOREIGN KEY (tid) REFERENCES Tools(tid) ON DELETE CASCADE
);

INSERT INTO Tools (tid, tool_name, description, enabled, premium_required, category_name, file_name) VALUES
(1, 'Roman Converter', 'Convert numbers to and from Roman numerals.', 1, 0, 'Converter', 'RomanConverter.dll'),
(2, 'Random Port Generator', 'Generate random port numbers outside of the range of "known" ports (0-1023).', 1, 0, 'Networking', 'RandomPortGenerator.dll'),
(3, 'Basic Auth Generator', 'Generate HTTP Basic Authentication header value from username and password', 1, 0, 'Web', 'BasicAuthGenerator.dll'),
(4, 'URL Encoder Decoder', 'Encode or decode URLs and URL components', 1, 0, 'Web', 'EncodeDecodeURL.dll'),
(5, 'Hash Text', 'Generate hash values using various algorithms.', 1, 0, 'Crypto', 'HashText.dll'),
(6, 'Integer Base Converter', 'Convert integers between different bases (binary, decimal, hexadecimal)', 1, 0, 'Converter', 'IntegerBaseConverter.dll'),
(7, 'QR Code Generator', 'Generate QR codes with custom colors and error correction', 1, 0, 'Images and videos', 'QRCodeGenerator.dll'),
(8, 'Slugify String', 'Convert strings to URL-friendly slugs', 1, 0, 'Web', 'SlugifyString.dll'),
(9, 'NATO Phonetic Alphabet', 'Convert text to NATO phonetic alphabet', 1, 0, 'Converter', 'TextToNATOAlphabet.dll'),
(10, 'Token Generator', 'Generate random tokens with customizable characters.', 1, 0, 'Crypto', 'TokenGenerator.dll'),
(11, 'ULID Generator', 'Generate ULIDs (Universally Unique Lexicographically Sortable Identifiers)', 1, 0, 'Crypto', 'ULIDGenerator.dll'),
(12, 'WiFi QR Code Generator', 'Generate QR codes for WiFi network connection', 1, 0, 'Images and videos', 'WifiQRCodeGenerator.dll');