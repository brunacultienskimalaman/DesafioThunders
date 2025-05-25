using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Dtos;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Messages;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Models.Enums;

namespace Thunders.TechTest.ApiService.Services;

public class UtilizacaoService : IUtilizacaoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UtilizacaoService> _logger;

    public UtilizacaoService(AppDbContext context,  ILogger<UtilizacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UtilizacaoResultado> ProcessarUtilizacaoAsync(UtilizacaoDto utilizacao)
    {
        try
        {
            var pracaExiste = await _context.Pracas
                .AnyAsync(p => p.Id == utilizacao.PracaId && p.Ativa);

            if (!pracaExiste)
            {
                throw new ArgumentException($"Praça com ID {utilizacao.PracaId} não encontrada ou inativa");
            }

            // Fazendo processamento padrão, rebu esta dando problemas, revisar !!!!
            var entity = new UtilizacaoPedagio
            {
                DataUtilizacao = utilizacao.DataUtilizacao,
                PracaId = utilizacao.PracaId,
                Cidade = utilizacao.Cidade.Trim().ToUpper(),
                Estado = utilizacao.Estado.Trim().ToUpper(),
                ValorPago = utilizacao.ValorPago,
                TipoVeiculo = utilizacao.TipoVeiculo,
                DataInsercao = DateTime.Now
            };

            _context.Utilizacoes.Add(entity);
            await _context.SaveChangesAsync();

            return new UtilizacaoResultado
            {
                Id = Guid.NewGuid(),
                DataProcessamento = DateTime.Now,
                Sucesso = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar utilização única");
            return new UtilizacaoResultado
            {
                Id = Guid.NewGuid(),
                DataProcessamento = DateTime.Now,
                Sucesso = false,
                MensagemErro = ex.Message
            };
        }
    }
    public async Task<LoteResultado> ProcessarUtilizacoesLoteAsync(UtilizacaoLoteDto lote)
    {
        var resultado = new LoteResultado
        {
            LoteId = Guid.NewGuid(),
            DataProcessamento = DateTime.Now
        };

        try
        {
            var pracaIds = lote.Utilizacoes.Select(u => u.PracaId).Distinct().ToList();
            var pracasValidas = await _context.Pracas
                .Where(p => pracaIds.Contains(p.Id) && p.Ativa)
                .Select(p => p.Id)
                .ToListAsync();

            var utilizacoesValidas = new List<UtilizacaoPedagio>();
            var erros = new List<string>();

            foreach (var utilizacao in lote.Utilizacoes)
            {
                if (!pracasValidas.Contains(utilizacao.PracaId))
                {
                    erros.Add($"Praça ID {utilizacao.PracaId} não encontrada ou inativa");
                    resultado.TotalComErro++;
                }
                else
                {
                    var entity = new UtilizacaoPedagio
                    {
                        DataUtilizacao = utilizacao.DataUtilizacao,
                        PracaId = utilizacao.PracaId,
                        Cidade = utilizacao.Cidade.Trim().ToUpper(),
                        Estado = utilizacao.Estado.Trim().ToUpper(),
                        ValorPago = utilizacao.ValorPago,
                        TipoVeiculo = utilizacao.TipoVeiculo,
                        DataInsercao = DateTime.Now
                    };

                    utilizacoesValidas.Add(entity);
                }
            }

            if (utilizacoesValidas.Any())
            {
                _context.Utilizacoes.AddRange(utilizacoesValidas);
                await _context.SaveChangesAsync();
                resultado.TotalProcessadas = utilizacoesValidas.Count;
            }

            resultado.Erros = erros;

            _logger.LogInformation("Lote processado diretamente: {Total} válidas, {Erros} com erro",
                resultado.TotalProcessadas, resultado.TotalComErro);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar lote de utilizações");
            resultado.Erros.Add($"Erro interno: {ex.Message}");
            return resultado;
        }
    }

    public async Task<EstatisticasDto> ObterEstatisticasAsync()
    {
        var hoje = DateTime.Today;
        var ontem = hoje.AddDays(-1);

        var stats = new EstatisticasDto
        {
            UltimaAtualizacao = DateTime.Now
        };

        var utilizacoesHoje = await _context.Utilizacoes
            .Where(u => u.DataUtilizacao >= hoje)
            .ToListAsync();

        stats.TotalUtilizacoesHoje = utilizacoesHoje.Count;
        stats.MediaValorHoje = utilizacoesHoje.Count > 0 ? utilizacoesHoje.Average(u => u.ValorPago) : 0;

        stats.TotalUtilizacoesOntem = await _context.Utilizacoes
            .Where(u => u.DataUtilizacao >= ontem && u.DataUtilizacao < hoje)
            .CountAsync();

        stats.UtilizacoesPorTipoVeiculo = await _context.Utilizacoes
            .Where(u => u.DataUtilizacao >= hoje)
            .GroupBy(u => u.TipoVeiculo)
            .ToDictionaryAsync(
                g => ((TipoVeiculo)g.Key).ToString(),
                g => g.Count()
            );

        stats.UtilizacoesPorEstado = await _context.Utilizacoes
            .Where(u => u.DataUtilizacao >= hoje)
            .GroupBy(u => u.Estado)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return stats;
    }

    public async Task<List<UtilizacaoDto>> ObterUltimasUtilizacoesDtoAsync()
    {
        try
        {
            var ultimasUtilizacoes = await _context.Utilizacoes
                .OrderByDescending(u => u.DataInsercao)
                .Take(10)
                .Select(u => new UtilizacaoDto
                {
                    DataUtilizacao = u.DataUtilizacao,
                    PracaId = u.PracaId,
                    Cidade = u.Cidade,
                    Estado = u.Estado,
                    ValorPago = u.ValorPago,
                    TipoVeiculo = u.TipoVeiculo
                })
                .ToListAsync();

            return ultimasUtilizacoes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter últimas utilizações");
            return new List<UtilizacaoDto>();
        }
    }
}