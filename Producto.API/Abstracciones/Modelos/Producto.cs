using System;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Modelos
{
    public class ProductoBase
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "La descripción debe tener entre 2 y 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01,7922816251426433759, ErrorMessage = "El precio debe ser un número positivo mayor que 0")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Range(0, 2147483647, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El código de barras es obligatorio")]
        [StringLength(13, MinimumLength = 8, ErrorMessage = "El código de barras debe tener entre 8 y 13 caracteres")]
        [RegularExpression(@"^\d{8,13}$", ErrorMessage = "El código de barras debe contener solo dígitos (8-13)")]
        [DataType(DataType.Text)]
        public string CodigoBarras { get; set; } = string.Empty;

        public class ProductoRequest : ProductoBase
        {
            [Required(ErrorMessage = "La subcategoría es obligatoria")]
            public Guid IdSubCategoria { get; set; }
        }

        public class ProductoResponse : ProductoBase
        {
            [Required]
            public Guid Id { get; set; }

            [Required(ErrorMessage = "La subcategoría es obligatoria")]
            [StringLength(100, ErrorMessage = "El nombre de la subcategoría puede tener hasta 100 caracteres")]
            public string SubCategoria { get; set; } = string.Empty;

            [Required(ErrorMessage = "La categoría es obligatoria")]
            [StringLength(100, ErrorMessage = "El nombre de la categoría puede tener hasta 100 caracteres")]
            public string Categoria { get; set; } = string.Empty;
        }

        public class ProductoDetalle : ProductoResponse
        {
            public string fechaActual { get; set; } = string.Empty;
            public double PrecioDolar { get; set; } // existente
            public double PrecioUSD { get; set; }   // rúbrica
        }
    }
}
