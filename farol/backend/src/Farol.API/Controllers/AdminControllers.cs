using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Services;
using Farol.Domain.Entities;
using Farol.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farol.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController(AdminService admin, EquipeService equipes) : ControllerBase
{
    // Municípios e rede
    [HttpPost("municipios")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<MunicipioDto> CriarMunicipio(MunicipioSalvarRequest r, CancellationToken ct) => admin.SalvarMunicipioAsync(null, r, ct);
    [HttpPut("municipios/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<MunicipioDto> AlterarMunicipio(int id, MunicipioSalvarRequest r, CancellationToken ct) => admin.SalvarMunicipioAsync(id, r, ct);
    [HttpDelete("municipios/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public async Task<IActionResult> ExcluirMunicipio(int id, CancellationToken ct) { await admin.ExcluirMunicipioAsync(id, ct); return NoContent(); }

    [HttpPost("localidades")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> CriarLocalidade(LocalidadeSalvarRequest r, CancellationToken ct) => admin.SalvarLocalidadeAsync(null, r, ct);
    [HttpPut("localidades/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> AlterarLocalidade(int id, LocalidadeSalvarRequest r, CancellationToken ct) => admin.SalvarLocalidadeAsync(id, r, ct);

    [HttpPost("subestacoes")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> CriarSubestacao(SubestacaoSalvarRequest r, CancellationToken ct) => admin.SalvarSubestacaoAsync(null, r, ct);
    [HttpPut("subestacoes/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> AlterarSubestacao(int id, SubestacaoSalvarRequest r, CancellationToken ct) => admin.SalvarSubestacaoAsync(id, r, ct);

    [HttpPost("conjuntos")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> CriarConjunto(ConjuntoSalvarRequest r, CancellationToken ct) => admin.SalvarConjuntoAsync(null, r, ct);
    [HttpPut("conjuntos/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> AlterarConjunto(int id, ConjuntoSalvarRequest r, CancellationToken ct) => admin.SalvarConjuntoAsync(id, r, ct);

    [HttpPost("transformadores")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> CriarTransformador(TransformadorSalvarRequest r, CancellationToken ct) => admin.SalvarTransformadorAsync(null, r, ct);
    [HttpPut("transformadores/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> AlterarTransformador(int id, TransformadorSalvarRequest r, CancellationToken ct) => admin.SalvarTransformadorAsync(id, r, ct);

    [HttpPost("classes-cliente")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<ClasseClienteDto> CriarClasse(ClasseSalvarRequest r, CancellationToken ct) => admin.SalvarClasseAsync(null, r, ct);
    [HttpPut("classes-cliente/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<ClasseClienteDto> AlterarClasse(int id, ClasseSalvarRequest r, CancellationToken ct) => admin.SalvarClasseAsync(id, r, ct);

    [HttpPost("tipos-ocorrencia")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> CriarTipo(TipoOcorrenciaSalvarRequest r, CancellationToken ct) => admin.SalvarTipoOcorrenciaAsync(null, r, ct);
    [HttpPut("tipos-ocorrencia/{id:int}")] [Authorize(Policy = Permissoes.CadastrosAdministrar)]
    public Task<int> AlterarTipo(int id, TipoOcorrenciaSalvarRequest r, CancellationToken ct) => admin.SalvarTipoOcorrenciaAsync(id, r, ct);

    [HttpPost("qualificacoes")] [Authorize(Policy = Permissoes.EquipesAdministrar)]
    public Task<ItemCodigoDto> CriarQualificacao(ItemSalvarRequest r, CancellationToken ct) => admin.SalvarQualificacaoAsync(null, r, ct);
    [HttpPost("recursos")] [Authorize(Policy = Permissoes.EquipesAdministrar)]
    public Task<ItemCodigoDto> CriarRecurso(ItemSalvarRequest r, CancellationToken ct) => admin.SalvarRecursoAsync(null, r, ct);

    // Equipes
    [HttpPost("equipes")] [Authorize(Policy = Permissoes.EquipesAdministrar)]
    public Task<EquipeDto> CriarEquipe(EquipeSalvarRequest r, CancellationToken ct) => equipes.SalvarAsync(null, r, ct);
    [HttpPut("equipes/{id:int}")] [Authorize(Policy = Permissoes.EquipesAdministrar)]
    public Task<EquipeDto> AlterarEquipe(int id, EquipeSalvarRequest r, CancellationToken ct) => equipes.SalvarAsync(id, r, ct);

    // Códigos de prioridade e prazos
    [HttpPost("prioridades")] [Authorize(Policy = Permissoes.PrazosAdministrar)]
    public Task<PrioridadeDto> CriarPrioridade(PrioridadeSalvarRequest r, CancellationToken ct) => admin.SalvarPrioridadeAsync(null, r, ct);
    [HttpPut("prioridades/{id:int}")] [Authorize(Policy = Permissoes.PrazosAdministrar)]
    public Task<PrioridadeDto> AlterarPrioridade(int id, PrioridadeSalvarRequest r, CancellationToken ct) => admin.SalvarPrioridadeAsync(id, r, ct);

    [HttpGet("feriados")] [Authorize(Policy = Permissoes.PrazosAdministrar)]
    public Task<List<FeriadoDto>> Feriados(CancellationToken ct) => admin.FeriadosAsync(ct);
    [HttpPost("feriados")] [Authorize(Policy = Permissoes.PrazosAdministrar)]
    public Task<FeriadoDto> CriarFeriado(FeriadoSalvarRequest r, CancellationToken ct) => admin.SalvarFeriadoAsync(r, ct);
    [HttpDelete("feriados/{id:int}")] [Authorize(Policy = Permissoes.PrazosAdministrar)]
    public async Task<IActionResult> ExcluirFeriado(int id, CancellationToken ct) { await admin.ExcluirFeriadoAsync(id, ct); return NoContent(); }

    // Camada A e estrutura de critérios
    [HttpPost("regras-precedencia")] [Authorize(Policy = Permissoes.CriteriosAdministrar)]
    public Task<RegraPrecedenciaDto> CriarRegra(RegraSalvarRequest r, CancellationToken ct) => admin.SalvarRegraAsync(null, r, ct);
    [HttpPut("regras-precedencia/{id:int}")] [Authorize(Policy = Permissoes.CriteriosAdministrar)]
    public Task<RegraPrecedenciaDto> AlterarRegra(int id, RegraSalvarRequest r, CancellationToken ct) => admin.SalvarRegraAsync(id, r, ct);

    [HttpPost("criterios")] [Authorize(Policy = Permissoes.CriteriosAdministrar)]
    public Task<CriterioAdminDto> CriarCriterio(CriterioSalvarRequest r, CancellationToken ct) => admin.SalvarCriterioAsync(null, r, ct);
    [HttpPut("criterios/{id:int}")] [Authorize(Policy = Permissoes.CriteriosAdministrar)]
    public Task<CriterioAdminDto> AlterarCriterio(int id, CriterioSalvarRequest r, CancellationToken ct) => admin.SalvarCriterioAsync(id, r, ct);

    [HttpGet("auditoria")] [Authorize(Policy = Permissoes.AuditoriaConsultar)]
    public Task<PaginaDto<AuditoriaDto>> Auditoria([FromQuery] FiltroAuditoria filtro, CancellationToken ct) => admin.AuditoriaAsync(filtro, ct);
}

/// <summary>Usuários, perfis e vínculos com municípios.</summary>
[ApiController]
[Route("api/admin/usuarios")]
[Authorize(Policy = Permissoes.UsuariosAdministrar)]
public class UsuariosController(UserManager<Usuario> usuarios, IAppDbContext db, Auditor auditor) : ControllerBase
{
    [HttpGet]
    public async Task<List<UsuarioDto>> Listar(CancellationToken ct)
    {
        var lista = await db.Users.AsNoTracking().Include(u => u.Municipios).OrderBy(u => u.Nome).ToListAsync(ct);
        var resultado = new List<UsuarioDto>();
        foreach (var u in lista)
            resultado.Add(new UsuarioDto(u.Id, u.Nome, u.Email ?? "", u.Ativo, (await usuarios.GetRolesAsync(u)).ToList(),
                u.Municipios.Select(m => m.MunicipioId).ToList(), u.EquipeId, u.Demonstrativo));
        return resultado;
    }

    [HttpPost]
    public Task<UsuarioDto> Criar(UsuarioSalvarRequest r, CancellationToken ct) => SalvarAsync(null, r, ct);

    [HttpPut("{id:guid}")]
    public Task<UsuarioDto> Alterar(Guid id, UsuarioSalvarRequest r, CancellationToken ct) => SalvarAsync(id, r, ct);

    private async Task<UsuarioDto> SalvarAsync(Guid? id, UsuarioSalvarRequest r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Nome) || string.IsNullOrWhiteSpace(r.Email)) throw new RegraNegocioException("Informe nome e e-mail.");
        if (r.Perfis.Count == 0 || r.Perfis.Any(p => !Perfis.Todos.Contains(p))) throw new RegraNegocioException("Perfis inválidos.");
        if (r.Perfis.Contains(Perfis.EquipeCampo) && r.EquipeId is null) throw new RegraNegocioException("Usuário de equipe de campo precisa de uma equipe.");

        Usuario u;
        if (id is { } existente)
        {
            u = await db.Users.Include(x => x.Municipios).FirstOrDefaultAsync(x => x.Id == existente, ct) ?? throw new NaoEncontradoException("Usuário não encontrado.");
            u.Nome = r.Nome.Trim(); u.Email = r.Email.Trim(); u.UserName = r.Email.Trim(); u.Ativo = r.Ativo; u.EquipeId = r.EquipeId;
            u.Municipios.Clear();
            u.Municipios.AddRange(r.MunicipiosIds.Distinct().Select(m => new UsuarioMunicipio { UsuarioId = u.Id, MunicipioId = m }));
            Verificar(await usuarios.UpdateAsync(u));
            if (!string.IsNullOrWhiteSpace(r.Senha))
            {
                Verificar(await usuarios.RemovePasswordAsync(u));
                Verificar(await usuarios.AddPasswordAsync(u, r.Senha));
            }
            var atuais = await usuarios.GetRolesAsync(u);
            Verificar(await usuarios.RemoveFromRolesAsync(u, atuais.Except(r.Perfis)));
            Verificar(await usuarios.AddToRolesAsync(u, r.Perfis.Except(atuais)));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(r.Senha)) throw new RegraNegocioException("Informe a senha inicial.");
            u = new Usuario
            {
                Nome = r.Nome.Trim(), Email = r.Email.Trim(), UserName = r.Email.Trim(), EmailConfirmed = true, Ativo = r.Ativo, EquipeId = r.EquipeId,
                Municipios = r.MunicipiosIds.Distinct().Select(m => new UsuarioMunicipio { MunicipioId = m }).ToList(),
            };
            Verificar(await usuarios.CreateAsync(u, r.Senha));
            Verificar(await usuarios.AddToRolesAsync(u, r.Perfis));
        }
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Usuario", u.Id,
            novos: new { u.Nome, u.Email, r.Perfis, r.MunicipiosIds, r.EquipeId, r.Ativo, senhaAlterada = !string.IsNullOrWhiteSpace(r.Senha) });
        await db.SaveChangesAsync(ct);
        return new UsuarioDto(u.Id, u.Nome, u.Email!, u.Ativo, r.Perfis, r.MunicipiosIds, u.EquipeId, u.Demonstrativo);
    }

    private static void Verificar(IdentityResult r)
    {
        if (!r.Succeeded) throw new RegraNegocioException(string.Join(" ", r.Errors.Select(e => e.Description)));
    }
}
