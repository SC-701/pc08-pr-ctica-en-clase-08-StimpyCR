

CREATE PROCEDURE AgregarProducto
    @Id UNIQUEIDENTIFIER ,
    @IdSubCategoria UNIQUEIDENTIFIER,
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(500),
    @Precio DECIMAL(18,2),
    @Stock INT,
    @CodigoBarras VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    begin transaction
    INSERT INTO dbo.Producto
    (
		Id,
        IdSubCategoria,
        Nombre,
        Descripcion,
        Precio,
        Stock,
        CodigoBarras
    )
    VALUES
    (	@Id,
        @IdSubCategoria,
        @Nombre,
        @Descripcion,
        @Precio,
        @Stock,
        @CodigoBarras
    );
    select @Id
    commit transaction
END;