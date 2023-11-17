GO
USE master;
GO
DROP DATABASE MagicLand;
GO
CREATE DATABASE MagicLand;
GO
USE MagicLand;


--- Role --- Create Time: 11/17/2023 - 12:44 PM
Go
Insert into [Role] (Id, [Name], [Status]) Values
('3c1849af-400c-43ca-979e-58c71ce9301d', 'ADMIN', 'ACTIVE');

Insert into [Role] (Id, [Name], [Status]) Values
('2f1ab569-d516-4a46-9a55-61dbcd6b3692', 'STAFF', 'ACTIVE');
Insert into [Role] (Id, [Name], [Status]) Values
('2bdbb8f4-0527-4f9b-8597-ff4d55ba4998', 'LECTURER', 'ACTIVE');
Insert into [Role] (Id, [Name], [Status]) Values
('dcf51c70-37df-4950-90b6-cc424d6e0296', 'PARENT', 'ACTIVE');



--- Address --- Create Time: 11/17/2023 - 12:50 PM
Go
Insert into [Address] (Id, Street, District, City) Values
('171df1f0-933a-47e2-b56c-a917e095a7c3', 'High Technology Area D7', '9', 'Ho Chi Minh');
Insert into [Address] (Id, Street, District, City) Values
('de71ab17-6b33-4a6f-bef1-5802fa8324d7', 'Linh Xuan', '9', 'Ho Chi Minh');
Insert into [Address] (Id, Street, District, City) Values
('b358bd99-d041-4619-8240-6b5277e76d1c', 'Le Van Viet', '7', 'Ho Chi Minh');
Insert into [Address] (Id, Street, District, City) Values
('efa2d425-b9ec-4e47-bbad-e445a587bbdd', 'FPT Hight Technology Area D7', '9', 'Ho Chi Minh');

--- User --- Create Time: 11/17/2023 - 12:53 PM
Go
-- Admintrator
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, AddressId, RoleId) Values
('08048a0b-7d2b-47de-9153-5fa23163aa3f', 'Admintrator', '', '', '', '', '', '3c1849af-400c-43ca-979e-58c71ce9301d');
-- Parent 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, AddressId, RoleId) Values
('42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Dai Nghia', '0123456789', 'nghia@gmail.com', 'Male', '1990-10-10', '171df1f0-933a-47e2-b56c-a917e095a7c3', 'dcf51c70-37df-4950-90b6-cc424d6e0296');
-- Lecture 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, AddressId, RoleId) Values
('66aa922d-4392-4958-bbef-fc9689c9779b', 'Nguyen Thi Tram', '0123456789', 'tram@gmail.com', 'Female', '1990-11-11', 'de71ab17-6b33-4a6f-bef1-5802fa8324d7', '2bdbb8f4-0527-4f9b-8597-ff4d55ba4998');
-- Staff 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, AddressId, RoleId) Values
('0888d62c-d820-4344-b6be-979ee79cc504', 'Ngo Tong Tai', '0123456789', 'tai@gmail.com', 'Male', '1990-12-12', 'b358bd99-d041-4619-8240-6b5277e76d1c', '2f1ab569-d516-4a46-9a55-61dbcd6b3692');


--- Personal Wallet --- Create Time: 11/17/2023 - 1:02 PM
Go
Insert into PersonalWallet (Id, UserId, Balance) Values
('b468f0e3-adf8-4217-a577-cf399cd6a713', '42276dec-4252-4ad2-8cb6-296c58ad062e', 1000000)



--- Wallet Transaction --- Create Time: 11/17/2023 - 1: 03 PM
-- Description format --
-- 1. Pay for [ Name Class]
-- 2. Top up your wallet from [ Name Bank]
Go
Insert into WalletTransaction (Id, PersonalWalletId, [Money], [Type], [Description], CreatedDate) Values 
('6a3ab18a-13e9-4388-ba8d-821c0f5cd10f', 'b468f0e3-adf8-4217-a577-cf399cd6a713', 1100000, 'TOPUP', 'Top up your wallet from Agribank', '2023-17-11');
Insert into WalletTransaction (Id, PersonalWalletId, [Money], [Type], [Description], CreatedDate) Values 
('bad11716-6585-4555-acdc-5982c7855dce', 'b468f0e3-adf8-4217-a577-cf399cd6a713', 100000, 'PAYMENT', 'Pay for advanced math classes', '2023-17-11');



