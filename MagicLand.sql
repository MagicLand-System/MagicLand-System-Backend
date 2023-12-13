GO
USE master;
GO
DROP DATABASE MagicLandDBServer;
GO
CREATE DATABASE MagicLandServer;
GO
USE MagicLandDBServer;


--- Role --- Create Time: 11/17/2023 - 12:44 PM
Go
Insert into [Role] (Id, [Name]) Values
('3c1849af-400c-43ca-979e-58c71ce9301d', 'ADMIN');

Insert into [Role] (Id, [Name]) Values
('2f1ab569-d516-4a46-9a55-61dbcd6b3692', 'STAFF');
Insert into [Role] (Id, [Name]) Values
('2bdbb8f4-0527-4f9b-8597-ff4d55ba4998', 'LECTURER');
Insert into [Role] (Id, [Name]) Values
('dcf51c70-37df-4950-90b6-cc424d6e0296', 'PARENT');


--- User --- Create Time: 11/17/2023 - 12:53 PM
Go
-- Admintrator
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, RoleId) Values
('08048a0b-7d2b-47de-9153-5fa23163aa3f', 'Admintrator', '09212312212','admin@gmail.com', 'MALE', '2023-11-17', '3c1849af-400c-43ca-979e-58c71ce9301d');
-- Parent 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, RoleId, AvatarImage, City, Street) Values
('42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Dai Nghia', '0962243201', 'nghia@gmail.com', 'MALE', '1990-10-10', 'dcf51c70-37df-4950-90b6-cc424d6e0296', null, 'Ho Chi Minh', 'D7 district high technogy');
-- Lecture 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, RoleId, AvatarImage, City, Street) Values
('66aa922d-4392-4958-bbef-fc9689c9779b', 'Nguyen Thi Tram', '0985081621', 'tram@gmail.com', 'FEMALE', '1990-11-11', '2bdbb8f4-0527-4f9b-8597-ff4d55ba4998', null, 'Ho Chi Minh', 'Pham Van Dong brige linh xuan');
-- Staff 
Insert into [User] (Id, FullName, Phone, Email, Gender, DateOfBirth, RoleId, AvatarImage, City, Street) Values
('0888d62c-d820-4344-b6be-979ee79cc504', 'Ngo Tong Tai', '0325021012', 'tai@gmail.com', 'MALE', '1990-12-12', '2f1ab569-d516-4a46-9a55-61dbcd6b3692', null, 'Ha Noi', 'Freedom stat center area');


--- Cart ---
Insert into Cart (Id, UserId) Values 
('b05c1ef3-a32f-492f-a5ca-359085df545d', '42276dec-4252-4ad2-8cb6-296c58ad062e');

--- Personal Wallet --- Create Time: 11/17/2023 - 1:02 PM
Go
Insert into PersonalWallet (Id, UserId, Balance) Values
('b468f0e3-adf8-4217-a577-cf399cd6a713', '42276dec-4252-4ad2-8cb6-296c58ad062e', 1000000)



--- Wallet Transaction --- Create Time: 11/17/2023 - 1: 03 PM
-- Description format --
-- 1. Pay for [ Name Class]
-- 2. Top up your wallet from [ Name Bank]
Go
Insert into WalletTransaction (Id, PersonalWalletId, [Money], [Type], [Description], CreatedTime) Values 
('6a3ab18a-13e9-4388-ba8d-821c0f5cd10f', 'b468f0e3-adf8-4217-a577-cf399cd6a713', 1100000, 'TOPUP', 'Top up your wallet from Agribank', '2023-11-17');
Insert into WalletTransaction (Id, PersonalWalletId, [Money], [Type], [Description], CreatedTime) Values 
('bad11716-6585-4555-acdc-5982c7855dce', 'b468f0e3-adf8-4217-a577-cf399cd6a713', 100000, 'PAYMENT', 'Pay for advanced math classes', '2023-11-17');



