CREATE DATABASE IF NOT EXISTS CloneEbayDB;
USE CloneEbayDB;

CREATE TABLE User (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100),
    email VARCHAR(100) UNIQUE,
    password VARCHAR(255),
    role VARCHAR(20),
    avatarURL TEXT
);

CREATE TABLE Address (
    id INT AUTO_INCREMENT PRIMARY KEY,
    userId INT,
    fullName VARCHAR(100),
    phone VARCHAR(20),
    street VARCHAR(100),
    city VARCHAR(50),
    state VARCHAR(50),
    country VARCHAR(50),
    isDefault BOOLEAN,
    FOREIGN KEY (userId) REFERENCES User(id)
);

CREATE TABLE Category (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100)
);

CREATE TABLE Product (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255),
    description TEXT,
    price DECIMAL(10,2),
    images TEXT,
    categoryId INT,
    sellerId INT,
    isAuction BOOLEAN,
    auctionEndTime DATETIME,
    FOREIGN KEY (categoryId) REFERENCES Category(id),
    FOREIGN KEY (sellerId) REFERENCES User(id)
);

CREATE TABLE OrderTable (
    id INT AUTO_INCREMENT PRIMARY KEY,
    buyerId INT,
    addressId INT,
    orderDate DATETIME,
    totalPrice DECIMAL(10,2),
    status VARCHAR(20),
    FOREIGN KEY (buyerId) REFERENCES User(id),
    FOREIGN KEY (addressId) REFERENCES Address(id)
);

CREATE TABLE OrderItem (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    productId INT,
    quantity INT,
    unitPrice DECIMAL(10,2),
    FOREIGN KEY (orderId) REFERENCES OrderTable(id),
    FOREIGN KEY (productId) REFERENCES Product(id)
);

CREATE TABLE Payment (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    userId INT,
    amount DECIMAL(10,2),
    method VARCHAR(50),
    status VARCHAR(20),
    paidAt DATETIME,
    FOREIGN KEY (orderId) REFERENCES OrderTable(id),
    FOREIGN KEY (userId) REFERENCES User(id)
);

CREATE TABLE ShippingInfo (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    carrier VARCHAR(100),
    trackingNumber VARCHAR(100),
    status VARCHAR(50),
    estimatedArrival DATETIME,
    FOREIGN KEY (orderId) REFERENCES OrderTable(id)
);

CREATE TABLE ReturnRequest (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    userId INT,
    reason TEXT,
    status VARCHAR(20),
    createdAt DATETIME,
    FOREIGN KEY (orderId) REFERENCES OrderTable(id),
    FOREIGN KEY (userId) REFERENCES User(id)
);

CREATE TABLE Bid (
    id INT AUTO_INCREMENT PRIMARY KEY,
    productId INT,
    bidderId INT,
    amount DECIMAL(10,2),
    bidTime DATETIME,
    FOREIGN KEY (productId) REFERENCES Product(id),
    FOREIGN KEY (bidderId) REFERENCES User(id)
);

CREATE TABLE Review (
    id INT AUTO_INCREMENT PRIMARY KEY,
    productId INT,
    reviewerId INT,
    rating INT,
    comment TEXT,
    createdAt DATETIME,
    FOREIGN KEY (productId) REFERENCES Product(id),
    FOREIGN KEY (reviewerId) REFERENCES User(id)
);

CREATE TABLE Message (
    id INT AUTO_INCREMENT PRIMARY KEY,
    senderId INT,
    receiverId INT,
    content TEXT,
    timestamp DATETIME,
    FOREIGN KEY (senderId) REFERENCES User(id),
    FOREIGN KEY (receiverId) REFERENCES User(id)
);

CREATE TABLE Coupon (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50),
    discountPercent DECIMAL(5,2),
    startDate DATETIME,
    endDate DATETIME,
    maxUsage INT,
    productId INT,
    FOREIGN KEY (productId) REFERENCES Product(id)
);

CREATE TABLE Inventory (
    id INT AUTO_INCREMENT PRIMARY KEY,
    productId INT,
    quantity INT,
    lastUpdated DATETIME,
    FOREIGN KEY (productId) REFERENCES Product(id)
);

CREATE TABLE Feedback (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sellerId INT,
    averageRating DECIMAL(3,2),
    totalReviews INT,
    positiveRate DECIMAL(5,2),
    FOREIGN KEY (sellerId) REFERENCES User(id)
);

