using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QRCoder;
using System.Drawing.Imaging;

namespace Api_Empleados.Funciones
{
    public class CarnetGenerador
    {

        public byte[] GenerarCarnetConPlantilla(
            string nombre,
            string nickName,
            string rutaPlantilla)
        {
            using (var ms = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                page.Width = XUnit.FromPoint(792);  // 11 pulgadas
                page.Height = XUnit.FromPoint(612); // 8.5 pulgadas

                var gfx = XGraphics.FromPdfPage(page);
                var fontTitulo = new XFont("Montserrat Classic", 24, XFontStyle.Bold);
                var fontTexto = new XFont("Montserrat Classic", 18, XFontStyle.Regular);

                double pageWidth = page.Width;
                double pageHeight = page.Height;

                // Fondo de plantilla
                using (var bgStream = File.OpenRead(rutaPlantilla))
                {
                    var plantillaImg = XImage.FromStream(() => bgStream);
                    gfx.DrawImage(plantillaImg, 0, 0, page.Width, page.Height);
                }

                // Nombre del usuario (centrado)
                var tituloRect = new XRect(50, (pageHeight / 2) - 30, pageWidth, 40);
                gfx.DrawString(nombre, fontTitulo, XBrushes.White, tituloRect, XStringFormats.TopCenter);

                // Nickname (centrado)
                var nickRect = new XRect(50, (pageHeight / 2), pageWidth, 30);
                gfx.DrawString(nickName, fontTexto, XBrushes.White, nickRect, XStringFormats.TopCenter);

                // Generar código QR
                using var qrGen = new QRCodeGenerator();
                var qrData = qrGen.CreateQrCode($"Usuario:{nickName}", QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                using var qrStream = new MemoryStream(qrBytes);
                var qrImg = XImage.FromStream(() => qrStream);

                // Dibujar código QR (centrado debajo de la foto)
                double qrAncho = 180;
                double qrAlto = 180;
                double qrX = (pageWidth - qrAncho) / 3;
                gfx.DrawImage(qrImg, qrX, 250, qrAncho, qrAlto);

                document.Save(ms, false);
                return ms.ToArray();
            }
        }
    }
}
