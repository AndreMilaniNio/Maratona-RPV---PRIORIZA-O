/** Permissões conhecidas (strings de usuario.permissoes). O servidor é quem decide; a UI só oculta. */
export const PERMISSOES = {
  solicitacaoRegistrar: 'solicitacao.registrar',
  osConsultar: 'os.consultar',
  osStatus: 'os.status',
  osEncerrar: 'os.encerrar',
  osReabrir: 'os.reabrir',
  osReclassificar: 'os.reclassificar',
  osAtualizarFatos: 'os.atualizar-fatos',
  osTrocarMunicipio: 'os.trocar-municipio',
  osUnificar: 'os.unificar',
  despachoDesignar: 'despacho.designar',
  despachoExcecao: 'despacho.excecao',
  cidadesTodas: 'cidades.todas',
  localizacaoConsultar: 'localizacao.consultar',
  auditoriaConsultar: 'auditoria.consultar',
  pontuacaoEditar: 'pontuacao.editar',
  pontuacaoPublicar: 'pontuacao.publicar',
  pontuacaoAprovar: 'pontuacao.aprovar',
  cadastrosAdministrar: 'cadastros.administrar',
  criteriosAdministrar: 'criterios.administrar',
  prazosAdministrar: 'prazos.administrar',
  equipesAdministrar: 'equipes.administrar',
  usuariosAdministrar: 'usuarios.administrar',
} as const;

export type Permissao = (typeof PERMISSOES)[keyof typeof PERMISSOES];

/** Requisito de acesso: todas de `todas` e ao menos uma de `algumaDe`. */
export interface RequisitoPermissao {
  todas?: string[];
  algumaDe?: string[];
}

export function atende(permissoes: readonly string[] | null | undefined, req?: RequisitoPermissao): boolean {
  if (!req) return true;
  const set = new Set(permissoes ?? []);
  if (req.todas && !req.todas.every((p) => set.has(p))) return false;
  if (req.algumaDe && req.algumaDe.length > 0 && !req.algumaDe.some((p) => set.has(p))) return false;
  return true;
}
