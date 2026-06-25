BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Lk_Dictionary"(
"Id" bigint NOT NULL PRIMARY KEY ,
"DictionaryName" varchar(100) NOT NULL  ,
"DisplayName" varchar(100) NOT NULL  ,
"ItemName" varchar(100) NOT NULL  ,
"ItemValue" varchar(100) NULL  ,
"SortOrder" integer NOT NULL  ,
"IsDefault" bit NOT NULL  ,
"IsActive" bit NOT NULL  ,
"Description" text NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
CREATE TABLE IF NOT EXISTS "Lk_InspectionRecord"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Operator" varchar(32) NOT NULL  ,
"Date" varchar(16) NOT NULL  ,
"ShiftId" bigint NOT NULL  ,
"PressureHoldTime" decimal NOT NULL  ,
"Pressure" decimal NOT NULL  ,
"ProductTypeId" bigint NOT NULL  ,
"ProductStatusId" bigint NOT NULL  ,
"PartStatusId" bigint NOT NULL  ,
"NgPositionId" bigint NOT NULL  ,
"ProductModel" varchar(64) NULL  ,
"SteelStamp" varchar(64) NULL  ,
"TestResult" varchar(8) NOT NULL  ,
"Images" text NULL  ,
"UserId" bigint NOT NULL  ,
"Remarks" varchar(512) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    , `Leakage` decimal);
CREATE TABLE IF NOT EXISTS "Lk_NgPosition"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Name" varchar(64) NOT NULL  ,
"IsDefault" bit NOT NULL  ,
"Description" varchar(256) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
CREATE TABLE IF NOT EXISTS "Lk_PartStatus"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Name" varchar(64) NOT NULL  ,
"IsDefault" bit NOT NULL  ,
"Description" varchar(256) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
CREATE TABLE IF NOT EXISTS "Lk_ProductStatus"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Name" varchar(64) NOT NULL  ,
"IsDefault" bit NOT NULL  ,
"Description" varchar(256) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
CREATE TABLE IF NOT EXISTS "Lk_ProductType"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Name" varchar(64) NOT NULL  ,
"IsDefault" bit NOT NULL  ,
"Description" varchar(256) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
CREATE TABLE IF NOT EXISTS "Lk_Shift"(
"Id" bigint NOT NULL PRIMARY KEY ,
"Name" varchar(64) NOT NULL  ,
"StartTime" varchar(8) NULL  ,
"EndTime" varchar(8) NULL  ,
"CreateTime" datetime NULL  ,
"UpdateTime" datetime NULL  ,
"CreateUserId" bigint NULL  ,
"CreateUserName" varchar(64) NULL  ,
"UpdateUserId" bigint NULL  ,
"UpdateUserName" varchar(64) NULL    );
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817517446529093,'超级管理员','2026-06-18',817507526307909,12,12,816185780064325,815537574379589,815537304834117,0,NULL,'fff','OK',NULL,1300000000101,'tt','2026-06-18 11:47:02.3630512',NULL,1300000000101,'超级管理员',NULL,NULL,NULL);
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817518056521797,'超级管理员','2026-06-18',817507526307909,3,12,816185780064325,815537574379589,815537304834117,0,'rr','tt','OK',NULL,1300000000101,NULL,'2026-06-18 11:49:31.2859601',NULL,1300000000101,'超级管理员',NULL,NULL,NULL);
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817518113366085,'超级管理员','2026-06-18',817507526307909,3,12,816185780064325,815537574379589,815537304834117,813662840180805,'tt','uu','NG',NULL,1300000000101,NULL,'2026-06-18 11:49:45.1647568',NULL,1300000000101,'超级管理员',NULL,NULL,NULL);
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817518509297733,'超级管理员','2026-06-18',817507526307909,3,12,816185780064325,815537574379589,815537304834117,813662695497797,NULL,'tt','NG',NULL,1300000000101,NULL,'2026-06-18 11:51:21.8297925',NULL,1300000000101,'超级管理员',NULL,NULL,NULL);
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817527136657477,'超级管理员','2026-06-18',817507526307909,3,12,816185780064325,815537574379589,815537304834117,0,NULL,'eee','OK',NULL,1300000000101,NULL,'2026-06-18 12:26:28.1185689','2026-06-25 15:00:50.12445',1300000000101,'超级管理员',1300000000101,'超级管理员',4.3);
INSERT INTO "Lk_InspectionRecord" ("Id","Operator","Date","ShiftId","PressureHoldTime","Pressure","ProductTypeId","ProductStatusId","PartStatusId","NgPositionId","ProductModel","SteelStamp","TestResult","Images","UserId","Remarks","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName","Leakage") VALUES (817527199559749,'超级管理员','2026-06-18',817507526307909,3,12,816185780064325,815537574379589,815537304834117,0,'eee',NULL,'OK',NULL,1300000000101,NULL,'2026-06-18 12:26:43.4759618','2026-06-25 15:00:43.3583615',1300000000101,'超级管理员',1300000000101,'超级管理员',4.2);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662493536325,'台阶漏',0,NULL,'2026-06-07 14:21:12',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662551785541,'内圆漏',0,NULL,'2026-06-07 14:21:26','2026-06-13 11:09:39',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662586261573,'前端漏',0,NULL,'2026-06-07 14:21:34',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662641119301,'键槽漏',0,NULL,'2026-06-07 14:21:48',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662695497797,'螺纹孔漏',0,NULL,'2026-06-07 14:22:01',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662771634245,'出砂孔漏',0,NULL,'2026-06-07 14:22:20',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (813662840180805,'外圆漏',0,NULL,'2026-06-07 14:22:36','2026-06-12 21:31:36',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_NgPosition" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537182679109,'多处漏',0,'有多个位置漏','2026-06-12 21:29:20','2026-06-13 11:09:32',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_PartStatus" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537304834117,'精车件',1,NULL,'2026-06-12 21:29:49','2026-06-18 11:43:36.2547666',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_PartStatus" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537369075781,'半精车',0,NULL,'2026-06-12 21:30:05',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_PartStatus" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537521434693,'毛坯件',0,NULL,'2026-06-12 21:30:42',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductStatus" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537574379589,'正常品',1,NULL,'2026-06-12 21:30:55','2026-06-18 11:43:45.9628141',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_ProductStatus" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (815537636651077,'复检品',0,NULL,'2026-06-12 21:31:10',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (812684261945413,'盾构件',0,'实现盾构产品的','2026-06-04 20:00:46',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816184524947525,'240-1.5M机壳',0,NULL,'2026-06-14 17:23:22',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816184654041157,'240-1.5M-95平机壳',0,NULL,'2026-06-14 17:23:54',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185125212229,'050机壳',0,NULL,'2026-06-14 17:25:49',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185179258949,'东风240机壳',0,NULL,'2026-06-14 17:26:02',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185409589317,'460-8段机壳',0,NULL,'2026-06-14 17:26:58',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185458872389,'1000机壳',0,NULL,'2026-06-14 17:27:10',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185532158021,'310机壳',0,NULL,'2026-06-14 17:27:28',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185593348165,'380机壳',0,NULL,'2026-06-14 17:27:43',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185633144901,'180机壳',0,NULL,'2026-06-14 17:27:53',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185689075781,'迈为机壳',0,NULL,'2026-06-14 17:28:06',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185739026501,'240一体化机壳',0,NULL,'2026-06-14 17:28:18',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_ProductType" ("Id","Name","IsDefault","Description","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (816185780064325,'410一体化机壳',1,NULL,'2026-06-14 17:28:28','2026-06-18 11:42:45.2638754',1300000000101,'超级管理员',1300000000101,'超级管理员');
INSERT INTO "Lk_Shift" ("Id","Name","StartTime","EndTime","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (817507526307909,'白班','8:30','20:30','2026-06-18 11:06:40.4279899',NULL,1300000000101,'超级管理员',NULL,NULL);
INSERT INTO "Lk_Shift" ("Id","Name","StartTime","EndTime","CreateTime","UpdateTime","CreateUserId","CreateUserName","UpdateUserId","UpdateUserName") VALUES (817507617493061,'夜班','20:30','8:30','2026-06-18 11:07:02.6945254',NULL,1300000000101,'超级管理员',NULL,NULL);
CREATE VIEW TestViewSysUser AS 
 SELECT  `u`.`Id` AS `Id` , `u`.`Account` AS `Account` , `u`.`RealName` AS `RealName` , `u`.`NickName` AS `NickName` , `a`.`Name` AS `OrgName` , `b`.`Name` AS `PosName`  FROM `SysUser` `u` Left JOIN `SysOrg` `a` ON ( `u`.`OrgId` = `a`.`Id` )  Left JOIN `SysPos` `b` ON ( `u`.`PosId` = `b`.`Id` );
CREATE INDEX Index_Dictionary_IsActive ON `Lk_Dictionary`(`IsActive` Desc);
CREATE INDEX Index_Dictionary_Name ON `Lk_Dictionary`(`DictionaryName` Asc);
CREATE INDEX Index_InspectionRecord_Date ON `Lk_InspectionRecord`(`Date` Asc);
CREATE INDEX Index_InspectionRecord_ProductTypeId ON `Lk_InspectionRecord`(`ProductTypeId` Asc);
CREATE INDEX Index_InspectionRecord_TestResult ON `Lk_InspectionRecord`(`TestResult` Asc);
CREATE INDEX Index_InspectionRecord_UserId ON `Lk_InspectionRecord`(`UserId` Asc);
CREATE INDEX index_Lk_Dictionary_CT ON `Lk_Dictionary`(`CreateTime` Asc);
CREATE INDEX index_Lk_InspectionRecord_CT ON `Lk_InspectionRecord`(`CreateTime` Asc);
CREATE INDEX index_Lk_NgPosition_CT ON `Lk_NgPosition`(`CreateTime` Asc);
CREATE INDEX index_Lk_PartStatus_CT ON `Lk_PartStatus`(`CreateTime` Asc);
CREATE INDEX index_Lk_ProductStatus_CT ON `Lk_ProductStatus`(`CreateTime` Asc);
CREATE INDEX index_Lk_ProductType_CT ON `Lk_ProductType`(`CreateTime` Asc);
CREATE INDEX index_Lk_Shift_CT ON `Lk_Shift`(`CreateTime` Asc);
COMMIT;
