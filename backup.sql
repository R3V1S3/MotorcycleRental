-- =============================================
-- PostgreSQL database dump (imitation)
-- Dumped from database version 14+
-- =============================================

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

-- =============================================
-- 1. Пересоздание базы данных
-- =============================================
SELECT pg_terminate_backend(pid) 
FROM pg_stat_activity 
WHERE datname = 'MotorcycleRental';

DROP DATABASE IF EXISTS "MotorcycleRental";

CREATE DATABASE "MotorcycleRental" 
    WITH ENCODING = 'UTF8' 
    LC_COLLATE = 'Russian_Russia.1251' 
    LC_CTYPE = 'Russian_Russia.1251' 
    TEMPLATE = template0;

\c "MotorcycleRental";

-- =============================================
-- 2. Таблицы (удаление старых, создание новых)
-- =============================================
DROP TABLE IF EXISTS "public"."RentalItems" CASCADE;
DROP TABLE IF EXISTS "public"."Rentals" CASCADE;
DROP TABLE IF EXISTS "public"."Vehicles" CASCADE;
DROP TABLE IF EXISTS "public"."Employees" CASCADE;
DROP TABLE IF EXISTS "public"."Clients" CASCADE;
DROP TABLE IF EXISTS "public"."VehicleCategories" CASCADE;

-- Таблица: Категории техники
CREATE TABLE "public"."VehicleCategories" (
    "CategoryID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" character varying(50) NOT NULL UNIQUE,
    "BasePricePerHour" numeric(10,2) NOT NULL,
    "IsSeasonal" boolean NOT NULL DEFAULT false,
    "SeasonStartMonth" integer,
    "SeasonEndMonth" integer,
    CONSTRAINT "CHK_Categories_Price" CHECK (("BasePricePerHour" > 0))
);

-- Таблица: Клиенты
CREATE TABLE "public"."Clients" (
    "ClientID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "FullName" character varying(100) NOT NULL,
    "Phone" character varying(20) NOT NULL,
    "PassportSeries" character varying(4) NOT NULL,
    "PassportNumber" character varying(6) NOT NULL,
    "Address" character varying(200),
    CONSTRAINT "UK_Clients_Passport" UNIQUE ("PassportSeries", "PassportNumber"),
    CONSTRAINT "CHK_Clients_Phone" CHECK (("Phone" ~ '^\+[0-9]{11}$'::text))
);

-- Таблица: Сотрудники
CREATE TABLE "public"."Employees" (
    "EmployeeID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "FullName" character varying(100) NOT NULL,
    "Login" character varying(50) NOT NULL UNIQUE,
    "PasswordHash" character varying(100),
    "Role" character varying(20) NOT NULL DEFAULT 'Менеджер'::character varying,
    CONSTRAINT "CHK_Employees_Role" CHECK (("Role" = ANY (ARRAY['Администратор'::character varying, 'Менеджер'::character varying])))
);

-- Таблица: Техника
CREATE TABLE "public"."Vehicles" (
    "VehicleID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "CategoryID" integer,
    "Model" character varying(100) NOT NULL,
    "PlateNumber" character varying(20) NOT NULL UNIQUE,
    "Power" integer NOT NULL,
    "Status" character varying(20) NOT NULL DEFAULT 'Свободна'::character varying,
    CONSTRAINT "CHK_Vehicles_Status" CHECK (("Status" = ANY (ARRAY['Свободна'::character varying, 'В аренде'::character varying, 'В ремонте'::character varying, 'На хранении'::character varying]))),
    CONSTRAINT "CHK_Vehicles_Power" CHECK (("Power" > 0)),
    CONSTRAINT "FK_Vehicles_Category" FOREIGN KEY ("CategoryID") REFERENCES "public"."VehicleCategories"("CategoryID") ON DELETE SET NULL
);

