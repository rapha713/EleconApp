using CadastroClientes.Data;
using CadastroClientes.Helpers;
using CadastroClientes.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;

namespace CadastroClientes.Services
{
    public class RelatorioService
    {
        private readonly BancoLocal _bancoLocal;

        public RelatorioService(BancoLocal banco) => _bancoLocal = banco;
        private enum TipoLinha { Nenhuma, Abaixo, Preencher }

        public class CabecalhoEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _fonte;

            public CabecalhoEventHandler(PdfFont fonte) => _fonte = fonte;

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent currentEvent)
            {
                var docEvent = (PdfDocumentEvent)currentEvent;
                var pdf = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var tam = page.GetPageSize();

                var pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

                float centroX = (tam.GetLeft() + tam.GetRight()) / 2;
                float tituloY = tam.GetTop() - 40;
                float linhaY = tam.GetTop() - 48;

                using (var canvas = new Canvas(pdfCanvas, tam))
                {
                    canvas.ShowTextAligned(new Paragraph("ELECON ASSISTÊNCIA TÉCNICA").SetFont(_fonte).SetFontSize(16),
                        centroX, tituloY, iText.Layout.Properties.TextAlignment.CENTER);
                }

                pdfCanvas.SetLineWidth(1f).MoveTo(tam.GetLeft() + 36, linhaY).LineTo(tam.GetRight() - 36, linhaY).Stroke();
            }
        }

        #region Canhoto
        public async Task<string> GerarNroOsAsync(Cliente cliente)
        {
            string caminho = System.IO.Path.Combine(FileSystem.CacheDirectory, $"Primeira_OS_{cliente.OS}.pdf");
            var textoCanhoto = (await _bancoLocal.ObterTextosCanhoto(1)).FirstOrDefault()?.Texto ?? string.Empty;
            var textoObsCanhoto = (await _bancoLocal.ObterTextosCanhoto(2)).FirstOrDefault()?.Texto ?? string.Empty;

            await Task.Run(() =>
            {
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf, PageSize.A4.Rotate());

                var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                string data = cliente.DtaEmissao.ToString("dd/MM/yyyy");

                var cabecalho = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1 })).UseAllAvailableWidth();

                iText.Layout.Element.Cell Esq(string txt, float tam, PdfFont fonte) => new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(txt).SetFont(fonte).SetFontSize(tam));

                iText.Layout.Element.Cell Dir(string txt, float tam, PdfFont fonte) => new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .Add(new Paragraph(txt).SetFont(fonte).SetFontSize(tam).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                cabecalho.AddCell(Esq("ELECON ASSISTÊNCIA TÉCNICA", 16, negrito));
                cabecalho.AddCell(Dir($"OS {cliente.OS}", 16, negrito).SetFontColor(new DeviceRgb(255, 0, 0)));

                cabecalho.AddCell(Esq("Rua Santa Cruz, 781 - Centro - Itu/SP\n" +
                    "Fones: (11) 4023-0188 - (11) 4023-5945 / Whats 96477-4702", 9, normal));
                cabecalho.AddCell(Dir($"Data: {data}", 14, normal));

                cabecalho.AddCell(Esq("E-mail: elecon@terra.com.br", 9, normal));
                cabecalho.AddCell(new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                doc.Add(cabecalho);

                doc.Add(new Paragraph($"APARELHO: {cliente.MarcaAparelhoTexto}").SetFont(negrito).SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMarginTop(15));

                doc.Add(new Paragraph(textoCanhoto).SetFont(normal).SetFontSize(10).SetMarginTop(10));

                doc.Add(new Paragraph($"OBS.: {textoObsCanhoto}").SetFont(normal).SetFontSize(10));
                doc.Add(new Paragraph("").SetBorderBottom(new SolidBorder(1)));

                var destacavel = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1 })).UseAllAvailableWidth().SetMarginTop(25);
                iText.Layout.Element.Cell Via(bool ultima)
                {
                    var cell = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(6);

                    if (!ultima)
                        cell.SetBorderRight(new DashedBorder(ColorConstants.BLACK, 1));

                    cell.Add(new Paragraph($"Ordem de Serviço").SetFont(normal).SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    cell.Add(new Paragraph($"{cliente.OS}").SetFont(negrito).SetFontColor(new DeviceRgb(255,0,0)).SetFontSize(20)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    return cell;
                }

                destacavel.AddCell(Via(false));
                destacavel.AddCell(Via(false));
                destacavel.AddCell(Via(true));

                doc.Add(destacavel);
            });

            return caminho;
        }
        #endregion

        #region OsFiltros
        public async Task<string> GerarOsFiltradaAsync(IEnumerable<Cliente> clientes)
        {
            string caminho = System.IO.Path.Combine(FileSystem.CacheDirectory, $"OS_filtradas.pdf");
            var ptBr = CultureInfo.GetCultureInfo("pt-BR");

            var dados = new List<(Cliente Cliente, List<Peca> Pecas)>();
            foreach (var cliente in clientes)
            {
                var pecas = await _bancoLocal.ObterPecasAsync(cliente.OS);
                dados.Add((cliente, pecas.Where(x => x.CodigoPeca != "0").ToList()));
            }

            await Task.Run(() =>
            {
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf);

                var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, new CabecalhoEventHandler(negrito));
                doc.SetMargins(70, 36, 36, 36);

                bool primeira = true;

                foreach (var (cliente, pecas) in dados)
                {
                    if (!primeira)
                        doc.Add(new Paragraph("").SetBorder(new SolidBorder(ColorConstants.BLACK, 1f)));
                    primeira = false;

                    AdicionarOs(doc, cliente, pecas, normal, negrito, ptBr);
                }
            });

            return caminho;
        }

        private static void AdicionarOs(Document doc, Cliente cliente, List<Peca> pecas, PdfFont normal, PdfFont negrito, CultureInfo ptBr)
        {
            string data = cliente.DtaEmissao.ToString("dd/MM/yyyy");


            iText.Layout.Element.Cell Esq(string txt, float tam, PdfFont fonte) =>
                new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(txt).SetFont(fonte).SetFontSize(tam));

            iText.Layout.Element.Cell Cen(string txt, float tam, PdfFont fonte) =>
                new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(txt).SetFont(fonte).SetFontSize(tam)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            iText.Layout.Element.Cell Dir(string txt, float tam, PdfFont fonte) =>
                new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(txt).SetFont(fonte).SetFontSize(tam)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
            
            var osData = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth();
            osData.AddCell(Esq($"OS {cliente.OS}", 14, negrito).SetFontColor(new DeviceRgb(255, 0, 0)));
            osData.AddCell(Dir($"Data: {data}", 14, normal));
            doc.Add(osData);

            var cabecalho = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 2 })).UseAllAvailableWidth();

            string dataCliente = FormatarData(cliente.DtaRetirada);

            cabecalho.AddCell(Esq($"Nome: {cliente.Nome}", 9, normal));
            cabecalho.AddCell(Cen($"Telefone: {cliente.Telefone}", 9, normal));
            cabecalho.AddCell(Dir($"{cliente.Andamento} - {dataCliente}", 9, normal));

            cabecalho.AddCell(Esq($"Aparelho: {cliente.MarcaAparelhoTexto}", 9, normal));
            cabecalho.AddCell(Cen($"Código: {cliente.Codigo}", 9, normal));
            cabecalho.AddCell(Dir($"Nro Série: {cliente.NroSerie}", 9, normal));

            doc.Add(cabecalho);

            doc.Add(new Paragraph($"Defeito: {cliente.Defeito}").SetFont(negrito).SetFontSize(11)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT).SetMarginTop(15));

            doc.Add(new Paragraph("Peças:").SetFont(negrito).SetFontSize(11)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT).SetMarginTop(15));

            var tabela = new Table(UnitValue.CreatePercentArray(new float[] { 1f, 2f, 3f, 1f }))
                .UseAllAvailableWidth().SetMarginTop(10);

            iText.Layout.Element.Cell Cab(string txt) =>
                new iText.Layout.Element.Cell().Add(new Paragraph(txt).SetFont(negrito));

            iText.Layout.Element.Cell Cel(string txt, bool direita = false)
            {
                var p = new Paragraph(txt).SetFont(normal);
                if (direita)
                    p.SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                return new iText.Layout.Element.Cell().Add(p);
            }

            tabela.AddHeaderCell(Cab("QTDE"));
            tabela.AddHeaderCell(Cab("PEÇA"));
            tabela.AddHeaderCell(Cab("CÓDIGO PEÇA"));
            tabela.AddHeaderCell(Cab("VALOR"));

            foreach (var p in pecas)
            {
                tabela.AddCell(Cel(p.Qtde.ToString()));
                tabela.AddCell(Cel(p.Nome ?? string.Empty));
                tabela.AddCell(Cel(p.CodigoPeca ?? string.Empty));
                tabela.AddCell(Cel(p.Valor.ToString("C2", ptBr), direita: true));
            }

            doc.Add(tabela);

            decimal valorFinal = pecas.Sum(p => p.Qtde * p.Valor);

            doc.Add(new Paragraph($"VALOR FINAL: {valorFinal.ToString("C2", ptBr)}")
                .SetFont(negrito).SetFontSize(11)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetMarginTop(15));
        }
        #endregion

        #region Os
        public async Task<string> GerarPdfOsAsync(Cliente cliente)
        {
            var pecas = await _bancoLocal.ObterPecasAsync(cliente.OS);
            var marcas = await _bancoLocal.ObterMarcasAsync();
            var tecnico = await _bancoLocal.ObterNovosFuncionariosAsync(pecas.FirstOrDefault()?.Tecnico ?? 0);
            string marcaTexto = string.Join(" - ", marcas.Where(x => !string.IsNullOrWhiteSpace(x.Nome))
                .OrderBy(x => x.Id).Take(5).Select(x => x.Nome));

            string caminho = System.IO.Path.Combine(FileSystem.CacheDirectory, $"OS_{cliente.OS}.pdf");
            await Task.Run(() =>
            {
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf, PageSize.A4);
                doc.SetMargins(12, 12, 12, 12);

                AdicionarCabecalho(doc, caminho, cliente.OS, marcaTexto);
                AdicionarDados(doc, caminho, cliente);
                AdicionarPeças(doc, caminho, pecas, cliente);
                AdicionarDadosTecnico(doc, caminho, cliente, tecnico);
            });

            return caminho;
        }

        public void AdicionarCabecalho(Document doc, string caminho, int OS, string marcas)
        {
            var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            Paragraph Linha(string texto, float tamanho) => new Paragraph(texto).SetFont(negrito)
                .SetFontSize(tamanho).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMargin(0);

            var cabecalho = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1 })).UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));

            var esquerda = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1)).SetPadding(6);

            esquerda.Add(Linha("ELECON ASSISTÊNCIA TÉCNICA", 20));
            esquerda.Add(Linha("AILTON - COM. PRODUTOS ELETRÔNICOS LTDA ME.", 12));
            esquerda.Add(Linha("CNPJ 53.575.742/0001-01        INSCR. ESTADO 387.033.460.111", 10));
            esquerda.Add(Linha(marcas, 10));
            esquerda.Add(Linha("Rua Santa Cruz, 781 - Centro Itu/SP | Fones: (11) 4023-0188 - (11) 4023-5945", 8));
            esquerda.Add(Linha("WhatsApp (11) 9.6477-4702 | E-mail: elecon@terra.com.br", 8));

            var direita = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE).SetPadding(6);

            direita.Add(Linha("OS", 14));
            direita.Add(Linha($"{OS}", 22).SetFontColor(new DeviceRgb(255, 0, 0)));

            cabecalho.AddCell(esquerda);
            cabecalho.AddCell(direita);

            doc.Add(cabecalho);
        }

        public void AdicionarDados(Document doc, string caminho, Cliente cliente)
        {
            var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            float larguraUtil = doc.GetPdfDocument().GetDefaultPageSize().GetWidth() - doc.GetLeftMargin() - doc.GetRightMargin();
            float larguraColuna = larguraUtil / 3;
            const float padding = 3;
            const float tamanhoFonte = 10;

            iText.Layout.Element.Cell Campo(string rotulo, string? valor, int colunas = 1, bool linha = false)
            {
                var celula = new iText.Layout.Element.Cell(1, colunas).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(padding);

                if (string.IsNullOrEmpty(rotulo))
                    return celula;

                if (!linha)
                {
                    celula.Add(new Paragraph().Add(new Text($"{rotulo}: ").SetFont(negrito)).Add(new Text(valor ?? string.Empty).SetFont(normal))
                        .SetFontSize(tamanhoFonte).SetMargin(0).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                    return celula;
                }

                string textoRotulo = $"{rotulo}: ";
                float larguraCelula = larguraColuna * colunas - (padding * 2);
                float larguraRotulo = negrito.GetWidth(textoRotulo, tamanhoFonte) + 4;
                float percentualRotulo = Math.Min(larguraRotulo / larguraCelula * 100f, 60f);

                var interna = new Table(UnitValue.CreatePercentArray(new float[] { percentualRotulo, 100f - percentualRotulo }))
                    .UseAllAvailableWidth();

                interna.AddCell(new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0).SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.BOTTOM)
                    .Add(new Paragraph(textoRotulo).SetFont(negrito).SetFontSize(tamanhoFonte).SetMargin(0)));

                interna.AddCell(new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f)).SetPadding(0).SetPaddingLeft(2)
                    .SetMinHeight(tamanhoFonte + 6).SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.BOTTOM)
                    .Add(new Paragraph(valor ?? string.Empty).SetFont(normal).SetFontSize(tamanhoFonte).SetMargin(0)));

                celula.Add(interna);

                return celula;
            }

            var dados = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 2 })).UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));

            string telefone = Mascara.Aplicar(TipoMascara.Telefone, cliente.Telefone);
            string cep = Mascara.Aplicar(TipoMascara.CEP, cliente.CEP.ToString());

            dados.AddCell(Campo("Nome", cliente.Nome, colunas: 2, linha: true));
            dados.AddCell(Campo("Telefone", telefone, linha: true));

            dados.AddCell(Campo("Endereço", cliente.Endereco, colunas:2, linha: true));
            dados.AddCell(Campo("Bairro", cliente.Bairro, linha: true));

            dados.AddCell(Campo("Cidade", cliente.CidadeTexto, linha: true));
            dados.AddCell(Campo("CEP", cep, linha: true));
            dados.AddCell(Campo("Emissão", $"{cliente.DtaEmissao:dd/MM/yyyy}"));

            dados.AddCell(Campo("Aparelho", cliente.AparelhoMarcaTexto, linha: true));
            dados.AddCell(Campo("Código", cliente.Codigo?.ToString(), linha: true));
            dados.AddCell(Campo("Série", cliente.NroSerie, linha: true));

            dados.AddCell(Campo("Sintoma (defeito)", cliente.Defeito, colunas: 3, linha: true));

            dados.AddCell(Campo("Cond. Aparelho", cliente.CondAparelho, colunas: 3, linha: true));

            if(cliente.Garantia == false)
            {
                dados.AddCell(Campo("Revendedor", "-"));
                dados.AddCell(Campo("Data Compra", "-"));
                dados.AddCell(Campo("Nro NF", "-"));
            }
            else
            {
                dados.AddCell(Campo("Revendedor", cliente.Revenda, linha: true));
                dados.AddCell(Campo("Data Compra", $"{cliente.DtaCompra:dd/MM/yyyy}", linha: true));
                dados.AddCell(Campo("Nro NF", cliente.NroNf?.ToString(), linha: true));
            }

            dados.AddCell(Campo("Cabo AC", Formatacao.SimOuTraco(cliente.CaboAc)));
            dados.AddCell(Campo("Controle", Formatacao.SimOuTraco(cliente.CR)));
            dados.AddCell(Campo("Caixa Acus", Formatacao.SimOuTraco(cliente.CaixaAcust)));

            dados.AddCell(Campo("Base", Formatacao.SimOuTraco(cliente.Base)));
            dados.AddCell(Campo("Suporte", Formatacao.SimOuTraco(cliente.Suporte)));
            dados.AddCell(Campo("Outros", cliente.Outros));

            doc.Add(dados);
        }

        public void AdicionarPeças(Document doc, string caminho, IEnumerable<Peca> pecas, Cliente cliente)
        {
            const float alturaLinha = 15;
            var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var ptBr = CultureInfo.GetCultureInfo("pt-BR");

            var tabela = new Table(UnitValue.CreatePercentArray(new float[] { 1f, 2f, 3f, 1f })).UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));

            iText.Layout.Element.Cell Cabecalho(string texto) => new iText.Layout.Element.Cell().Add(new Paragraph(texto)
                .SetMinHeight(alturaLinha).SetFont(negrito).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(11));

            iText.Layout.Element.Cell Celula(string? texto) => new iText.Layout.Element.Cell().Add(new Paragraph(texto ?? string.Empty)
                .SetMinHeight(alturaLinha).SetFont(normal).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT).SetFontSize(9));

            tabela.AddHeaderCell(Cabecalho("QTDE"));
            tabela.AddHeaderCell(Cabecalho("PEÇA"));
            tabela.AddHeaderCell(Cabecalho("CÓDIGO PEÇA"));
            tabela.AddHeaderCell(Cabecalho("VALOR"));

            var lista = pecas.ToList();

            var pecasValor = lista.Where(x => x.CodigoPeca != "0");

            decimal valorPecas = lista.Where(x => x.CodigoPeca != "0").Sum(p => p.Valor * p.Qtde);
            decimal maoDeObra = lista.Where(x => x.CodigoPeca == "0").Sum(m => m.Valor);
            decimal valorFinal = valorPecas + maoDeObra;

            const int linhasMinimas = 10;
            var pecasCadastradas = lista.Where(x => x.CodigoPeca != "0").ToList();
            int totalLinhas = Math.Max(linhasMinimas, pecasCadastradas.Count);

            for (int i = 0; i < totalLinhas; i++)
            {
                var p = i < pecasCadastradas.Count ? pecasCadastradas[i] : null;

                tabela.AddCell(Celula(p?.Qtde.ToString() ?? string.Empty));
                tabela.AddCell(Celula(p?.Nome ?? string.Empty));
                tabela.AddCell(Celula(p?.CodigoPeca ?? string.Empty));
                if (p?.Qtde > 1 && p?.Qtde.ToString() != null)
                    tabela.AddCell(Celula((p.Valor * p.Qtde).ToString("C2", ptBr)));
                else
                    tabela.AddCell(Celula(p?.Valor.ToString("C2", ptBr) ?? string.Empty));
            }

            doc.Add(tabela);

            Paragraph Linha(string texto, float tamanho) => new Paragraph(texto).SetFont(negrito)
                .SetFontSize(tamanho).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMargin(0);

            var divisoria = new SolidBorder(ColorConstants.BLACK, 1);
            var total = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2 })).UseAllAvailableWidth().SetKeepTogether(true)
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));

            var datas = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderRight(divisoria).SetPadding(3);

            var primeira = pecas.FirstOrDefault();
            string txtData = "Data Aprovação: ";
            string dataSelecionado = FormatarData(primeira?.DtaAprovacao);

            if (cliente.Andamento == "Aprovado" || cliente.Andamento == "Entregue")
            {
                txtData = "Data Aprovação:";
                dataSelecionado = FormatarData(primeira?.DtaAprovacao);
            }
            else if (cliente.Andamento == "Reprovado" || cliente.Andamento == "Entregue(Reprovado)")
            {
                txtData = "Data Reprovação:";
                dataSelecionado = FormatarData(primeira?.DtaReprovacao);
            }

            datas.Add(Linha($"{txtData} {dataSelecionado}       Data Retirada: {FormatarData(cliente.DtaRetirada)}", 11));

            var direita = new iText.Layout.Element.Cell(2, 1).SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE).SetPadding(6);

            direita.Add(Linha("Valor Peças", 11));
            direita.Add(Linha(valorPecas.ToString("C2", ptBr), 11));
            direita.Add(Linha("Valor Mão de Obra", 11).SetMarginTop(6));
            direita.Add(Linha(maoDeObra.ToString("C2", ptBr), 11));
            direita.Add(new LineSeparator(new SolidLine(0.5f)).SetMarginTop(8).SetMarginBottom(8));
            direita.Add(Linha("TOTAL ORÇAMENTO", 12).SetFontColor(new DeviceRgb(255, 0, 0)));
            direita.Add(Linha(valorFinal.ToString("C2", ptBr), 12));

            var assinatura = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBorderRight(divisoria)
                .SetPaddingTop(6).SetPaddingBottom(6).SetPaddingLeft(30).SetPaddingRight(30)
                .SetMinHeight(60).SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.BOTTOM);

            assinatura.Add(new LineSeparator(new SolidLine(0.5f)));
            assinatura.Add(Linha("Ass. cliente", 11));

            total.AddCell(datas);
            total.AddCell(direita);
            total.AddCell(assinatura);

            doc.Add(total);
        }

        private static string FormatarData(DateTime? data) => data.HasValue && data.Value.Year > 1900 ? data.Value.ToString("dd/MM/yyyy") : "___/___/___";

        public void AdicionarDadosTecnico(Document doc, string caminho, Cliente cliente, IEnumerable<Funcionarios> tecnico, int linhasObservacao = 4)
        {
            var negrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            Paragraph Linha(string texto, float tamanho) => new Paragraph(texto).SetFont(negrito)
                .SetFontSize(tamanho).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetMargin(0);

            var divisoria = new SolidBorder(ColorConstants.BLACK, 1);

            var total = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2 })).UseAllAvailableWidth()
                .SetKeepTogether(true).SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));

            var assinatura = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderRight(divisoria).SetPaddingTop(6).SetPaddingBottom(6).SetPaddingLeft(30).SetPaddingRight(30)
                .SetMinHeight(60).SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.BOTTOM);

            assinatura.Add(Linha(tecnico.FirstOrDefault()?.Nome ?? "", 11));
            assinatura.Add(new LineSeparator(new SolidLine(0.5f)));
            assinatura.Add(Linha("Técnico responsável", 11));

            var direita = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPadding(6);

            direita.Add(Linha("Observações", 12).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));
            direita.Add(Linha($"{cliente.Observacao}", 10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

            total.AddCell(assinatura);
            total.AddCell(direita);

            doc.Add(total);
        }
        #endregion
    }
}