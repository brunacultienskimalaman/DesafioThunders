using Microsoft.EntityFrameworkCore;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Dtos.Praca;
using Thunders.TechTest.ApiService.Models;

namespace Thunders.TechTest.ApiService.Services
{
    public class PracaService : IPracaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PracaService> _logger;

        public PracaService(AppDbContext context, ILogger<PracaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PracaDto>> ObterUltimasPracasPorIdAsync()
        {
            try
            {
                var ultimasPracas = await _context.Pracas
                    .OrderByDescending(p => p.Id)
                    .Take(10)
                    .Select(p => new PracaDto
                    {
                        Nome = p.Nome,
                        Cidade = p.Cidade,
                        Estado = p.Estado,
                        Ativa = p.Ativa
                    })
                    .ToListAsync();

                return ultimasPracas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter últimas praças");
                return new List<PracaDto>();
            }
        }

        public async Task<PracaResultado> ProcessarPracaAsync(PracaDto praca)
        {
            try
            {
                var entity = new Praca
                {
                    Nome = praca.Nome,
                    Cidade = praca.Cidade.Trim().ToUpper(),
                    Estado = praca.Estado.Trim().ToUpper(),
                    Ativa = praca.Ativa

                };

                _context.Pracas.Add(entity);
                await _context.SaveChangesAsync();

                return new PracaResultado
                {
                    Id = Guid.NewGuid(),
                    DataProcessamento = DateTime.Now,
                    Sucesso = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar utilização única");
                return new PracaResultado
                {
                    Id = Guid.NewGuid(),
                    DataProcessamento = DateTime.Now,
                    Sucesso = false,
                    MensagemErro = ex.Message
                };
            }
        }

    }
}
