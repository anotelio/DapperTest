
IF	DB_ID(N'retail_db') IS NULL
	CREATE DATABASE [retail_db]
GO

USE [retail_db]
GO

CREATE SCHEMA [sale] AUTHORIZATION [db_ddladmin]
GO

IF	OBJECT_ID('[sale].[products]', 'U') IS NULL
	CREATE TABLE [sale].[products] (
		product_id 		INT PRIMARY KEY,
		product_name 	NVARCHAR(255) NOT NULL,
		product_type 	TINYINT NOT NULL
	)
GO

IF	OBJECT_ID('[sale].[shipments]', 'U') IS NULL
	CREATE TABLE [sale].[shipments] (
		shipment_id 	INT PRIMARY KEY IDENTITY,
		product_id 		INT NOT NULL,
		quantity 		DECIMAL(9,3) NOT NULL,
		is_delete 		BIT NOT NULL DEFAULT (0),
		created_at_utc 	DATETIME NOT NULL DEFAULT GETUTCDATE()
	)
GO

IF	TYPE_ID('[sale].[t_products]') IS NULL
	CREATE TYPE [sale].[t_products] AS TABLE(
		product_id 		INT NOT NULL,
		product_name 	NVARCHAR(255) NOT NULL,
		product_type 	TINYINT NOT NULL
	)
GO

IF	TYPE_ID('[sale].[t_shipments]') IS NULL
	CREATE TYPE [sale].[t_shipments] AS TABLE(
		product_id 		INT NOT NULL,
		quantity 		DECIMAL(9,3) NOT NULL
	)
GO

DECLARE @products TABLE(
		product_id 	INT NOT NULL,
		product_name 	NVARCHAR(255) NOT NULL,
		product_type 	TINYINT NOT NULL
)

INSERT INTO @products
VALUES
	(101, 'lemonade', 1),
	(102, 'loaf', 2),
	(103, 'milk', 1),
	(104, 'chicken', 4),
	(105, 'turkey', 4),
	(106, 'apple', 3);

MERGE INTO [sale].[products] AS tgt
USING @products AS src
ON tgt.product_id = src.product_id
WHEN MATCHED
	THEN UPDATE
	SET product_name = src.product_name,
		product_type = src.product_type
WHEN NOT MATCHED BY TARGET
	THEN INSERT (product_id, product_name, product_type)
	VALUES (src.product_id, src.product_name, src.product_type)
WHEN NOT MATCHED BY SOURCE
	THEN DELETE;
GO

DECLARE @shipments TABLE(
		shipment_id		INT NOT NULL,
		product_id 		INT NOT NULL,
		quantity 		DECIMAL(9,3) NOT NULL
)

INSERT INTO @shipments
VALUES
	(1, 105, 12),
	(2, 106, 25),
	(3, 101, 30),
	(4, 104, 16.5),
	(5, 102, 7),
	(6, 104, 12.1),
	(7, 103, 4),
	(8, 102, 11),
	(9, 101, 10),
	(10, 104, 10.3),
	(11, 105, 4),
	(12, 106, 15);

SET IDENTITY_INSERT [sale].[shipments] ON;

MERGE INTO [sale].[shipments] AS tgt
USING @shipments AS src
ON tgt.shipment_id = src.shipment_id
WHEN MATCHED
	THEN UPDATE
	SET product_id = src.product_id,
		quantity = src.quantity
WHEN NOT MATCHED BY TARGET
	THEN INSERT (shipment_id, product_id, quantity)
	VALUES (src.shipment_id, src.product_id, src.quantity)
WHEN NOT MATCHED BY SOURCE
	THEN DELETE;

SET IDENTITY_INSERT [sale].[shipments] OFF;

GO

CREATE OR ALTER PROCEDURE [sale].[products_get]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id]		AS ProductId,
		p.[product_name]	AS ProductName,
		p.[product_type]	AS ProductType
	FROM [sale].[products] p;
END;
GO

CREATE OR ALTER PROCEDURE [sale].[products_filter_get]
	@product_name_filter NVARCHAR(10),
	@product_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id],
		p.[product_name],
		p.[product_type]
	FROM [sale].[products] p
	WHERE (
			@product_name_filter IS NULL
			OR p.[product_name] LIKE @product_name_filter)
		AND (
			@product_type IS NULL
			OR p.product_type = @product_type);
END;
GO

