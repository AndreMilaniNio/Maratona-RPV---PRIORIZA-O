using Farol.Domain.Entities;
using Farol.Domain.Enums;

namespace Farol.Application.Prioritization;

public sealed record PrazosCalculados(
    DateTimeOffset Triagem,
    DateTimeOffset Despacho,
    DateTimeOffset Inicio,
    DateTimeOffset? Restabelecimento,
    DateTimeOffset Conclusao);

/// <summary>
/// Calcula os cinco prazos (seção 8) a partir da abertura. Calendário corrido soma minutos;
/// calendário útil conta só o expediente (seg–sex, 08h–18h no fuso operacional), sem feriados.
/// </summary>
public static class CalculadoraPrazos
{
    public static readonly TimeOnly InicioExpediente = new(8, 0);
    public static readonly TimeOnly FimExpediente = new(18, 0);

    public static PrazosCalculados Calcular(
        Prioridade prioridade, DateTimeOffset abertaEm, TimeZoneInfo fuso, IReadOnlySet<DateOnly> feriados)
    {
        DateTimeOffset Somar(int minutos) => prioridade.Calendario == CalendarioPrazo.Corrido
            ? abertaEm.AddMinutes(minutos)
            : SomarMinutosUteis(abertaEm, minutos, fuso, prioridade.ConsideraFeriados ? feriados : new HashSet<DateOnly>());

        return new PrazosCalculados(
            Somar(prioridade.PrazoTriagemMin),
            Somar(prioridade.PrazoDespachoMin),
            Somar(prioridade.PrazoInicioMin),
            prioridade.PrazoRestabelecimentoMin is { } r ? Somar(r) : null,
            Somar(prioridade.PrazoConclusaoMin));
    }

    public static DateTimeOffset SomarMinutosUteis(DateTimeOffset inicio, int minutos, TimeZoneInfo fuso, IReadOnlySet<DateOnly> feriados)
    {
        var local = TimeZoneInfo.ConvertTime(inicio, fuso).DateTime;
        var restante = TimeSpan.FromMinutes(minutos);

        local = ProximoMomentoUtil(local, feriados);
        while (restante > TimeSpan.Zero)
        {
            var fimDoDia = local.Date + FimExpediente.ToTimeSpan();
            var disponivel = fimDoDia - local;
            if (restante <= disponivel)
            {
                local += restante;
                restante = TimeSpan.Zero;
            }
            else
            {
                restante -= disponivel;
                local = ProximoMomentoUtil(local.Date.AddDays(1) + InicioExpediente.ToTimeSpan(), feriados);
            }
        }

        var offset = fuso.GetUtcOffset(local);
        return new DateTimeOffset(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), offset).ToUniversalTime();
    }

    private static DateTime ProximoMomentoUtil(DateTime local, IReadOnlySet<DateOnly> feriados)
    {
        while (true)
        {
            var dia = DateOnly.FromDateTime(local);
            var horario = TimeOnly.FromDateTime(local);
            var diaUtil = local.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !feriados.Contains(dia);
            if (diaUtil && horario < InicioExpediente) return local.Date + InicioExpediente.ToTimeSpan();
            if (diaUtil && horario < FimExpediente) return local;
            local = local.Date.AddDays(1) + InicioExpediente.ToTimeSpan();
        }
    }

    public static DateTimeOffset? PrazoDo(OrdemServico os, TipoPrazo tipo) => tipo switch
    {
        TipoPrazo.Triagem => os.PrazoTriagem,
        TipoPrazo.Despacho => os.PrazoDespacho,
        TipoPrazo.Inicio => os.PrazoInicio,
        TipoPrazo.Restabelecimento => os.PrazoRestabelecimento,
        TipoPrazo.Conclusao => os.PrazoConclusao,
        _ => null,
    };
}
