using System.Security.Cryptography;
using System.Text;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Domain.Services;

public static class OperationalProcedureService
{
    public const string CompanyFooter =
        "Rua Justo Manfron, 2845 G, Santa Felicidade, Curitiba - PR - CEP 82.410-540 – CNPJ 81.076.242/0001-60";

    private const int SectionsOnFirstPage = 3;

    public static string DeriveCraneModelCode(string? craneModel, string? craneBrand = null)
    {
        if (string.IsNullOrWhiteSpace(craneModel)) return "-";

        var model = craneModel.Trim();

        if (!string.IsNullOrWhiteSpace(craneBrand))
        {
            var brand = craneBrand.Trim();
            if (model.StartsWith(brand, StringComparison.OrdinalIgnoreCase))
            {
                model = model[brand.Length..].TrimStart(' ', '-', '.', ':');
            }
        }

        var cleaned = new string(model.Where(c => !char.IsWhiteSpace(c) && c != '.').ToArray()).ToUpperInvariant();
        return string.IsNullOrEmpty(cleaned) ? "-" : cleaned;
    }

    public static OperationalProcedureDocument BuildDocument(RiggingPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var craneCode = DeriveCraneModelCode(plan.CraneModel, plan.CraneBrand);
        var sections = BuildSections(craneCode);
        var pages = new List<OperationalProcedurePage>();
        var pageSections = new List<OperationalProcedureSection>();
        var pageNumber = 1;

        foreach (var section in sections)
        {
            pageSections.Add(section);

            if (pageSections.Count == SectionsOnFirstPage)
            {
                pages.Add(new OperationalProcedurePage
                {
                    Number = pageNumber++,
                    Sections = pageSections.ToArray()
                });
                pageSections.Clear();
            }
        }

        if (pageSections.Count > 0)
        {
            pages.Add(new OperationalProcedurePage
            {
                Number = pageNumber,
                Sections = pageSections.ToArray()
            });
        }

        return new OperationalProcedureDocument
        {
            OperationName = FormatValue(plan.OperationName),
            ArtNumber = FormatValue(plan.ArtNumber),
            PlanNumber = FormatValue(plan.PlanNumber),
            CraneModelCode = craneCode,
            Pages = pages
        };
    }

    public static string GenerateContent(RiggingPlan plan) => RenderContent(BuildDocument(plan));

