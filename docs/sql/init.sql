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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(120) NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
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

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [Ingredients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Ingredients] PRIMARY KEY ([Id])
);

CREATE TABLE [Units] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(80) NOT NULL,
    [Symbol] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Recipes] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(160) NOT NULL,
    [Slug] nvarchar(180) NOT NULL,
    [Summary] nvarchar(1000) NOT NULL,
    [ThumbnailPath] nvarchar(260) NULL,
    [Servings] int NOT NULL,
    [CookTimeMinutes] int NOT NULL,
    [IsVisible] bit NOT NULL,
    [CategoryId] int NOT NULL,
    [AuthorId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Recipes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Recipes_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Recipes_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Comments] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Comments_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Favorites] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Favorites_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Favorites_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RecipeIngredients] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [IngredientId] int NOT NULL,
    [UnitId] int NOT NULL,
    [Quantity] decimal(10,2) NOT NULL,
    [Note] nvarchar(250) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeIngredients_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecipeIngredients_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RecipeSteps] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [StepNumber] int NOT NULL,
    [Instruction] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RecipeSteps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeSteps_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'Slug', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [CreatedAt], [Description], [Name], [Slug], [UpdatedAt])
VALUES (1, '2026-03-13T00:00:00.0000000Z', N'Những món ăn chính dùng cho bữa trưa hoặc bữa tối.', N'Món chính', N'mon-chinh', '2026-03-13T00:00:00.0000000Z'),
(2, '2026-03-13T00:00:00.0000000Z', N'Công thức thanh đạm, tốt cho sức khỏe.', N'Món chay', N'mon-chay', '2026-03-13T00:00:00.0000000Z'),
(3, '2026-03-13T00:00:00.0000000Z', N'Các món ngọt nhẹ cho bữa ăn thêm trọn vẹn.', N'Món tráng miệng', N'mon-trang-mieng', '2026-03-13T00:00:00.0000000Z'),
(4, '2026-03-13T00:00:00.0000000Z', N'Thức uống ngon, dễ làm tại nhà.', N'Đồ uống', N'do-uong', '2026-03-13T00:00:00.0000000Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'Slug', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Ingredients]'))
    SET IDENTITY_INSERT [Ingredients] ON;
INSERT INTO [Ingredients] ([Id], [CreatedAt], [Description], [Name], [UpdatedAt])
VALUES (1, '2026-03-13T00:00:00.0000000Z', N'Nguyên liệu giàu đạm, dễ chế biến.', N'Thịt gà', '2026-03-13T00:00:00.0000000Z'),
(2, '2026-03-13T00:00:00.0000000Z', N'Tạo vị ngọt tự nhiên và màu sắc đẹp.', N'Cà rốt', '2026-03-13T00:00:00.0000000Z'),
(3, '2026-03-13T00:00:00.0000000Z', N'Nguyên liệu quen thuộc trong nhiều món hầm.', N'Khoai tây', '2026-03-13T00:00:00.0000000Z'),
(4, '2026-03-13T00:00:00.0000000Z', N'Dùng cho món tráng miệng và đồ uống.', N'Sữa tươi', '2026-03-13T00:00:00.0000000Z'),
(5, '2026-03-13T00:00:00.0000000Z', N'Nguyên liệu nền cho nhiều món bánh.', N'Bột mì', '2026-03-13T00:00:00.0000000Z'),
(6, '2026-03-13T00:00:00.0000000Z', N'Tăng vị ngọt cho món ăn.', N'Đường', '2026-03-13T00:00:00.0000000Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Ingredients]'))
    SET IDENTITY_INSERT [Ingredients] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Name', N'Symbol', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Units]'))
    SET IDENTITY_INSERT [Units] ON;
INSERT INTO [Units] ([Id], [CreatedAt], [Name], [Symbol], [UpdatedAt])
VALUES (1, '2026-03-13T00:00:00.0000000Z', N'Gram', N'g', '2026-03-13T00:00:00.0000000Z'),
(2, '2026-03-13T00:00:00.0000000Z', N'Kilogram', N'kg', '2026-03-13T00:00:00.0000000Z'),
(3, '2026-03-13T00:00:00.0000000Z', N'Mi-li-lít', N'ml', '2026-03-13T00:00:00.0000000Z'),
(4, '2026-03-13T00:00:00.0000000Z', N'Lít', N'l', '2026-03-13T00:00:00.0000000Z'),
(5, '2026-03-13T00:00:00.0000000Z', N'Cái', N'cái', '2026-03-13T00:00:00.0000000Z'),
(6, '2026-03-13T00:00:00.0000000Z', N'Muỗng', N'muỗng', '2026-03-13T00:00:00.0000000Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Name', N'Symbol', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Units]'))
    SET IDENTITY_INSERT [Units] OFF;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);

CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);

CREATE INDEX [IX_Comments_RecipeId] ON [Comments] ([RecipeId]);

CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);

CREATE UNIQUE INDEX [IX_Favorites_RecipeId_UserId] ON [Favorites] ([RecipeId], [UserId]);

CREATE INDEX [IX_Favorites_UserId] ON [Favorites] ([UserId]);

CREATE UNIQUE INDEX [IX_Ingredients_Name] ON [Ingredients] ([Name]);

CREATE INDEX [IX_RecipeIngredients_IngredientId] ON [RecipeIngredients] ([IngredientId]);

CREATE UNIQUE INDEX [IX_RecipeIngredients_RecipeId_IngredientId] ON [RecipeIngredients] ([RecipeId], [IngredientId]);

CREATE INDEX [IX_RecipeIngredients_UnitId] ON [RecipeIngredients] ([UnitId]);

CREATE INDEX [IX_Recipes_AuthorId] ON [Recipes] ([AuthorId]);

CREATE INDEX [IX_Recipes_CategoryId] ON [Recipes] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Recipes_Slug] ON [Recipes] ([Slug]);

CREATE UNIQUE INDEX [IX_RecipeSteps_RecipeId_StepNumber] ON [RecipeSteps] ([RecipeId], [StepNumber]);

CREATE UNIQUE INDEX [IX_Units_Name] ON [Units] ([Name]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260313095835_InitialCreate', N'9.0.10');

COMMIT;
GO

