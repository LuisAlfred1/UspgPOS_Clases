﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Security.Policy;

namespace UspgPOS.Models
{
    public class Producto
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("codigo")]
        [MaxLength(100)]
        public string? Codigo { get; set; }

        [Column("nombre")]
        [StringLength(255)]
        [Required]
        public string Nombre { get; set; }

        [Column("marca_id")]
        public long MarcaId { get; set; }

        [Column("clasificacion_id")]
        public long ClasificacionId { get; set; }

        [Column("precio")]
        [Required]
        public decimal Precio {  get; set; }

        [Column("cantidad")]
        [Required]
        public int Cantidad {  get; set; }

        [Column("img_url")]
        public string? ImgUrl { get; set; }
        [Column("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        public Marca? Marca { get; set; }
        public Clasificacion? Clasificacion { get; set; }

    }
}
