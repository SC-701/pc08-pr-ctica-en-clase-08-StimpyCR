

CREATE PROCEDURE EditarProducto
    @Id UNIQUEIDENTIFIER,
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

    UPDATE dbo.Producto
    SET
        IdSubCategoria = @IdSubCategoria,
        Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock,
        CodigoBarras = @CodigoBarras
    WHERE Id = @Id;
    select @Id
    commit transaction
END;