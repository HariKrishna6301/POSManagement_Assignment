# POS Billing & Inventory Management System

Assignment implementation using C# / .NET 8, ASP.NET Core MVC, SQL Server, Dapper and stored procedures.

## Projects

- POS.Web - Presentation/UI + Web API webhook
- POS.Business - Business Logic Layer
- POS.Data - Data Access Layer
- POS.LowStockService - optional bonus background service

## Setup

1. Install .NET 8 SDK and SQL Server.
2. Open `Database/POSManagementDB.sql` in SSMS and execute it.
3. Update `POS.Web/appsettings.json` connection string.
4. Run:
   `dotnet restore`
   `dotnet build`
   `dotnet run --project POS.Web`
5. Open the URL printed by ASP.NET Core.

## Webhook

Endpoint:

POST `/api/payment/webhook`

Header:

`X-Webhook-Token: POS-DEMO-SECRET-123`

Body:

```json
{
  "orderId": "ORD-123",
  "status": "success",
  "amount": 3298.82,
  "transactionId": "TXN123"
}
```

The order is initially Pending. A successful webhook marks it Paid and reduces inventory in the SQL transaction. A failed webhook marks it Failed without reducing inventory.

## Assignment coverage

- Product CRUD
- Inventory and low-stock threshold
- POS search/cart/quantity/remove
- Flat cart discount
- Configurable GST
- Order creation and unique order number
- Payment webhook + token validation
- Webhook raw-payload logging
- Order history filters and expandable items
- Daily sales summary
- Print/save report as PDF through browser
- Bonus low-stock background worker

## Git

Recommended commits:
- `Initial solution structure`
- `Add database schema and stored procedures`
- `Implement product inventory`
- `Implement POS billing`
- `Implement payment webhook`
- `Add order history and reports`
- `Add low stock background service`
- `Add README and demo data`