-- Таблица: Договоры аренды
CREATE TABLE "public"."Rentals" (
    "RentalID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClientID" integer,
    "EmployeeID" integer,
    "StartDate" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "PlannedEndDate" timestamp without time zone NOT NULL,
    "ActualEndDate" timestamp without time zone,
    "TotalCost" numeric(10,2) NOT NULL DEFAULT 0,
    "Status" character varying(20) NOT NULL DEFAULT 'Активен'::character varying,
    CONSTRAINT "CHK_Rentals_Dates" CHECK (("PlannedEndDate" >= "StartDate")),
    CONSTRAINT "CHK_Rentals_ActualDate" CHECK ((("ActualEndDate" >= "StartDate") OR ("ActualEndDate" IS NULL))),
    CONSTRAINT "CHK_Rentals_Status" CHECK (("Status" = ANY (ARRAY['Активен'::character varying, 'Завершен'::character varying, 'Просрочен'::character varying]))),
    CONSTRAINT "FK_Rentals_Client" FOREIGN KEY ("ClientID") REFERENCES "public"."Clients"("ClientID"),
    CONSTRAINT "FK_Rentals_Employee" FOREIGN KEY ("EmployeeID") REFERENCES "public"."Employees"("EmployeeID")
);

-- Таблица: Позиции аренды (1:М)
CREATE TABLE "public"."RentalItems" (
    "ItemID" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RentalID" integer NOT NULL,
    "VehicleID" integer,
    "HoursCount" integer NOT NULL,
    "Cost" numeric(10,2) NOT NULL,
    CONSTRAINT "CHK_RentalItems_Hours" CHECK (("HoursCount" > 0)),
    CONSTRAINT "CHK_RentalItems_Cost" CHECK (("Cost" >= 0)),
    CONSTRAINT "FK_RentalItems_Rental" FOREIGN KEY ("RentalID") REFERENCES "public"."Rentals"("RentalID") ON DELETE CASCADE,
    CONSTRAINT "FK_RentalItems_Vehicle" FOREIGN KEY ("VehicleID") REFERENCES "public"."Vehicles"("VehicleID")
);

-- =============================================
-- 3. Индексы
-- =============================================
CREATE INDEX "IX_Clients_Phone" ON "public"."Clients" USING btree ("Phone");
CREATE INDEX "IX_Vehicles_PlateNumber" ON "public"."Vehicles" USING btree ("PlateNumber");
CREATE INDEX "IX_Vehicles_Status" ON "public"."Vehicles" USING btree ("Status");
CREATE INDEX "IX_Rentals_StartDate" ON "public"."Rentals" USING btree ("StartDate");
CREATE INDEX "IX_Rentals_ClientID" ON "public"."Rentals" USING btree ("ClientID");
CREATE INDEX "IX_RentalItems_RentalID" ON "public"."RentalItems" USING btree ("RentalID");

-- =============================================
-- 4. Представления (VIEW)
-- =============================================
CREATE OR REPLACE VIEW "public"."vw_FreeVehicles" AS
 SELECT "VehicleID", "Model", "PlateNumber", "Power", "Status"
   FROM "public"."Vehicles"
  WHERE ("Status" = 'Свободна'::character varying);

CREATE OR REPLACE VIEW "public"."vw_RentalFullInfo" AS
 SELECT r."RentalID", c."FullName" AS "ClientName", c."Phone" AS "ClientPhone", 
        e."FullName" AS "EmployeeName", v."Model" AS "VehicleModel", v."PlateNumber", 
        r."StartDate", r."PlannedEndDate", r."ActualEndDate", ri."HoursCount", 
        ri."Cost", r."Status" AS "RentalStatus"
   FROM (((("public"."Rentals" r
     JOIN "public"."Clients" c ON ((r."ClientID" = c."ClientID")))
     JOIN "public"."Employees" e ON ((r."EmployeeID" = e."EmployeeID")))
     JOIN "public"."RentalItems" ri ON ((r."RentalID" = ri."RentalID")))
     JOIN "public"."Vehicles" v ON ((ri."VehicleID" = v."VehicleID")));

