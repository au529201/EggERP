IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906105856_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906105856_InitialCreate', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Businesses] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [LegalName] nvarchar(max) NULL,
        [TaxIdentificationNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [AddressLine1] nvarchar(max) NULL,
        [AddressLine2] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [Province] nvarchar(max) NULL,
        [PostalCode] nvarchar(max) NULL,
        [CountryCode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Businesses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Expenses] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [ExpenseDateUtc] datetime2 NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ReferenceNumber] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Inventories] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [QuantityOnHand] decimal(18,2) NOT NULL,
        [ReorderLevel] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Inventories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NULL,
        [PurchaseId] uniqueidentifier NULL,
        [PaymentDateUtc] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ReferenceNumber] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SKU] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CostPrice] decimal(18,2) NOT NULL,
        [SellingPrice] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [PurchaseItems] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PurchaseItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Purchases] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [SupplierId] uniqueidentifier NULL,
        [PurchaseDateUtc] datetime2 NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Purchases] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [SaleItems] (
        [Id] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SaleItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Sales] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [SaleDateUtc] datetime2 NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Sales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ContactPerson] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    CREATE TABLE [TaxRates] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Rate] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_TaxRates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906144139_InitialCoreEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906144139_InitialCoreEntities', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    DROP TABLE [Payments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Sales] ADD [PaymentMethod] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Sales] ADD [PaymentSource] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Sales] ADD [ReferenceNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Purchases] ADD [PaymentMethod] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Purchases] ADD [PaymentSource] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Purchases] ADD [ReferenceNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    ALTER TABLE [Expenses] ADD [PaymentSource] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260911040347_AddPaymentSourceToSalesPurchasesExpenses', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912042721_AddIdentitySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260912042721_AddIdentitySchema', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914145237_MakeBusinessIdRequired'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'BusinessId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [BusinessId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914145237_MakeBusinessIdRequired'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914145237_MakeBusinessIdRequired', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919062741_AddFlocksAndFlockMovements'
)
BEGIN
    CREATE TABLE [FlockMovements] (
        [Id] uniqueidentifier NOT NULL,
        [FlockId] uniqueidentifier NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [MovementDate] datetime2 NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_FlockMovements] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919062741_AddFlocksAndFlockMovements'
)
BEGIN
    CREATE TABLE [Flocks] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [BirdType] nvarchar(max) NOT NULL,
        [Source] nvarchar(max) NOT NULL,
        [AcquisitionDate] datetime2 NOT NULL,
        [InitialCount] int NOT NULL,
        [CurrentCount] int NOT NULL,
        [AcquisitionCost] decimal(18,2) NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Flocks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919062741_AddFlocksAndFlockMovements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919062741_AddFlocksAndFlockMovements', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919113515_AddBusinessTaxStatus'
)
BEGIN
    ALTER TABLE [Businesses] ADD [PercentageTaxOption] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919113515_AddBusinessTaxStatus'
)
BEGIN
    ALTER TABLE [Businesses] ADD [TaxStatus] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919113515_AddBusinessTaxStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919113515_AddBusinessTaxStatus', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919162051_AddActivityLogs'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] uniqueidentifier NULL,
        [Details] nvarchar(max) NULL,
        [TimestampUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919162051_AddActivityLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919162051_AddActivityLogs', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920081909_AddFlockLinkedProductAndEggProduction'
)
BEGIN
    ALTER TABLE [Flocks] ADD [LinkedProductId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920081909_AddFlockLinkedProductAndEggProduction'
)
BEGIN
    CREATE TABLE [EggProductions] (
        [Id] uniqueidentifier NOT NULL,
        [FlockId] uniqueidentifier NOT NULL,
        [ProductionDate] datetime2 NOT NULL,
        [QuantityProduced] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_EggProductions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920081909_AddFlockLinkedProductAndEggProduction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920081909_AddFlockLinkedProductAndEggProduction', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    DROP TABLE [EggProductions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flocks]') AND [c].[name] = N'CurrentCount');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Flocks] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Flocks] DROP COLUMN [CurrentCount];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flocks]') AND [c].[name] = N'InitialCount');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Flocks] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Flocks] DROP COLUMN [InitialCount];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flocks]') AND [c].[name] = N'IsActive');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Flocks] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Flocks] DROP COLUMN [IsActive];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    EXEC sp_rename N'[Flocks].[Source]', N'Species', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    EXEC sp_rename N'[Flocks].[BirdType]', N'Name', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    EXEC sp_rename N'[Flocks].[AcquisitionDate]', N'PlacementDate', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [Products] ADD [Category] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [Flocks] ADD [Breed] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [Flocks] ADD [CloseDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [FlockMovements] ADD [Direction] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [Expenses] ADD [FlockId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    ALTER TABLE [Customers] ADD [Type] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    CREATE TABLE [EggMovements] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [FlockId] uniqueidentifier NULL,
        [MovementDate] datetime2 NOT NULL,
        [Direction] nvarchar(max) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_EggMovements] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921012817_RedesignFlocksAndEggMovements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921012817_RedesignFlocksAndEggMovements', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Category');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Products] DROP COLUMN [Category];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'SKU');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Products] DROP COLUMN [SKU];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'CategoryId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Products] ALTER COLUMN [CategoryId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    ALTER TABLE [Products] ADD [Group] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    ALTER TABLE [Products] ADD [Type] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922031342_RestructureProductToGroupTypeDescription'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922031342_RestructureProductToGroupTypeDescription', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922154948_AddPurchaseBankNameAndStatus'
)
BEGIN
    ALTER TABLE [Purchases] ADD [BankName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922154948_AddPurchaseBankNameAndStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922154948_AddPurchaseBankNameAndStatus', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923030108_UpdateSaleStatusDefault'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923030108_UpdateSaleStatusDefault', N'9.0.8');
END;

COMMIT;
GO

