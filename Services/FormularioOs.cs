using System.Globalization;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;

namespace CadastroClientes.Services;
public class LinhaPeca
{
    public decimal Qtde { get; set; }
    public string Nome { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Causa { get; set; } = "";
    public decimal Valor { get; set; }
}

public class DadosOs
{
    public string Os { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Telefone { get; set; } = "";
    public DateTime? DataOs { get; set; }

    public string Endereco { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Cep { get; set; } = "";

    public bool? Garantia { get; set; }
    public DateTime? DataNf { get; set; }
    public string NroNf { get; set; } = "";
    public string Revenda { get; set; } = "";

    public string Aparelho { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Serie { get; set; } = "";
    public string CondAparelho { get; set; } = "";
    public string Defeito { get; set; } = "";

    public bool? CaboAc { get; set; }
    public bool? Controle { get; set; }
    public bool? CaixaAcs { get; set; }
    public bool? Base { get; set; }
    public bool? Suporte { get; set; }
    public bool? Outros { get; set; }

    public List<LinhaPeca> Pecas { get; set; } = new();
    public decimal MaoDeObra { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataRetirada { get; set; }

    public string Tecnico { get; set; } = "";
    public string ObsTecnico { get; set; } = "";
}

public class FormularioOs
{
    private const float EscX = 1.0113f;
    private const float EscY = 1.03f;
    private const float Margem = 28f;

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private enum Alin { Esq, Centro, Dir }

    private readonly PdfCanvas _c;
    private readonly float _altura;
    private readonly PdfFont _normal;
    private readonly PdfFont _negrito;

    public FormularioOs(PdfCanvas canvas, float alturaPagina, PdfFont normal, PdfFont negrito)
    {
        _c = canvas;
        _altura = alturaPagina;
        _normal = normal;
        _negrito = negrito;
    }

    private float X(float u) => Margem + (u - 14f) * EscX;
    private float Y(float u) => _altura - (Margem + (u - 2f) * EscY);

    private void Linha(float x1, float x2, float y, float esp = 0.7f)
        => _c.SetLineWidth(esp).MoveTo(X(x1), Y(y)).LineTo(X(x2), Y(y)).Stroke();

    private void Vertical(float x, float y1, float y2, float esp = 0.7f)
        => _c.SetLineWidth(esp).MoveTo(X(x), Y(y1)).LineTo(X(x), Y(y2)).Stroke();

    private void Retangulo(float x1, float y1, float x2, float y2, float esp)
        => _c.SetLineWidth(esp).Rectangle(X(x1), Y(y2), X(x2) - X(x1), Y(y1) - Y(y2)).Stroke();

    private void Texto(string? txt, float x, float y, float tam, bool negrito = false,
                       Alin alin = Alin.Esq, float larguraMax = 0, iText.Kernel.Colors.Color? cor = null)
    {
        if (string.IsNullOrWhiteSpace(txt)) return;

        var fonte = negrito ? _negrito : _normal;
        float maxPt = larguraMax * EscX;

        // reduz a fonte até caber na largura disponível
        while (larguraMax > 0 && tam > 5f && fonte.GetWidth(txt, tam) > maxPt)
            tam -= 0.5f;

        float px = X(x);
        float w = fonte.GetWidth(txt, tam);
        if (alin == Alin.Centro) px -= w / 2f;
        else if (alin == Alin.Dir) px -= w;

        _c.SaveState()
          .SetFillColor(cor ?? ColorConstants.BLACK)
          .BeginText()
          .SetFontAndSize(fonte, tam)
          .MoveText(px, Y(y))
          .ShowText(txt)
          .EndText()
          .RestoreState();
    }

    // rótulo + linha + valor digitado em cima da linha
    private void Campo(string rotulo, float xRotulo, float x1, float x2, float yLinha, string? valor)
    {
        Texto(rotulo, xRotulo, yLinha - 3f, 8f, true);
        Linha(x1, x2, yLinha, 0.8f);
        Texto(valor, x1 + 2f, yLinha - 2f, 9f, false, Alin.Esq, x2 - x1 - 4f);
    }

    // linha de data (dd/MM/yyyy ou "/   /" em branco)
    private void LinhaData(float x1, float x2, float y, DateTime? data)
    {
        Linha(x1, x2, y, 0.8f);
        if (data.HasValue)
        {
            Texto(data.Value.ToString("dd/MM/yyyy"), (x1 + x2) / 2f, y - 2f, 9f, false, Alin.Centro);
        }
        else
        {
            float t = (x2 - x1) / 3f;
            Texto("/", x1 + t, y - 2f, 9f, false, Alin.Centro);
            Texto("/", x1 + 2 * t, y - 2f, 9f, false, Alin.Centro);
        }
    }

    // quadradinho, com X quando marcado
    private void Marca(float x, float y, bool marcado)
    {
        float px = X(x), py = Y(y);
        _c.SetLineWidth(0.7f).Rectangle(px, py - 1f, 7f, 7f).Stroke();
        if (marcado)
        {
            _c.SetLineWidth(1f)
              .MoveTo(px + 1.5f, py + 0.5f).LineTo(px + 5.5f, py + 4.5f)
              .MoveTo(px + 1.5f, py + 4.5f).LineTo(px + 5.5f, py + 0.5f)
              .Stroke();
        }
    }

    private void Sim(float x, float y, bool? valor)
    {
        Marca(x, y, valor == true);
        Texto("( )", x + 11f, y, 8f, true);
    }

    private List<string> Quebrar(string? txt, float tam, float largPt, int maxLinhas)
    {
        var linhas = new List<string>();
        if (string.IsNullOrWhiteSpace(txt)) return linhas;

        string atual = "";
        var palavras = txt.Replace("\r", " ").Replace("\n", " ")
                          .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var palavra in palavras)
        {
            string teste = atual.Length == 0 ? palavra : atual + " " + palavra;
            if (atual.Length == 0 || _normal.GetWidth(teste, tam) <= largPt)
                atual = teste;
            else
            {
                linhas.Add(atual);
                atual = palavra;
            }
        }
        if (atual.Length > 0) linhas.Add(atual);

        return linhas.Take(maxLinhas).ToList();
    }

    public void Desenhar(DadosOs d)
    {
        var vermelho = ColorConstants.RED;

        // moldura externa
        Retangulo(14, 2, 547, 760, 1.2f);

        // ===== CABEÇALHO =====
        Vertical(370, 2, 93);
        Linha(14, 547, 93, 1.5f);

        Texto("ELECON ASSISTÊNCIA TÉCNICA", 192, 22, 18, true, Alin.Centro);
        Linha(70, 313, 26, 1f);
        Texto("AILTON - COM. PRODUTOS ELETRÔNICOS LTDA ME.", 192, 40, 11, true, Alin.Centro, 350);
        Texto("CNPJ 53.575.742/0001-01", 18, 54, 8.5f, true);
        Texto("INSCR. ESTADO 387.033.460.111", 190, 54, 8.5f, true);
        Texto("SAMSUNG - LG - PHILIPS - PHILCO - BRITANIA - LENOXX", 192, 67, 8.5f, true, Alin.Centro, 350);
        Texto("Rua Santa Cruz, 781 - Centro Itu/SP | Fones: (11) 4023-0188 - (11) 4023-5945", 18, 79, 8f, true, Alin.Esq, 350);
        Texto("WhatsApp (11) 9.6477-4702 | E-mail: elecon@terra.com.br", 18, 90, 8f, true, Alin.Esq, 350);

        Texto("ORDEM DE SERVIÇO", 458, 16, 11, true, Alin.Centro);
        Texto(d.Os, 458, 57, 18, true, Alin.Centro, 160, vermelho);

        // ===== DADOS DO CLIENTE =====
        Campo("Nome", 21, 57, 280, 120, d.Nome);
        Campo("Telefone", 284, 322, 410, 120, d.Telefone);
        Texto("Data O.S.", 414, 117, 8, true);
        LinhaData(458, 532, 120, d.DataOs);

        Campo("Ender.", 21, 52, 368, 147, d.Endereco);
        Campo("Bairro", 372, 402, 532, 147, d.Bairro);

        Campo("Cidade", 21, 52, 146, 173, d.Cidade);
        Campo("CEP", 151, 168, 232, 173, d.Cep);
        Texto("Garantia", 237, 170, 8, true);
        Sim(272, 170, d.Garantia);
        Vertical(358, 161, 177);
        Texto("Data N.F.", 363, 170, 8, true);
        LinhaData(402, 532, 173, d.DataNf);

        Campo("Nº N.F.", 21, 62, 190, 199, d.NroNf);
        Campo("Revend", 243, 278, 532, 199, d.Revenda);

        Campo("Aparelho", 21, 64, 281, 225, d.Aparelho);
        Campo("Modelo", 288, 326, 532, 225, d.Modelo);

        Campo("Série", 21, 59, 281, 251, d.Serie);
        Campo("Cond. Do Aparelho", 286, 366, 532, 251, d.CondAparelho);

        // Sintoma (defeito): 2 linhas
        Texto("Sintoma (defeito)", 21, 274, 8, true);
        Linha(103, 532, 277, 0.8f);
        Linha(103, 532, 291, 0.8f);
        var defeito = Quebrar(d.Defeito, 9f, (532 - 105) * EscX, 2);
        for (int i = 0; i < defeito.Count; i++)
            Texto(defeito[i], 105, 275 + i * 14, 9);

        // acessórios
        Texto("Cabo AC", 18, 313, 8, true); Sim(66, 313, d.CaboAc);
        Texto("Base", 18, 328, 8, true); Sim(66, 328, d.Base);
        Texto("Controle", 196, 313, 8, true); Sim(248, 313, d.Controle);
        Texto("Suporte", 196, 328, 8, true); Sim(248, 328, d.Suporte);
        Texto("Caixa Acs", 370, 313, 8, true); Sim(422, 313, d.CaixaAcs);
        Texto("Outros", 370, 328, 8, true); Sim(422, 328, d.Outros);

        // ===== TABELA DE PEÇAS =====
        const float topo = 345, corpo = 371, alturaLinha = 13, fim = 501;
        int maxLinhas = (int)((fim - corpo) / alturaLinha);   // 10

        Linha(14, 547, topo, 1.5f);
        Linha(14, 547, corpo, 1.2f);
        Linha(14, 547, fim, 1.2f);
        foreach (var x in new float[] { 58, 190, 324, 413 })
            Vertical(x, topo, fim);
        for (int i = 1; i < maxLinhas; i++)
            Linha(14, 547, corpo + i * alturaLinha, 0.5f);

        Texto("Qtd.", 36, 363, 11, true, Alin.Centro);
        Texto("Peça", 124, 363, 11, true, Alin.Centro);
        Texto("Código da peça", 257, 363, 11, true, Alin.Centro);
        Texto("Preço à vista", 480, 363, 11, true, Alin.Centro);

        for (int i = 0; i < d.Pecas.Count && i < maxLinhas; i++)
        {
            var p = d.Pecas[i];
            float yb = corpo + i * alturaLinha + 9.5f;
            Texto(p.Qtde.ToString("0.##", PtBr), 36, yb, 8, false, Alin.Centro);
            Texto(p.Nome, 61, yb, 8, false, Alin.Esq, 126);
            Texto(p.Codigo, 193, yb, 8, false, Alin.Esq, 128);
            Texto(p.Valor.ToString("C2", PtBr), 543, yb, 8, false, Alin.Dir);
        }

        // ===== VALORES / APROVAÇÃO =====
        decimal valorPecas = d.Pecas.Sum(p => p.Qtde * p.Valor);
        decimal total = valorPecas + d.MaoDeObra;

        Vertical(413, 501, 592);
        foreach (var y in new float[] { 514, 527, 541, 553, 566 })
            Linha(413, 547, y);

        Texto("Valor das peças", 480, 511, 8, true, Alin.Centro);
        Texto(valorPecas > 0 ? valorPecas.ToString("C2", PtBr) : "", 543, 524, 9, false, Alin.Dir);
        Texto("Mão de Obra", 480, 538, 8, true, Alin.Centro);
        Texto(d.MaoDeObra > 0 ? d.MaoDeObra.ToString("C2", PtBr) : "", 543, 550, 9, false, Alin.Dir);
        Texto("TOTAL FINAL", 480, 563, 8, true, Alin.Centro);
        Texto(total > 0 ? total.ToString("C2", PtBr) : "", 543, 585, 11, true, Alin.Dir);

        Texto("Aprovação do orçamento", 30, 538, 8, true);
        LinhaData(135, 205, 541, d.DataAprovacao);
        Texto("Data de retirada", 247, 538, 8, true);
        LinhaData(318, 392, 541, d.DataRetirada);

        Texto("Ass. do cliente", 30, 578, 8, true);
        Linha(102, 323, 580, 0.8f);

        // ===== TÉCNICO =====
        Linha(14, 547, 592, 1.2f);
        Linha(14, 547, 607, 0.8f);
        Vertical(280, 592, 673);

        Texto("Técnico", 147, 603, 8, true, Alin.Centro);
        Texto("Descrição do conserto efetuado / Obs. Do Técnico", 413, 603, 8, true, Alin.Centro);

        var tec = Quebrar(d.Tecnico, 9f, (276 - 18) * EscX, 3);
        for (int i = 0; i < tec.Count; i++)
            Texto(tec[i], 18, 620 + i * 13, 9);

        var obs = Quebrar(d.ObsTecnico, 9f, (544 - 286) * EscX, 4);
        foreach (var y in new float[] { 620, 633, 646, 659 })
            Linha(284, 544, y, 0.5f);
        for (int i = 0; i < obs.Count; i++)
            Texto(obs[i], 286, 618 + i * 13, 9);

        // ===== CANHOTOS (destacar) =====
        Linha(14, 547, 673, 1.2f);
        Vertical(191, 673, 760);
        Vertical(370, 673, 760);

        foreach (var cx in new float[] { 102, 281, 458 })
        {
            Texto($"OS {d.Os}", cx, 714, 16, true, Alin.Centro, 150, vermelho);
            Texto(d.Nome, cx, 735, 8, false, Alin.Centro, 160);
        }
    }
}