CREATE OR ALTER PROCEDURE [sale].[shipment_diff_params_numbers]
	@product_id INT,
	@number_count INT OUTPUT,
	@quantity_sum DECIMAL(10,3) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@number_count += COUNT(*),
		@quantity_sum = ISNULL(SUM(s.[quantity]), 0)
	FROM [sale].[shipments] s
	WHERE s.[product_id] = @product_id;

	IF	(@quantity_sum > 25.000)
		BEGIN
			RETURN 1;
		END;

	RETURN 0;
END;
GO

CREATE OR ALTER PROCEDURE [sale].[product_shipments_multiple]
	@product_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id],
		p.[product_name],
		p.[product_type]
	FROM [sale].[products] p
	WHERE p.[product_id] = @product_id;

	SELECT
		s.[shipment_id],
		s.[product_id],
		s.[quantity],
		s.[is_delete],
		s.[created_at_utc]
	FROM [sale].[shipments] s
	WHERE s.[product_id] = @product_id;

END;
GO

CREATE OR ALTER PROCEDURE [sale].[products_add_tvp]
	@products [sale].[t_products] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [sale].[products]
	(
		product_id,
		product_name,
		product_type
	)
	SELECT
		tp.product_id,
		tp.product_name,
		tp.product_type
	FROM @products tp;

	/*
		[
			{
				"product_id": 107,
				"product_name": "beef",
				"product_type": 5
			},
			{
				"product_id": 108,
				"product_name": "cabbage",
				"product_type": 6
			}
		]

		DELETE [retail_db].[sale].[products]
		WHERE product_type > 4
	*/
END;
GO

CREATE OR ALTER PROCEDURE [sale].[shipments_add_tvp]
	@shipments [sale].[t_shipments] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [sale].[shipments]
	(
		product_id,
		quantity
	)
	OUTPUT inserted.shipment_id
	SELECT
		ts.product_id,
		ts.quantity
	FROM @shipments ts;

	/*
		{
			"product_id": 103,
			"quantity": 4.75
		}

		DELETE [retail_db].[sale].[shipments]
		WHERE shipment_id > 12
	*/
END;
GO

CREATE OR ALTER PROCEDURE [sale].[product_shipments_multi_mapping]
	@product_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id],
		p.[product_name],
		p.[product_type],
		s.[shipment_id],
		s.[quantity],
		s.[is_delete],
		s.[created_at_utc]
	FROM [sale].[products] p
	LEFT JOIN [sale].[shipments] s
	ON p.product_id = s.product_id
	WHERE p.[product_id] = @product_id;
END;
GO

CREATE OR ALTER PROCEDURE [sale].[product_shipments_all_multi_mapping]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id],
		p.[product_name],
		p.[product_type],
		s.[shipment_id],
		s.[quantity],
		s.[is_delete],
		s.[created_at_utc]
	FROM [sale].[products] p
	LEFT JOIN [sale].[shipments] s
	ON p.product_id = s.product_id;
END;
GO

CREATE OR ALTER PROCEDURE [sale].[product_shipments_get_json]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id],
		p.[product_name],
		p.[product_type],
		(
			SELECT
				s.shipment_id,
				s.product_id,
				s.quantity,
				s.is_delete,
				s.created_at_utc
			FROM [sale].[shipments] s
			WHERE p.product_id = s.product_id
			FOR JSON PATH
		) shipments
	FROM [sale].[products] p
	FOR JSON PATH;
END;
GO

CREATE OR ALTER PROCEDURE [sale].[product_shipmentsJson_get]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.[product_id] AS ProductId,
		p.[product_name] AS ProductName,
		p.[product_type] AS ProductType,
		(
			SELECT
				s.shipment_id,
				s.product_id,
				s.quantity,
				s.is_delete,
				s.created_at_utc
			FROM [sale].[shipments] s
			WHERE p.product_id = s.product_id
			FOR JSON PATH
		) shipments
	FROM [sale].[products] p;
END;
GO

/*
DECLARE @Table TABLE(
        SPID INT,
        Status VARCHAR(MAX),
        LOGIN VARCHAR(MAX),
        HostName VARCHAR(MAX),
        BlkBy VARCHAR(MAX),
        DBName VARCHAR(MAX),
        Command VARCHAR(MAX),
        CPUTime INT,
        DiskIO INT,
        LastBatch VARCHAR(MAX),
        ProgramName VARCHAR(MAX),
        SPID_1 INT,
        REQUESTID INT
)

INSERT INTO @Table EXEC sp_who2

SELECT  *
FROM    @Table
WHERE DBName = 'retail_db'
	AND ProgramName LIKE '%.Net%'
*/