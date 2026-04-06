


CREATE PROCEDURE ObtenerProducto
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        Id,
        IdSubCategoria,
        Nombre,
        Descripcion,
        Precio,
        Stock,
        CodigoBarras
    FROM dbo.Producto
    WHERE Id = @Id;

END;