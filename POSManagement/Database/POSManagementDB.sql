
IF DB_ID('POSManagementDB') IS NULL
BEGIN
    CREATE DATABASE POSManagementDB;
END
GO

USE POSManagementDB;
GO


IF TYPE_ID('dbo.OrderItemType') IS NULL
BEGIN
    EXEC('
    CREATE TYPE dbo.OrderItemType AS TABLE
    (
        ProductId INT NOT NULL,
        ProductName NVARCHAR(150) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL
    )');
END
GO


IF OBJECT_ID('dbo.OrderItems') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.WebhookLogs') IS NOT NULL DROP TABLE dbo.WebhookLogs;
IF OBJECT_ID('dbo.Orders') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Products') IS NOT NULL DROP TABLE dbo.Products;
GO

CREATE TABLE Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    SKU NVARCHAR(50) NOT NULL UNIQUE,
    Category NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL,
    LowStockThreshold INT NOT NULL DEFAULT(5),
    CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
    UpdatedAt DATETIME2 NULL
);
GO

CREATE TABLE Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    Subtotal DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(18,2) NOT NULL,
    Tax DECIMAL(18,2) NOT NULL,
    GrandTotal DECIMAL(18,2) NOT NULL,
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT('Pending'),
    CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
    PaidAt DATETIME2 NULL,
    TransactionId NVARCHAR(100) NULL
);
GO

CREATE TABLE OrderItems
(
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY(OrderId)
        REFERENCES Orders(OrderId),

    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY(ProductId)
        REFERENCES Products(ProductId)
);
GO

CREATE TABLE WebhookLogs
(
    WebhookLogId INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NULL,
    Status NVARCHAR(20) NULL,
    RawPayload NVARCHAR(MAX) NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
    IsValid BIT NOT NULL
);
GO



CREATE OR ALTER PROCEDURE dbo.sp_Product_GetAll
AS
BEGIN
    SELECT *
    FROM Products
    ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Search
    @Search NVARCHAR(150)
AS
BEGIN
    SELECT *
    FROM Products
    WHERE Name LIKE '%' + @Search + '%'
       OR SKU LIKE '%' + @Search + '%'
    ORDER BY Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Insert
    @Name NVARCHAR(150),
    @SKU NVARCHAR(50),
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @StockQuantity INT,
    @LowStockThreshold INT
