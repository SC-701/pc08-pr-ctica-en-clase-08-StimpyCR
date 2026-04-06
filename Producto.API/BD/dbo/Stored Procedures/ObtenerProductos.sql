-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE ObtenerProductos

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT
		Id
      ,IdSubCategoria
      ,Nombre
      ,Descripcion
      ,Precio
      ,Stock
      ,CodigoBarras
  FROM [dbo].[Producto]

END