CREATE TABLE Dispute (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT,
    raisedBy INT,
    description TEXT,
    status VARCHAR(20),
    resolution TEXT,
    FOREIGN KEY (orderId) REFERENCES OrderTable(id),
    FOREIGN KEY (raisedBy) REFERENCES User(id)
);

CREATE TABLE Store (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sellerId INT,
    storeName VARCHAR(100),
    description TEXT,
    bannerImageURL TEXT,
    FOREIGN KEY (sellerId) REFERENCES User(id)
);

-- Sample data for User table
INSERT INTO `User` (`Username`, `Password`, `Email`, `FullName`, `Role`, `RegistrationDate`) VALUES
('johndoe', 'password123', 'john.doe@example.com', 'John Doe', 'Buyer', NOW()),
('janesmith', 'password456', 'jane.smith@example.com', 'Jane Smith', 'Seller', NOW());

-- Sample data for Address table
INSERT INTO Address (userId, fullName, phone, street, city, state, country, isDefault) VALUES
(1, 'John Doe', '+1-555-0100', '123 Main St', 'San Francisco', 'CA', 'USA', TRUE);

-- Sample data for Category table
INSERT INTO Category (name) VALUES
('Electronics'),
('Books'),
('Clothing');

-- Sample data for Product table
INSERT INTO Product (title, description, price, images, categoryId, sellerId, isAuction, auctionEndTime) VALUES
('Wireless Headphones', 'High-quality wireless headphones with noise cancellation', 89.99, 'https://example.com/headphones.jpg', 1, 2, FALSE, NULL),
('Programming Book', 'Learn C# and .NET Core development', 45.50, 'https://example.com/book.jpg', 2, 2, FALSE, NULL),
('T-Shirt', 'Cotton casual t-shirt', 19.99, 'https://example.com/tshirt.jpg', 3, 2, FALSE, NULL);

-- Sample data for OrderTable
INSERT INTO OrderTable (buyerId, addressId, orderDate, totalPrice, status) VALUES
(1, 1, DATE_SUB(NOW(), INTERVAL 5 DAY), 135.49, 'Delivered'),
(1, 1, DATE_SUB(NOW(), INTERVAL 2 DAY), 89.99, 'In Transit'),
(1, 1, NOW(), 19.99, 'Pending');

-- Sample data for OrderItem
INSERT INTO OrderItem (orderId, productId, quantity, unitPrice) VALUES
-- Order 1 items
(1, 1, 1, 89.99),
(1, 2, 1, 45.50),
-- Order 2 items
(2, 1, 1, 89.99),
-- Order 3 items
(3, 3, 1, 19.99);

-- Sample data for ShippingInfo
INSERT INTO ShippingInfo (orderId, carrier, trackingNumber, status, estimatedArrival) VALUES
-- Order 1 shipping history
(1, 'FedEx', 'FDX123456789', 'Delivered', DATE_SUB(NOW(), INTERVAL 2 DAY)),
(1, 'FedEx', 'FDX123456789', 'In Transit', DATE_SUB(NOW(), INTERVAL 3 DAY)),
(1, 'FedEx', 'FDX123456789', 'Shipped', DATE_SUB(NOW(), INTERVAL 4 DAY)),
-- Order 2 shipping
(2, 'UPS', 'UPS987654321', 'In Transit', DATE_ADD(NOW(), INTERVAL 1 DAY)),
(2, 'UPS', 'UPS987654321', 'Shipped', DATE_SUB(NOW(), INTERVAL 1 DAY));

-- Sample data for ReturnRequest
INSERT INTO ReturnRequest (orderId, userId, reason, status, createdAt) VALUES
(1, 1, 'Product damaged during shipping', 'Approved', DATE_SUB(NOW(), INTERVAL 1 DAY));

-- Sample data for Payment
INSERT INTO Payment (orderId, userId, amount, method, status, paidAt) VALUES
(1, 1, 135.49, 'Credit Card', 'Completed', DATE_SUB(NOW(), INTERVAL 5 DAY)),
(2, 1, 89.99, 'PayPal', 'Completed', DATE_SUB(NOW(), INTERVAL 2 DAY)),
(3, 1, 19.99, 'Credit Card', 'Pending', NULL);

