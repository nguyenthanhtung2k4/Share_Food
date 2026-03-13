USE [ShareFoodDb];
GO

SELECT TOP 10 Id, Title, Slug, Servings, CookTimeMinutes, IsVisible
FROM Recipes
ORDER BY CreatedAt DESC;
GO

SELECT TOP 10 Id, Name, Slug
FROM Categories
ORDER BY Name;
GO

SELECT TOP 10 Id, Name, Symbol
FROM Units
ORDER BY Name;
GO

SELECT TOP 10 Id, FullName, Email, EmailConfirmed
FROM AspNetUsers
ORDER BY JoinedAt DESC;
GO
