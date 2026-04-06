

CREATE PROCEDURE EliminarProducto
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    begin transaction
    DELETE FROM dbo.Producto
    WHERE Id = @Id;
    select @Id
    commit transaction
END;