    public static string RenderContent(OperationalProcedureDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sb = new StringBuilder();

        foreach (var page in document.Pages)
        {
            AppendHeader(sb, document);

            foreach (var section in page.Sections)
            {
                sb.AppendLine($"{section.Number}. {section.Title}");
                sb.AppendLine($"Objetivo: {section.Objective}");
                sb.AppendLine("Passos:");
                foreach (var step in section.Steps)
                {
                    sb.AppendLine($"- {step}");
                }
                sb.AppendLine("Referências:");
                foreach (var reference in section.References)
                {
                    sb.AppendLine($"- {reference}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"Rodapé: {CompanyFooter}");
            sb.AppendLine($"Página {page.Number} de {document.Pages.Count}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static PlanProcedureSnapshot CreateSnapshot(RiggingPlan plan, DateTime? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var content = GenerateContent(plan);

        return new PlanProcedureSnapshot
        {
            PlanId = plan.Id,
            CraneModelCode = DeriveCraneModelCode(plan.CraneModel, plan.CraneBrand),
            Content = content,
            ContentSha256 = ComputeSha256(content),
            CreatedAt = createdAt ?? DateTime.Now
        };
    }

    public static string ResolveContent(RiggingPlan plan, PlanProcedureSnapshot? snapshot)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Status == "finalized" && snapshot is not null
            ? snapshot.Content
            : GenerateContent(plan);
    }

    public static string ComputeSha256(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void AppendHeader(StringBuilder sb, OperationalProcedureDocument document)
    {
        sb.AppendLine("Guindastes Ribas");
        sb.AppendLine("PROCEDIMENTO");
        sb.AppendLine($"Operação: {document.OperationName}");
        sb.AppendLine($"ART: {document.ArtNumber}");
        sb.AppendLine($"RIGGING: {document.PlanNumber}");
        sb.AppendLine();
    }

    private static string FormatValue(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();

    private static IReadOnlyList<OperationalProcedureSection> BuildSections(string craneCode) =>
    [
        new OperationalProcedureSection
        {
            Number = 1,
            Title = "ISOLAMENTO DA ÁREA",
            Objective = "Garantir segurança de pessoas e equipamentos durante a operação.",
            Steps =
            [
                "Delimitar e sinalizar a área de risco.",
                "Impedir circulação de pessoas não autorizadas.",
                "Verificar ausência de interferências: rede elétrica, estruturas, veículos.",
                "Liberar área somente após checagem."
            ],
            References =
            [
                "NR-18.5.3 / NR-18.4.2(a) – Isolamento e sinalização de área de movimentação.",
                "NR-11.7.1 / NR-11.7.2 – Espaço de circulação seguro e sinalização de transporte de cargas.",
                "NR-18.15.47 – Proibição de permanência sob cargas suspensas."
            ]
        },
        new OperationalProcedureSection
        {
            Number = 2,
            Title = $"PATOLAMENTO DO GUINDASTE {craneCode}",
            Objective = "Garantir estabilidade e segurança do equipamento de içamento.",
            Steps =
            [
                "Posicionar o guindaste conforme plano.",
                "Avaliar capacidade do solo (RMET) e posicionar os dormentes",
                "Estender e nivelar estabilizadores a 100%."
            ],
            References =
            [
                "NR-18.14.22 / NR-18.14.24 – Estabilidade e operação conforme manuais.",
                "NR-11.5.1 – Equipamentos devem estar em condições seguras e inspecionados.",
                "NR-18.14.4 – Inspeção diária obrigatória dos equipamentos de movimentação."
            ]
        },
        new OperationalProcedureSection
        {
            Number = 3,
            Title = "POSICIONAMENTO DO VEÍCULO DE CARGA",
            Objective = "Facilitar içamento e transporte seguro da carga.",
            Steps =
            [
                "Aproximar carreta ou veículo guiado pelo sinaleiro.",
                "Acionar freio, calçar rodas e desligar motor.",
                "Garantir espaço para manobra do gancho e acesso à carga."
            ],
            References =
            [
                "NR-18.5.5 / NR-18.15.40 – Organização do fluxo de veículos no canteiro.",
                "NR-11.7.2 / NR-11.7.4 – Transporte de cargas e permanência de veículos estacionados."
            ]
        },
        new OperationalProcedureSection
        {
            Number = 4,
            Title = "IÇAMENTO DA CARGA",
            Objective = "Elevar e movimentar carga com segurança e estabilidade.",
            Steps =
            [
                "Verificar cintas, manilhas e pontos de ancoragem.",
                "Montar amarração conforme plano de rigging.",
                "Realizar elevação de teste (10–20 cm).",
                "Elevar carga lentamente, mantendo comunicação com sinaleiro.",
                "Não movimentar carga sobre pessoas.",
                "Respeitar limites do equipamento e condições ambientais (vento, visibilidade)."
            ],
            References =
            [
                "NR-18.14.20 / NR-18.14.25 / NR-18.14.12–18.14.18 – Capacitação, inspeção e comunicação de operação.",
                "NR-18.5.3 / NR-18.15.47 – Proibição de pessoas sob cargas suspensas.",
                "NR-11.6.1 / NR-11.6.2 – Transporte seguro de cargas, estabilidade e movimentação.",
                "NR-11.7.6 – Planejamento da movimentação de cargas."
            ]
        },
        new OperationalProcedureSection
        {
            Number = 5,
            Title = "DESCARGA DA CARGA",
            Objective = "Depositar carga com segurança e estabilidade.",
            Steps =
            [
                "Aproximar lentamente da posição de apoio.",
                "Baixar carga de forma controlada.",
                "Garantir apoio estável antes de soltar amarrações.",
                "Afastar equipe da zona de risco.",
                "Liberar área após conferência final."
            ],
            References =
            [
                "NR-18.4.2(d) / NR-18.5.3 / NR-18.14.26 – Deposição segura e liberação da área.",
                "NR-11.6.3 / NR-11.7.5 – Evitar tombamento, esmagamento ou movimentação insegura."
            ]
        }
    ];
}