AS
BEGIN
    INSERT INTO Products
    (
        Name,
        SKU,
        Category,
        Price,
        StockQuantity,
        LowStockThreshold
    )
    VALUES
    (
        @Name,
        @SKU,
        @Category,
        @Price,
        @StockQuantity,
        @LowStockThreshold
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Update
    @ProductId INT,
    @Name NVARCHAR(150),
    @SKU NVARCHAR(50),
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @StockQuantity INT,
    @LowStockThreshold INT
AS
BEGIN
    UPDATE Products
    SET Name = @Name,
        SKU = @SKU,
        Category = @Category,
        Price = @Price,
        StockQuantity = @StockQuantity,
        LowStockThreshold = @LowStockThreshold,
        UpdatedAt = GETDATE()
    WHERE ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Delete
    @ProductId INT
AS
BEGIN
    DELETE FROM Products
    WHERE ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLowStockProducts
AS
BEGIN
    SELECT *
    FROM Products
    WHERE StockQuantity <= LowStockThreshold
    ORDER BY StockQuantity;
END;
GO



CREATE OR ALTER PROCEDURE dbo.sp_CreateOrder
    @OrderNumber NVARCHAR(50),
    @Subtotal DECIMAL(18,2),
    @Discount DECIMAL(18,2),
    @Tax DECIMAL(18,2),
    @GrandTotal DECIMAL(18,2),
    @Items dbo.OrderItemType READONLY
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    IF EXISTS
    (
        SELECT 1
        FROM @Items i
        LEFT JOIN Products p
            ON p.ProductId = i.ProductId
        WHERE p.ProductId IS NULL
           OR i.Quantity <= 0
           OR p.StockQuantity < i.Quantity
    )
    BEGIN
        ROLLBACK;
        THROW 50001,
        'One or more items are unavailable or have insufficient stock.',
        1;
    END

    INSERT INTO Orders
    (
        OrderNumber,
        Subtotal,
        Discount,
        Tax,
        GrandTotal,
        PaymentStatus
    )
    VALUES
    (
        @OrderNumber,
        @Subtotal,
        @Discount,
        @Tax,
        @GrandTotal,
        'Pending'
    );

    DECLARE @OrderId INT = SCOPE_IDENTITY();

    INSERT INTO OrderItems
    (
        OrderId,
        ProductId,
        ProductName,
        Quantity,
        UnitPrice,
        LineTotal
    )
    SELECT
        @OrderId,
        ProductId,
        ProductName,
        Quantity,
        UnitPrice,
        LineTotal
    FROM @Items;

    COMMIT;

    SELECT @OrderId AS OrderId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ProcessPayment
    @OrderNumber NVARCHAR(50),
    @Status NVARCHAR(20),
    @Amount DECIMAL(18,2),
    @TransactionId NVARCHAR(100)
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    DECLARE
        @OrderId INT,
        @GrandTotal DECIMAL(18,2),
        @CurrentStatus NVARCHAR(20);

    SELECT
        @OrderId = OrderId,
        @GrandTotal = GrandTotal,
        @CurrentStatus = PaymentStatus
    FROM Orders
    WHERE OrderNumber = @OrderNumber;

    IF @OrderId IS NULL
    BEGIN
        ROLLBACK;
        SELECT CAST(0 AS BIT);
        RETURN;
    END

    IF @CurrentStatus = 'Paid'
    BEGIN
        COMMIT;
        SELECT CAST(1 AS BIT);
        RETURN;
    END

    IF ABS(@GrandTotal - @Amount) > 0.01
    BEGIN
        ROLLBACK;
        SELECT CAST(0 AS BIT);
        RETURN;
    END

    IF @Status = 'success'
    BEGIN

        UPDATE Orders
        SET PaymentStatus = 'Paid',
            PaidAt = GETDATE(),
            TransactionId = @TransactionId
        WHERE OrderId = @OrderId;

        UPDATE p
        SET p.StockQuantity = p.StockQuantity - oi.Quantity
        FROM Products p
        INNER JOIN OrderItems oi
            ON p.ProductId = oi.ProductId
        WHERE oi.OrderId = @OrderId;

    END
    ELSE IF @Status = 'failed'
    BEGIN

        UPDATE Orders
        SET PaymentStatus = 'Failed',
            TransactionId = @TransactionId
        WHERE OrderId = @OrderId;

    END
    ELSE
    BEGIN
        ROLLBACK;
        SELECT CAST(0 AS BIT);
        RETURN;
    END

    COMMIT;
    SELECT CAST(1 AS BIT);
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_GetOrderHistory
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @PaymentStatus NVARCHAR(20) = NULL
AS
BEGIN

    SELECT *
    FROM Orders
    WHERE (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt < DATEADD(DAY,1,@ToDate))
      AND (@PaymentStatus IS NULL OR PaymentStatus = @PaymentStatus)
    ORDER BY CreatedAt DESC;

END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetOrderItems
    @OrderId INT
AS
BEGIN
    SELECT *
    FROM OrderItems
    WHERE OrderId = @OrderId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetOrderByNumber
    @OrderNumber NVARCHAR(50)
AS
BEGIN
    SELECT TOP 1 *
    FROM Orders
    WHERE OrderNumber = @OrderNumber;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDailySalesSummary
    @SaleDate DATE
AS
BEGIN

    SELECT
        COUNT(*) AS TotalOrders,
        COALESCE(SUM(GrandTotal),0) AS TotalRevenue,
        COALESCE(SUM(Discount),0) AS TotalDiscount
    FROM Orders
    WHERE PaymentStatus = 'Paid'
      AND CAST(CreatedAt AS DATE) = @SaleDate;

END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_WebhookLog_Insert
    @OrderNumber NVARCHAR(50),
    @Status NVARCHAR(20),
    @RawPayload NVARCHAR(MAX),
    @IsValid BIT
AS
BEGIN

    INSERT INTO WebhookLogs
    (
        OrderNumber,
        Status,
        RawPayload,
        IsValid
    )
    VALUES
    (
        @OrderNumber,
        @Status,
        @RawPayload,
        @IsValid
    );

END;
GO

INSERT INTO Products
(
    Name,
    SKU,
    Category,
    Price,
    StockQuantity,
    LowStockThreshold
)
VALUES
('Wireless Mouse','MOU001','Electronics',799,20,5),
('Mechanical Keyboard','KEY001','Electronics',2499,10,3),
('USB-C Cable','CAB001','Accessories',499,4,5),
('Notebook','NOT001','Stationery',120,50,10);
GO

SELECT * FROM Products;
GO