--- Promotion --- Create Time: 11/17/2023 - 1:10 PM - *** In Fixing Not Insert ***
Go
Insert into Promotion ( Id, Code, [Image], UnitDiscount, DiscountValue, EndDate) Values
('7ab4f60d-5b04-4474-a891-eee0b4817541', 'VfvO1856oHM4hQxdJ0YMdbVubsEeiq', '', 'PERCENTAGE', 10, '2023-12-17');
Insert into Promotion ( Id, Code, [Image], UnitDiscount, DiscountValue, EndDate) Values
('2f905f4f-432c-4d04-98b9-57daa89ceefa', 'Hlin2ab8EtBE6PktNBJQbA59qPq5rS', '', 'CASH', 10000, '2023-12-17');



--- User Promotion --- Create Time: 11/17/2023 - 1:17 PM - *** In Fixing Not Insert ***
GO
Insert into UserPromotion (Id, UserId, PromotionId, AccumulateQuantity) Values
('3642e93c-bb30-48f6-9b3d-fedc7349b3dd','42276DEC-4252-4AD2-8CB6-296C58AD062E','7ab4f60d-5b04-4474-a891-eee0b4817541',1);
Insert into UserPromotion (Id, UserId, PromotionId, AccumulateQuantity) Values
('b0a3a1ad-20ff-4131-9419-3d8c31513017','42276DEC-4252-4AD2-8CB6-296C58AD062E', '2f905f4f-432c-4d04-98b9-57daa89ceefa', 1);



--- Student --- Create Time: 11/17/2023 - 1:37 PM
GO
Insert into Student (Id, ParentId, FullName, DateOfBirth, Gender, Email, AvatarImage) Values
('172c40fe-32e4-43fd-b982-c87afe8b54fa', '42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Bao Trong', '2009-9-9', 'Male', 'trong@gmail.com', null);
Insert into Student (Id, ParentId, FullName, DateOfBirth, Gender, Email, AvatarImage) Values
('f9113f7e-ae51-4f65-a7b4-2348f666787d', '42276dec-4252-4ad2-8cb6-296c58ad062e', 'Tran Thanh Van', '2005-8-8', 'Female', 'van@gmail.com', null);

--- CourseCategory --- Create Time: 12/10/2023 - 1:18 PM
GO
Insert into CourseCategory (Id, [Name]) values ('f9e93ffc-e2e6-4881-be8c-24e411050900', 'MATH')
Insert into CourseCategory (Id, [Name]) values ('3103b0d7-d169-4155-96cd-4c30d72f82cc', 'PHYSIC')
Insert into CourseCategory (Id, [Name]) values ('3e9687b0-8f5c-4384-b5b1-1dae3a0703ee', 'LANGUAGE')
Insert into CourseCategory (Id, [Name]) values ('06f5a898-e0db-4766-b0af-3340ec5d7b04', 'PAIN')
Insert into CourseCategory (Id, [Name]) values ('8184d47a-5ffd-4c71-a0ea-fd768cd64865', 'DRAWING ')
Insert into CourseCategory (Id, [Name]) values ('291c9d7b-d7f2-4bb8-884f-2fe45703b662', 'DANCE')
Insert into CourseCategory (Id, [Name]) values ('9bd5c634-6b0a-42c3-a898-d2db78ee73bc', 'PROGRAM')