--- Promotion --- Create Time: 11/17/2023 - 1:10 PM
Go
Insert into Promotion ( Id, Code, [Image], UnitDiscount, DiscountValue, EndDate) Values
('7ab4f60d-5b04-4474-a891-eee0b4817541', 'VfvO1856oHM4hQxdJ0YMdbVubsEeiq', '', 'PERCENTAGE', 10, '2023-12-17');
Insert into Promotion ( Id, Code, [Image], UnitDiscount, DiscountValue, EndDate) Values
('2f905f4f-432c-4d04-98b9-57daa89ceefa', 'Hlin2ab8EtBE6PktNBJQbA59qPq5rS', '', 'CASH', 10000, '2023-12-17');



--- User Promotion --- Create Time: 11/17/2023 - 1:17 PM
GO
Insert into UserPromotion (Id, UserId, PromotionId, AccumulateQuantity) Values
('3642e93c-bb30-48f6-9b3d-fedc7349b3dd', '42276dec-4252-4ad2-8cb6-296c58ad062e', '7ab4f60d-5b04-4474-a891-eee0b4817541', 1);
Insert into UserPromotion (Id, UserId, PromotionId, AccumulateQuantity) Values
('b0a3a1ad-20ff-4131-9419-3d8c31513017', '42276dec-4252-4ad2-8cb6-296c58ad062e', '2f905f4f-432c-4d04-98b9-57daa89ceefa', 1);



--- Student --- Create Time: 11/17/2023 - 1:37 PM
GO
Insert into Student (Id, ParentId, FullName, DateOfBirth, Gender, Email) Values
('172c40fe-32e4-43fd-b982-c87afe8b54fa', '42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Bao Trong', '2009-9-9', 'Male', 'trong@gmail.com');
Insert into Student (Id, ParentId, FullName, DateOfBirth, Gender, Email) Values
('f9113f7e-ae51-4f65-a7b4-2348f666787d', '42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Thanh Van', '2005-8-8', 'Female', 'van@gmail.com');



