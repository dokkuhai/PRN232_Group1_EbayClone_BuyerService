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
-- Bảng giỏ hàng chính
CREATE TABLE Cart (
    id INT AUTO_INCREMENT PRIMARY KEY,
    userId INT NULL,
    guestToken VARCHAR(100) NULL,
    createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_cart_user FOREIGN KEY (userId) REFERENCES User(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_cart (userId),
    UNIQUE KEY unique_guest_cart (guestToken)
);

-- Bảng sản phẩm trong giỏ
CREATE TABLE CartItem (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cartId INT NOT NULL,
    productId INT NOT NULL,
    quantity INT DEFAULT 1 CHECK (quantity > 0),
    
    CONSTRAINT fk_cartitem_cart FOREIGN KEY (cartId) REFERENCES Cart(id) ON DELETE CASCADE,
    CONSTRAINT fk_cartitem_product FOREIGN KEY (productId) REFERENCES Product(id)
);

ALTER TABLE Inventory
ADD CONSTRAINT uq_inventory_product UNIQUE (ProductId);