--- Course --- Create Time: 11/17/2023 - 2:19 PM / Finish Time: 2:25 PM
GO
Insert into Course (Id, [Name], NumberOfSession, MinYearOldsStudent, MaxYearOldsStudent, [Status], Price, CourseCategoryId) Values
('fded66d4-c3e7-4721-b509-e71feab6723a','Basic Math', 6, 3, 6, 'ACTIVE', 2000000, 'f9e93ffc-e2e6-4881-be8c-24e411050900');
Insert into Course (Id, [Name], NumberOfSession, MinYearOldsStudent, MaxYearOldsStudent, [Status], Price, CourseCategoryId) Values
('a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 'Advance Math', 5, 5, 12, 'ACTIVE', 2500000, 'f9e93ffc-e2e6-4881-be8c-24e411050900');
Insert into Course (Id, [Name], NumberOfSession, MinYearOldsStudent, MaxYearOldsStudent, [Status], Price, CourseCategoryId) Values
('9cbca0ad-7048-4777-a13f-8c34e92dea43', 'Normal Math', 3, 5, 7, 'ACTIVE', 17000000, 'f9e93ffc-e2e6-4881-be8c-24e411050900');

--- Course Description --- Create Time: 12/11/2023 - 2:26
GO
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('a19598fb-f125-47a7-8334-0cbc85b34240', 'Basic Math', 'This course help leaning math basic in easy', 1, 'fded66d4-c3e7-4721-b509-e71feab6723a')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('b33c6d7a-6b7e-48ca-91c0-3744b388ddc1', 'Why Should Chose This Course', 'Have the most common and easy way to teach help student leaning faster', 2, 'fded66d4-c3e7-4721-b509-e71feab6723a')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('d830a44d-2a75-4a1d-903b-ca9f77cbb53d', 'Why Sould Chose This Course', 'Tranning significant skill set', 3, 'fded66d4-c3e7-4721-b509-e71feab6723a')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('1df32ba5-cd02-44b1-b745-5f04a065fd05', 'Why Should Chose This Course', 'Improvement thinking and progressing', 4, 'fded66d4-c3e7-4721-b509-e71feab6723a')

Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('d7079327-f296-4000-a3d4-ff5f44498d60', 'This is title', 'This is content', 1, 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('16c8455b-b3f7-48ea-a45c-6d02a0c00130', 'This is title', 'This is content', 2, 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('13824a01-009f-4efc-85f7-e95c3f7b9b30', 'This is title', 'This is content', 3, 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32')

Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('57bfc13f-cf2e-460a-8be0-148fddfb30e8', 'This is title', 'This is content', 1, '9cbca0ad-7048-4777-a13f-8c34e92dea43')
Insert into CourseDescription (Id, Title, Content, [Order], CourseId) Values 
('8362ecd6-8475-4d07-9706-2ae0616499c2', 'This is title', 'This is content', 2, '9cbca0ad-7048-4777-a13f-8c34e92dea43')
--- Course Prerequisite --- Create: 11/17/2023 - 2:24
GO
Insert into CoursePrerequisite (Id, CurrentCourseId, PrerequisiteCourseId) Values
('4df39613-41d4-4411-861a-af3bf76e8007', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 'fded66d4-c3e7-4721-b509-e71feab6723a');
Insert into CoursePrerequisite (Id, CurrentCourseId, PrerequisiteCourseId) Values
('a7c0ee77-73cb-4fcc-9db4-b03aeb47ac06', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', '9cbca0ad-7048-4777-a13f-8c34e92dea43');


--- Class --- Create Time: 11/17/2023 - 2:13 PM / Finish: 2:43 PM
GO
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, LimitNumberStudent, City, District, Street, ClassCode, LeastNumberStudent) Values
('c6d70a5f-56ae-4de0-b441-c080da024524', '4 Simple calculation', 'fded66d4-c3e7-4721-b509-e71feab6723a', '2022-11-17', '2023-01-05', 'STARTED', 'OFFLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 30, 'Ho Chi Minh', '9', 'D7 hightech', 'MAD292', 10);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, LimitNumberStudent, City, District, Street, ClassCode, LeastNumberStudent) Values
('74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678', 'Math with basic geometry', 'fded66d4-c3e7-4721-b509-e71feab6723a', '2023-11-25', '2024-01-25', 'UPCOMING', 'ONLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 30, 'Home', '', '', 'MAD291', 10);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, LimitNumberStudent, City, District, Street, ClassCode, LeastNumberStudent) Values
('77a0a23f-5a3d-4447-8985-d7ed203d322f', 'Fractions and Unknown numbers', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', '2023-9-17', '2023-11-17', 'ENDED', 'ONLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 30, 'Home', '', '', 'MES202', 10);
Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, LimitNumberStudent, City, District, Street, ClassCode, LeastNumberStudent) Values
('592fd51c-e177-49c2-a2dd-a679d02a91c4', 'Geometry of space', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', '2023-11-27', '2024-01-27', 'UPCOMING', 'OFFLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 30, 'Ho Chi Minh', '9', 'D7 hightech', 'PRN231',10);

Insert into Class (Id, [Name], CourseId, StartDate, EndDate, [Status], Method, LecturerId, LimitNumberStudent, City, District, Street, ClassCode, LeastNumberStudent) Values
('e79b39f1-b0e4-4ef5-9d66-6794c2099020', 'Equations', '9cbca0ad-7048-4777-a13f-8c34e92dea43', '2023-11-27', '2024-01-27', 'UPCOMING', 'OFFLINE', '66aa922d-4392-4958-bbef-fc9689c9779b', 30, 'Ho Chi Minh', '9', 'D7 hightech', 'PRU292', 10);


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
Insert into Room(Id, [Name], [Floor], [Status], LinkUrl, Capacity) Values
('99f6f043-3fee-435f-a8ae-1f55f13b3256', '609', 4, 'ACTIVE', null, 30)
Insert into Room(Id, [Name], [Floor], [Status], LinkUrl, Capacity) Values
('c4cf53be-440b-445f-b8f3-82d7f3af409c', '404', 2, 'ACTIVE', null, 30)
Insert into Room(Id, [Name], [Floor], [Status], LinkUrl, Capacity) Values
('5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4', '609', 4, 'ACTIVE', 'https://room', 0)



--- Session --- Create Time: 11/17/2023 - 2:53 PM / Finish: 3:14 PM
GO
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('a127da28-40ff-40fb-b40d-8fbccc7e5650', 'fded66d4-c3e7-4721-b509-e71feab6723a', 1, 'This is cotent for session 1 of coure basic math', 'This is description for session 1 of coure basic math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('680b3459-228b-4fd3-b68d-54b629b7b523', 'fded66d4-c3e7-4721-b509-e71feab6723a', 2, 'This is cotent for session 2 of coure basic math', 'This is description for session 2 of coure basic math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('007f12eb-93d7-43e7-a065-de4919cd2b10', 'fded66d4-c3e7-4721-b509-e71feab6723a', 3, 'This is cotent for session 3 of coure basic math', 'This is description for session 3 of coure basic math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('6814331b-8564-480f-ac55-75ea58168a49', 'fded66d4-c3e7-4721-b509-e71feab6723a', 4, 'This is cotent for session 4 of coure basic math', 'This is description for session 4 of coure basic math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('52fca4b4-73d4-4734-bb7a-95cead567338', 'fded66d4-c3e7-4721-b509-e71feab6723a', 5, 'This is cotent for session 5 of coure basic math', 'This is description for session 5 of coure basic math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('e4d7f722-655c-42e6-89f6-29289b361120', 'fded66d4-c3e7-4721-b509-e71feab6723a', 6, 'This is cotent for session 6 of coure basic math', 'This is description for session 6 of coure basic math');


-- Insert Time: 11/22/2023 - 11:23
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('76ab84e2-146d-4e2e-a100-91c1b0998285', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 1, 'This is cotent for session 1 of coure advance math', 'This is description for session 1 of coure advance math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('a8523e7c-b88c-4e36-b002-161af680e4e0', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 2, 'This is cotent for session 2 of coure advance math', 'This is description for session 2 of coure advance math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('f20510f6-a1b1-4894-a4d2-62d8094bc57b', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 3, 'This is cotent for session 3 of coure advance math', 'This is description for session 3 of coure advance math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('6a874dd6-ca56-4fde-adee-3dd47a17a430', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 4, 'This is cotent for session 4 of coure advance math', 'This is description for session 4 of coure advance math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('473ed4c8-bdff-47cf-bbd2-e5d8bc472554', 'a44d6a0a-8e7e-4fe4-804a-6ff195d94a32', 5, 'This is cotent for session 5 of coure advance math', 'This is description for session 5 of coure advance math');


Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('6b591b11-5f7a-4102-809b-9c69e84eddc0', '9cbca0ad-7048-4777-a13f-8c34e92dea43', 1, 'This is cotent for session 1 of course normal math', 'This is description for session 1 of course normal math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('d3407e14-c7fc-49ff-ade3-438bedf415a8', '9cbca0ad-7048-4777-a13f-8c34e92dea43', 2, 'This is cotent for session 2 of course normal math', 'This is description for session 2 of course normal math');
Insert into [Session] (Id, CourseId, NoSession, Content, [Description]) Values 
('50a5f183-2b39-4bd9-ae59-ce99cd1d21ca', '9cbca0ad-7048-4777-a13f-8c34e92dea43', 3, 'This is cotent for session 3 of course normal math', 'This is description for session 3 of course normal math');

--- Student Class --- Create Time: 11/17/2023 - 3:28 PM
GO
Insert into StudentClass (Id, ClassId, StudentId, [Status]) Values
('0b365b52-3718-47a7-8a03-96fc55a7868f', 'c6d70a5f-56ae-4de0-b441-c080da024524', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NORMAL');
Insert into StudentClass (Id, ClassId, StudentId, [Status]) Values
('5c62c817-4b61-4a14-b240-a8b879b499a5', 'c6d70a5f-56ae-4de0-b441-c080da024524', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NORMAL');
Insert into StudentClass (Id, ClassId, StudentId, [Status]) Values
('33ef53be-369f-4fce-a7ac-7064211735b5', 'c6d70a5f-56ae-4de0-b441-c080da024524', '172c40fe-32e4-43fd-b982-c87afe8b54fa', 'NORMAL');

--- Schedule --- Create Time: 12/12/2023 - 11:24 AM
GO
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('b09aaf22-5059-4635-bd15-a1d95eedb529', 2, '2023-12-05', 'c6d70a5f-56ae-4de0-b441-c080da024524', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '99f6f043-3fee-435f-a8ae-1f55f13b3256')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('b59317a3-1b59-4dff-aa26-fc7f7892ef3b', 8, '2023-12-07', 'c6d70a5f-56ae-4de0-b441-c080da024524', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '99f6f043-3fee-435f-a8ae-1f55f13b3256')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('7622b97c-d126-453a-aa8e-adc04b8edf74', 32, '2023-12-09', 'c6d70a5f-56ae-4de0-b441-c080da024524', '301efd4a-618e-4495-8e7e-daa223d3945e', '99f6f043-3fee-435f-a8ae-1f55f13b3256')

Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('66089738-6350-4ae9-92aa-b9d71a3c82de', 1, '2023-12-05', '74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678', '301efd4a-618e-4495-8e7e-daa223d3945e', '5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('d920f2cb-a106-40d3-b661-400f8b3c4610', 4, '2023-12-07', '74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678', '6ab50a00-08ba-483c-bf5d-0d55b05a2ccc', '5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4')


Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('1ed95198-13d8-4180-9194-e894d6047fa0', 4, '2023-12-05', '77a0a23f-5a3d-4447-8985-d7ed203d322f', '417997ac-afd7-4363-bfe5-6cdd56d4713a', '5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('8e526f13-32cc-46c6-a557-270d1cf41c0b', 32, '2023-12-09', '77a0a23f-5a3d-4447-8985-d7ed203d322f', '301efd4a-618e-4495-8e7e-daa223d3945e', '5cc8a8e9-d53e-46de-b177-c4fc7c1c84c4')


Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('63e41284-3c47-45e2-90c4-2decdbdaab05', 1, '2023-12-05', '592fd51c-e177-49c2-a2dd-a679d02a91c4', '417997ac-afd7-4363-bfe5-6cdd56d4713a', 'c4cf53be-440b-445f-b8f3-82d7f3af409c')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('cf11d4ef-353b-4348-8ed1-c7cec87da153', 1, '2023-12-05', '592fd51c-e177-49c2-a2dd-a679d02a91c4', '301efd4a-618e-4495-8e7e-daa223d3945e', 'c4cf53be-440b-445f-b8f3-82d7f3af409c')
Insert into Schedule (Id, [DayOfWeek], [Date], ClassId, SlotId, RoomId) Values 
('d31368c8-2848-40bf-9bc0-93bda2342758', 1, '2023-12-05', '592fd51c-e177-49c2-a2dd-a679d02a91c4', '6ab50a00-08ba-483c-bf5d-0d55b05a2ccc', 'c4cf53be-440b-445f-b8f3-82d7f3af409c')