CREATE OR REPLACE VIEW "public"."vw_CategoryRevenue" AS
 SELECT vc."Name" AS "CategoryName", count(ri."ItemID") AS "RentalCount", sum(ri."Cost") AS "TotalRevenue"
   FROM (((("public"."VehicleCategories" vc
     JOIN "public"."Vehicles" v ON ((vc."CategoryID" = v."CategoryID")))
     JOIN "public"."RentalItems" ri ON ((v."VehicleID" = ri."VehicleID")))
     JOIN "public"."Rentals" r ON ((ri."RentalID" = r."RentalID")))
  WHERE (r."Status" = 'Завершен'::character varying)
  GROUP BY vc."Name"
 HAVING (sum(ri."Cost") > (0)::numeric);

CREATE OR REPLACE VIEW "public"."vw_PowerfulFreeVehicles" AS
 SELECT "VehicleID", "Model", "PlateNumber", "Power", "Status"
   FROM "public"."Vehicles"
  WHERE (("Status" = 'Свободна'::character varying) AND ("Power" > 100));

-- =============================================
-- 5. Функции и Триггеры
-- =============================================
-- Функция обновления статуса техники
CREATE OR REPLACE FUNCTION "public"."fn_UpdateVehicleStatus"()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    IF TG_OP = 'INSERT' AND NEW."Status" = 'Активен' THEN
        UPDATE "public"."Vehicles"
        SET "Status" = 'В аренде'
        FROM "public"."RentalItems" ri
        WHERE ri."VehicleID" = "public"."Vehicles"."VehicleID"
          AND ri."RentalID" = NEW."RentalID";
    END IF;
    
    IF TG_OP = 'UPDATE' AND NEW."Status" = 'Завершен' AND OLD."Status" != 'Завершен' THEN
        UPDATE "public"."Vehicles"
        SET "Status" = 'Свوبодна'
        FROM "public"."RentalItems" ri
        WHERE ri."VehicleID" = "public"."Vehicles"."VehicleID"
          AND ri."RentalID" = NEW."RentalID"
          AND "public"."Vehicles"."Status" = 'В аренде';
    END IF;
    
    RETURN NEW;
END;
$function$;

CREATE TRIGGER "trg_UpdateVehicleStatus"
    AFTER INSERT OR UPDATE ON "public"."Rentals"
    FOR EACH ROW EXECUTE FUNCTION "public"."fn_UpdateVehicleStatus"();

-- Функция пересчета суммы договора
CREATE OR REPLACE FUNCTION "public"."fn_CalculateRentalTotal"()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
DECLARE
    target_rental_id integer;
BEGIN
    IF TG_OP = 'DELETE' THEN
        target_rental_id := OLD."RentalID";
    ELSE
        target_rental_id := NEW."RentalID";
    END IF;
    
    UPDATE "public"."Rentals"
    SET "TotalCost" = (
        SELECT COALESCE(SUM("Cost"), 0)
        FROM "public"."RentalItems"
        WHERE "RentalID" = target_rental_id
    )
    WHERE "RentalID" = target_rental_id;
    
    RETURN NEW;
END;
$function$;

CREATE TRIGGER "trg_CalculateRentalTotal"
    AFTER INSERT OR UPDATE OR DELETE ON "public"."RentalItems"
    FOR EACH ROW EXECUTE FUNCTION "public"."fn_CalculateRentalTotal"();

-- Процедура сезонного обновления
CREATE OR REPLACE PROCEDURE "public"."usp_UpdateSeasonalStatus"()
 LANGUAGE plpgsql
AS $procedure$
DECLARE
    current_month integer;
BEGIN
    current_month := EXTRACT(MONTH FROM CURRENT_TIMESTAMP);
    
    UPDATE "public"."Vehicles" v
    SET "Status" = CASE 
        WHEN current_month BETWEEN 12 AND 3 THEN 'Свободна'
        ELSE 'На хранении'
    END
    FROM "public"."VehicleCategories" vc
    WHERE v."CategoryID" = vc."CategoryID"
    AND vc."Name" = 'Снегоход'
    AND v."Status" NOT IN ('В аренде', 'В ремонте');
    
    UPDATE "public"."Vehicles" v
    SET "Status" = CASE 
        WHEN current_month BETWEEN 4 AND 11 THEN 'Свободна'
        ELSE 'На хранении'
    END
    FROM "public"."VehicleCategories" vc
    WHERE v."CategoryID" = vc."CategoryID"
    AND vc."Name" = 'Мотоцикл'
    AND v."Status" NOT IN ('В аренде', 'В ремонте');
    
    RAISE NOTICE 'Сезонные статусы обновлены. Текущий месяц: %', current_month;
END;
$procedure$;

-- =============================================
-- 6. Данные (INSERT statements)
-- =============================================
-- Категории
INSERT INTO "public"."VehicleCategories" ("Name", "BasePricePerHour", "IsSeasonal", "SeasonStartMonth", "SeasonEndMonth") VALUES
('Снегоход', 2500.00, true, 12, 3),
('Квадроцикл', 3000.00, false, NULL, NULL),
('Мотоцикл', 1500.00, true, 4, 11);

-- Сотрудники
INSERT INTO "public"."Employees" ("FullName", "Login", "PasswordHash", "Role") VALUES
('Иванов Иван Петрович', 'admin', 'admin123', 'Администратор'),
('Петров Петр Сергеевич', 'manager1', 'pass123', 'Менеджер'),
('Волкова Мария Сергеевна', 'manager2', 'pass123', 'Менеджер');

-- Клиенты
INSERT INTO "public"."Clients" ("FullName", "Phone", "PassportSeries", "PassportNumber", "Address") VALUES
('Сидоров Алексей Николаевич', '+79001234567', '4500', '123456', 'г. Москва, ул. Ленина, д. 1'),
('Козлова Мария Ивановна', '+79007654321', '4501', '654321', 'г. Москва, ул. Мира, д. 10'),
('Смирнов Алексей Иванович', '+79031112233', '4502', '222333', 'г. Москва, ул. Ленина, д. 5'),
('Кузнецова Анна Петровна', '+79034445566', '4503', '444555', 'г. Москва, ул. Гагарина, д. 10');

-- Техника
INSERT INTO "public"."Vehicles" ("CategoryID", "Model", "PlateNumber", "Power", "Status") VALUES
(1, 'Yamaha Viking 540', 'А 001 АА 77', 60, 'Свободна'),
(1, 'Русская Механика Рысь', 'А 002 АА 77', 120, 'В аренде'),
(2, 'Yamaha Grizzly 700', 'В 001 ВВ 77', 70, 'Свободна'),
(2, 'BRP Can-Am 850', 'В 002 ВВ 77', 85, 'Свободна'),
(3, 'Honda CB500F', 'М 001 ММ 77', 50, 'Свободна'),
(3, 'Kawasaki Ninja 650', 'М 002 ММ 77', 68, 'Свободна');

-- Договоры (с ручным указанием ID для связи с позициями)
INSERT INTO "public"."Rentals" ("RentalID", "ClientID", "EmployeeID", "StartDate", "PlannedEndDate", "ActualEndDate", "Status", "TotalCost") VALUES
(1, 1, 1, CURRENT_TIMESTAMP - INTERVAL '60 days', CURRENT_TIMESTAMP - INTERVAL '58 days', CURRENT_TIMESTAMP - INTERVAL '58 days', 'Завершен', 15000.00),
(2, 4, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '1 day', NULL, 'Активен', 12500.00);

-- Сброс счетчика ID для Rentals
SELECT setval('"public"."Rentals_RentalID_seq"', 2, true);

-- Позиции аренды
INSERT INTO "public"."RentalItems" ("RentalID", "VehicleID", "HoursCount", "Cost") VALUES
(1, 3, 2, 6000.00),
(1, 4, 3, 9000.00),
(2, 2, 5, 12500.00);

-- Сброс счетчика ID для RentalItems
SELECT setval('"public"."RentalItems_ItemID_seq"', 3, true);

-- =============================================
-- 7. Финальная проверка
-- =============================================
SELECT '✅ База данных успешно восстановлена!' AS Status;
SELECT count(*) AS Clients_Count FROM "public"."Clients";
SELECT count(*) AS Vehicles_Count FROM "public"."Vehicles";
SELECT count(*) AS Rentals_Count FROM "public"."Rentals";
SELECT count(*) AS Items_Count FROM "public"."RentalItems";