--- Course --- Create Time: 11/17/2023 - 2:19 PM / Finish Time: 2:25 PM
GO
Insert into Course (Id, [Name], NumberOfLesson, MinYearsStudent, MaxYearsStudent, [Status]) Values
('fded66d4-c3e7-4721-b509-e71feab6723a', 'Basic Math', 6, 2020, 2018, 'ACTIVE');
Insert into Course (Id, [Name], NumberOfLesson, MinYearsStudent, MaxYearsStudent, [Status]) Values
('a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 'Advance Math', 12, 2020, 2018, 'ACTIVE');



--- Course Prerequisite --- Create: 11/17/2023 - 2:24
GO
Insert into CoursePrerequisite (Id, CurrentCourseId, PrerequisiteCourseId) Values
('4df39613-41d4-4411-861a-af3bf76e8007', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 'fded66d4-c3e7-4721-b509-e71feab6723a');



--- Class --- Create Time: 11/17/2023 - 2:13 PM / Finish: 2:43 PM
GO
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, Price, AddressId, LimitNumberStudent) Values
('c6d70a5f-56ae-4de0-b441-c080da024524', '4 Simple calculation', 'fded66d4-c3e7-4721-b509-e71feab6723a', '2022-17-11', '2023-01-05', 'STARTED', 'OFFLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 100000, 'efa2d425-b9ec-4e47-bbad-e445a587bbdd', 30);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, Price, AddressId, LimitNumberStudent) Values
('74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678', 'Math with basic geometry', 'fded66d4-c3e7-4721-b509-e71feab6723a', '2023-11-25', '2024-01-25', 'UPCOMING', 'ONLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 100000, '', 30);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, Price, AddressId, LimitNumberStudent) Values
('77a0a23f-5a3d-4447-8985-d7ed203d322f', 'Fractions and Unknown numbers', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', '2023-9-17', '2023-11-17', 'ENDED', 'ONLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 100000, '', 30);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, Price, AddressId, LimitNumberStudent) Values
('592fd51c-e177-49c2-a2dd-a679d02a91c4', 'Geometry of space', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', '2023-11-27', '2024-01-27', 'UPCOMING', 'OFFLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 100000, 'efa2d425-b9ec-4e47-bbad-e445a587bbdd', 30);



--- Class Fee Transaction --- Create Time: 11/17/2023 - 2:45 PM 
GO
Insert into ClassFeeTransaction (Id, ParentId, [Date], ActualPrice) Values 
('2308df16-f4fa-48fe-976b-9fa39ce8f606', '42276dec-4252-4ad2-8cb6-296c58ad062e', '2023-11-17', 100000);


--- Class Transaction --- Create Time: 11/17/2023 - 2:43 PM / Finish: 2:52 PM
GO
Insert into ClassTransaction (Id, ClassId, ClassFeeTransactionId) Values
('35f272a6-6b3d-420a-8206-c200abffc84a', 'c6d70a5f-56ae-4de0-b441-c080da024524', '2308df16-f4fa-48fe-976b-9fa39ce8f606');




--- Student Transaction --- Create Time: 11/17/2023 - 3:29 PM
GO
Insert into StudentTransaction (Id, StudentId, ClassTransactionId) Values
('1d6935d1-98ee-4f86-9430-bd404017b3b2', '172c40fe-32e4-43fd-b982-c87afe8b54fa', '35f272a6-6b3d-420a-8206-c200abffc84a');




--- Slot --- Create Time: 11/17/2023 - 2:55 PM / Finish: 3:05 PM
GO
Insert into Slot (Id, StartTime, EndTime) Values
('417997ac-afd7-4363-bfe5-6cdd56d4713a', '7:00', '9:00');
Insert into Slot (Id, StartTime, EndTime) Values
('301efd4a-618e-4495-8e7e-daa223d3945e', '9:15', '11:15');
Insert into Slot (Id, StartTime, EndTime) Values
('6ab50a00-08ba-483c-bf5d-0d55b05a2ccc', '12:00', '14:00');
Insert into Slot (Id, StartTime, EndTime) Values
('2291e53b-094b-493e-8132-c6494d2b18a8', '14:15', '16:15');
Insert into Slot (Id, StartTime, EndTime) Values
('688fe18c-5db1-40aa-a7f3-f47ccd9fd395', '16:30', '18:30');
Insert into Slot (Id, StartTime, EndTime) Values
('418704fb-fac8-4119-8795-c8fe5d348753', '19:00', '21:00');


--- Room --- Create Time: 11/17/2023 - 3:06 PM
GO
Insert into Room(Id, [Name], [Floor], [Status], LinkUrl) Values
('99f6f043-3fee-435f-a8ae-1f55f13b3256', '609', 4, 'ACTIVE', null)
Insert into Room(Id, [Name], [Floor], [Status], LinkUrl) Values
('5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4', '609', 4, 'ACTIVE', 'https://room')



--- Session --- Create Time: 11/17/2023 - 2:53 PM / Finish: 3:14 PM
GO
Insert into [Session] (Id, ClassId, [DayOfWeek], [Date], SlotId, RoomId) Values 
('a127da28-40ff-40fb-b40d-8fbccc7e5650', 'c6d70a5f-56ae-4de0-b441-c080da024524', 2, '2023-11-18', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '99f6f043-3fee-435f-a8ae-1f55f13b3256');
Insert into [Session] (Id, ClassId, [DayOfWeek], [Date], SlotId, RoomId) Values 
('680b3459-228b-4fd3-b68d-54b629b7b523', 'c6d70a5f-56ae-4de0-b441-c080da024524', 4, '2023-11-20', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '99f6f043-3fee-435f-a8ae-1f55f13b3256');
Insert into [Session] (Id, ClassId, [DayOfWeek], [Date], SlotId, RoomId) Values 
('007f12eb-93d7-43e7-a065-de4919cd2b10', 'c6d70a5f-56ae-4de0-b441-c080da024524', 6, '2023-11-22', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '99f6f043-3fee-435f-a8ae-1f55f13b3256');

--- Class Instance --- Create Time: 11/17/2023 - 3:28 PM
GO
Insert into ClassInstance (Id, SessionId, StudentId, [Status]) Values
('0b365b52-3718-47a7-8a03-96fc55a7868f', 'a127da28-40ff-40fb-b40d-8fbccc7e5650', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NOTYET');
Insert into ClassInstance (Id, SessionId, StudentId, [Status]) Values
('5c62c817-4b61-4a14-b240-a8b879b499a5', '680b3459-228b-4fd3-b68d-54b629b7b523', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NOTYET');
Insert into ClassInstance (Id, SessionId, StudentId, [Status]) Values
('33ef53be-369f-4fce-a7ac-7064211735b5', '007f12eb-93d7-43e7-a065-de4919cd2b10', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NOTYET');