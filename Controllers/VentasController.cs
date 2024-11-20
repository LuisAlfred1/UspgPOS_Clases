using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UspgPOS.Data;
using UspgPOS.Models;

namespace UspgPOS.Controllers
{
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Ventas.Include(v => v.Cliente).Include(v => v.Sucursal);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", HttpContext.Session.GetString("SucursalId"));
            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,Total,ClienteId,SucursalId")] Venta venta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long? id, [Bind("Id,Fecha,Total,ClienteId,SucursalId")] Venta venta)
        {
            if (id != venta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", venta.ClienteId);
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Nombre", venta.SucursalId);
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long? id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                _context.Ventas.Remove(venta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(long? id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }

        public IActionResult ImprimirFactura(long id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var venta = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Sucursal)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/icons/apple-touch-icon-180x180.png");
            var monedaGuatemala = new System.Globalization.CultureInfo("es-GT");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    page.Header().Row(header =>
                    {
                        //Fecha
                        header.RelativeItem().Row(row =>
                        {
                            row.RelativeItem().Text($"Fecha: {venta.Fecha.ToString("dd/MM/yyyy")}");
                        });

                        // Sección central: Título
                        header.RelativeItem().Column(column =>
                        {
                            column.Item().PaddingTop(2, Unit.Centimetre)
                                .PaddingLeft(-4, Unit.Centimetre)
                                .Text("FACTURA COMERCIAL")
                                .FontFamily("Times New Roman")
                                .FontSize(36).Bold()
                                .AlignCenter();
                        });

                        //logo
                        header.RelativeItem().AlignRight().Image(logo, ImageScaling.FitArea);

                    });

                    page.Content().Column(column =>
                    {

                        column.Item().PaddingVertical(0.5f, Unit.Centimetre).Row(row =>
                        {
                            row.RelativeItem().Text("NOMBRE:").FontSize(10).Bold();
                            row.RelativeItem().Text(venta.Cliente?.Nombre ?? "N/A").FontSize(12);

                            row.RelativeItem().Text("TELÉFONO:").FontSize(10).Bold();
                            row.RelativeItem().Text("+502 9645-2315").FontSize(12);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("DIRECCIÓN:").FontSize(10).Bold();
                            row.RelativeItem().Text("5ta Av. 5-59 Zona 12 ").FontSize(12);

                            row.RelativeItem().Text("SUCURSAL:").FontSize(10).Bold();
                            row.RelativeItem().Text(venta.Sucursal.Nombre ?? "N/A").FontSize(12);
                        });

                        column.Item().PaddingVertical(1, Unit.Centimetre);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloCelda).Text("Cantidad").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Descripcion").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Precio Unitario").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Subtotal").FontSize(12).Bold();

                                static IContainer EstiloCelda(IContainer container)
                                {
                                    return container.Background("#e0e0e0").Border(1).BorderColor("#e0e0e0").Padding(5).AlignCenter();
                                }

                            });


                            foreach (DetalleVenta detalle in venta.DetallesVenta)
                            {
                                table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignRight().Text(detalle.Cantidad.ToString());
                                table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignCenter().Text(detalle.Producto?.Nombre ?? "N/A");
                                table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignRight().Text(detalle.PrecioUnitario.ToString("C", monedaGuatemala));
                                table.Cell().Border(1).BorderColor("#c0c0c0").Padding(5).AlignRight().Text((detalle.Cantidad * detalle.PrecioUnitario).ToString("C", monedaGuatemala));
                            }

                            table.Cell().ColumnSpan(3).Background("#f0f0f0").Border(1).BorderColor("#c0c0c0").Padding(5).AlignRight()
                                .Text("TOTAL").FontSize(12).Bold();
                            table.Cell().Background("#f0f0f0").Border(1).BorderColor("#c0c0c0").Padding(5).AlignRight()
                                .Text(venta.Total.ToString("C", monedaGuatemala));
                        });

                        column.Item().PaddingVertical(0.5f, Unit.Centimetre).Row(row =>
                        {
                            row.RelativeItem().Text("Direccion:").FontSize(10).Bold();
                            row.RelativeItem().Text("Av.central, zona 15").FontSize(12);

                            row.RelativeItem().Text("Sitio Web:").FontSize(10).Bold();
                            row.RelativeItem().Text("@sitionIncreible").FontSize(12);

                            row.RelativeItem().Text("").FontSize(10).Bold();
                            row.RelativeItem().Text("").FontSize(12);

                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Correo:").FontSize(10).Bold();
                            row.RelativeItem().Text("mundo@gmail.com").FontSize(12);

                            row.RelativeItem().Text("Teléfono:").FontSize(10).Bold();
                            row.RelativeItem().Text("(55) 1251-2369").FontSize(12);

                            row.RelativeItem().Text("Firma del cliente:").FontSize(10).Bold();
                            row.RelativeItem().Text("______________").FontSize(12);
                        });

                    });

                    page.Footer().Row(row =>
                    {
                        // Texto del lado izquierdo: Servicio a Domicilio
                        row.RelativeItem()
                            .PaddingLeft(1, Unit.Centimetre)
                            .Text("SERVICIO A DOMICILIO")
                            .FontFamily("Times New Roman")
                            .FontSize(20)
                            .Bold()
                            .AlignLeft();
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Factura_{id}.pdf");
        }
    }
}
