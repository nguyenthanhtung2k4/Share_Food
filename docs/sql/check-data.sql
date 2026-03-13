USE [ShareFoodDb];
GO

SELECT COUNT(*) AS TongCongThuc FROM Recipes;
SELECT COUNT(*) AS TongNguoiDung FROM AspNetUsers;
SELECT COUNT(*) AS TongBinhLuan FROM Comments;
SELECT COUNT(*) AS TongYeuThich FROM Favorites;
SELECT COUNT(*) AS TongThongBao FROM Notifications;
SELECT COUNT(*) AS TongOtp FROM EmailOtps;
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

SELECT TOP 10 Id, Title, Message, IsRead, CreatedAt
FROM Notifications
ORDER BY CreatedAt DESC;
GO

SELECT TOP 10 Id, Code, ExpiresAt, IsUsed
FROM EmailOtps
ORDER BY CreatedAt DESC;
GO
