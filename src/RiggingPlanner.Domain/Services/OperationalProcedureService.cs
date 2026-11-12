using System.Security.Cryptography;
using System.Text;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Domain.Services;

public static class OperationalProcedureService
{
    public static string DeriveCraneModelCode(string? craneModel)
    {
        if (string.IsNullOrWhiteSpace(craneModel)) return "-";
        var cleaned = new string(craneModel.Where(c => !char.IsWhiteSpace(c) && c != '.').ToArray()).ToUpperInvariant();
        return string.IsNullOrEmpty(cleaned) ? "-" : cleaned;
    }

    public static string GenerateContent(RiggingPlan plan)
    {
        var operationName = !string.IsNullOrWhiteSpace(plan.OperationName) ? plan.OperationName : "-";
        var artNumber = !string.IsNullOrWhiteSpace(plan.ArtNumber) ? plan.ArtNumber : "-";
        var planNumber = !string.IsNullOrWhiteSpace(plan.PlanNumber) ? plan.PlanNumber : "-";
        var craneCode = DeriveCraneModelCode(plan.CraneModel);

        var sb = new StringBuilder();
        sb.AppendLine("PROCEDIMENTO OPERACIONAL");
        sb.AppendLine($"Operação: {operationName}");
        sb.AppendLine($"ART: {artNumber}");
        sb.AppendLine($"RIGGING: {planNumber}");
        sb.AppendLine();
        sb.AppendLine("==================================================");
        sb.AppendLine();
        sb.AppendLine("1. ISOLAMENTO DA ÁREA");
        sb.AppendLine("Objetivo: Garantir segurança de pessoas e equipamentos durante a operação.");
        sb.AppendLine("Passos:");
        sb.AppendLine("- Delimitar e sinalizar a área de risco.");
        sb.AppendLine("- Impedir circulação de pessoas não autorizadas.");
        sb.AppendLine("- Verificar ausência de interferências: rede elétrica, estruturas, veículos.");
        sb.AppendLine("- Liberar área somente após checagem.");
        sb.AppendLine("Referências:");
        sb.AppendLine("- NR-18.5.3 / NR-18.4.2(a) – Isolamento e sinalização de área de movimentação.");
        sb.AppendLine("- NR-11.7.1 / NR-11.7.2 – Espaço de circulação seguro e sinalização de transporte de cargas.");
        sb.AppendLine("- NR-18.15.47 – Proibição de permanência sob cargas suspensas.");
        sb.AppendLine();
        sb.AppendLine($"2. PATOLAMENTO DO GUINDASTE {craneCode}");
        sb.AppendLine("Objetivo: Garantir estabilidade e segurança do equipamento de içamento.");
        sb.AppendLine("Passos:");
        sb.AppendLine("- Posicionar o guindaste conforme plano.");
        sb.AppendLine("- Avaliar capacidade do solo (RMET) e posicionar os dormentes");
        sb.AppendLine("- Estender e nivelar estabilizadores a 100%.");
        sb.AppendLine("Referências:");
        sb.AppendLine("- NR-18.14.22 / NR-18.14.24 – Estabilidade e operação conforme manuais.");
        sb.AppendLine("- NR-11.5.1 – Equipamentos devem estar em condições seguras e inspecionados.");
        sb.AppendLine("- NR-18.14.4 – Inspeção diária obrigatória dos equipamentos de movimentação.");
        sb.AppendLine();
        sb.AppendLine("3. POSICIONAMENTO DO VEÍCULO DE CARGA");
        sb.AppendLine("Objetivo: Facilitar içamento e transporte seguro da carga.");
        sb.AppendLine("Passos:");
        sb.AppendLine("- Aproximar carreta ou veículo guiado pelo sinaleiro.");
        sb.AppendLine("- Acionar freio, calçar rodas e desligar motor.");
        sb.AppendLine("- Garantir espaço para manobra do gancho e acesso à carga.");
        sb.AppendLine("Referências:");
        sb.AppendLine("- NR-18.5.5 / NR-18.15.40 – Organização do fluxo de veículos no canteiro.");
        sb.AppendLine("- NR-11.7.2 / NR-11.7.4 – Transporte de cargas e permanência de veículos estacionados.");
        sb.AppendLine();
        sb.AppendLine("4. IÇAMENTO DA CARGA");
        sb.AppendLine("Objetivo: Elevar e movimentar carga com segurança e estabilidade.");
        sb.AppendLine("Passos:");
        sb.AppendLine("- Verificar cintas, manilhas e pontos de ancoragem.");
        sb.AppendLine("- Montar amarração conforme plano de rigging.");
        sb.AppendLine("- Realizar elevação de teste (10–20 cm).");
        sb.AppendLine("- Elevar carga lentamente, mantendo comunicação com sinaleiro.");
        sb.AppendLine("- Não movimentar carga sobre pessoas.");
        sb.AppendLine("- Respeitar limites do equipamento e condições ambientais (vento, visibilidade).");
        sb.AppendLine("Referências:");
        sb.AppendLine("- NR-18.14.20 / NR-18.14.25 / NR-18.14.12–18.14.18 – Capacitação, inspeção e comunicação de operação.");
        sb.AppendLine("- NR-18.5.3 / NR-18.15.47 – Proibição de pessoas sob cargas suspensas.");
        sb.AppendLine("- NR-11.6.1 / NR-11.6.2 – Transporte seguro de cargas, estabilidade e movimentação.");
        sb.AppendLine("- NR-11.7.6 – Planejamento da movimentação de cargas.");
        sb.AppendLine();
        sb.AppendLine("5. DESCARGA DA CARGA");
        sb.AppendLine("Objetivo: Depositar carga com segurança e estabilidade.");
        sb.AppendLine("Passos:");
        sb.AppendLine("- Aproximar lentamente da posição de apoio.");
        sb.AppendLine("- Baixar carga de forma controlada.");
        sb.AppendLine("- Garantir apoio estável antes de soltar amarrações.");
        sb.AppendLine("- Afastar equipe da zona de risco.");
        sb.AppendLine("- Liberar área após conferência final.");
        sb.AppendLine("Referências:");
        sb.AppendLine("- NR-18.4.2(d) / NR-18.5.3 / NR-18.14.26 – Deposição segura e liberação da área.");
        sb.AppendLine("- NR-11.6.3 / NR-11.7.5 – Evitar tombamento, esmagamento ou movimentação insegura.");
        sb.AppendLine();
        sb.AppendLine("Endereço: Rua Justo Manfron, 2845 G, Santa Felicidade, Curitiba - PR - CEP 82.410-540 – CNPJ 81.076.242/0001-60");

        return sb.ToString();
    }

    public static string ComputeSha256(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
