import { API_URL } from '@/app/config/env';
import type { ProblemDetails } from '@/types/api';

const TOKEN_KEY = 'farol.token';

/** Armazenamento do JWT (sessionStorage: some ao fechar a aba). Não guarda dados de negócio. */
export const tokenStorage = {
  get(): string | null {
    try {
      return sessionStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  },
  set(token: string) {
    try {
      sessionStorage.setItem(TOKEN_KEY, token);
    } catch {
      /* armazenamento indisponível: sessão vale só em memória */
    }
  },
  clear() {
    try {
      sessionStorage.removeItem(TOKEN_KEY);
    } catch {
      /* ignore */
    }
  },
};

/** Erro de chamada à API, com o ProblemDetails (RFC 7807) quando houver. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null, fallback?: string) {
    super(problem?.title ?? problem?.detail ?? fallback ?? `Erro ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  /** Falha de rede / API fora do ar. */
  get isNetwork() {
    return this.status === 0;
  }

  /** Erros por campo (400). Chaves normalizadas em camelCase, sem prefixo "$.". */
  get fieldErrors(): Record<string, string[]> {
    const out: Record<string, string[]> = {};
    for (const [k, v] of Object.entries(this.problem?.errors ?? {})) {
      const key = k
        .replace(/^\$\.?/, '')
        .split('.')
        .map((p) => (p ? p[0].toLowerCase() + p.slice(1) : p))
        .join('.');
      out[key] = Array.isArray(v) ? v : [String(v)];
    }
    return out;
  }

  /** Pendências (422). */
  get pendencias(): string[] {
    const p = this.problem?.pendencias;
    return Array.isArray(p) ? p.map(String) : [];
  }
}

let onUnauthorized: (() => void) | null = null;
/** Registrado pelo AuthProvider: 401 encerra a sessão e volta ao Login. */
export function setUnauthorizedHandler(fn: (() => void) | null) {
  onUnauthorized = fn;
}

export type QueryValue = string | number | boolean | null | undefined | readonly (string | number | boolean)[];
export type QueryParams = Record<string, QueryValue>;

/** Monta a query string; listas viram parâmetros repetidos; vazios são omitidos. */
export function buildQuery(params?: QueryParams): string {
  if (!params) return '';
  const sp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    if (Array.isArray(value)) {
      for (const v of value) sp.append(key, String(v));
    } else {
      sp.append(key, String(value));
    }
  }
  const s = sp.toString();
  return s ? `?${s}` : '';
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  query?: QueryParams;
  body?: unknown;
  signal?: AbortSignal;
  /** Retorna a Response crua (download). */
  raw?: boolean;
}

async function parseProblem(res: Response): Promise<ProblemDetails | null> {
  const text = await res.text().catch(() => '');
  if (!text) return null;
  try {
    const parsed: unknown = JSON.parse(text);
    if (parsed && typeof parsed === 'object') return parsed as ProblemDetails;
    return { title: String(parsed) };
  } catch {
    return { title: text.slice(0, 300) };
  }
}

export async function request<T>(path: string, opts: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' };
  const token = tokenStorage.get();
  if (token) headers.Authorization = `Bearer ${token}`;

  let body: BodyInit | undefined;
  if (opts.body instanceof FormData) {
    body = opts.body;
  } else if (opts.body !== undefined) {
    headers['Content-Type'] = 'application/json';
    body = JSON.stringify(opts.body);
  }

  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}${buildQuery(opts.query)}`, {
      method: opts.method ?? 'GET',
      headers,
      body,
      signal: opts.signal,
    });
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError') throw err;
    throw new ApiError(0, null, 'Não foi possível conectar à API. Verifique a conexão e tente novamente.');
  }

  if (res.status === 401) {
    const problem = await parseProblem(res);
    // Login com credenciais erradas também devolve 401: não derruba a sessão nesse caso.
    if (!path.startsWith('/api/auth/login')) onUnauthorized?.();
    throw new ApiError(401, problem, 'Sessão expirada. Entre novamente.');
  }
  if (!res.ok) {
    const problem = await parseProblem(res);
    const fallback =
      res.status === 403
        ? 'Você não tem permissão para esta operação.'
        : res.status === 404
          ? 'Registro não encontrado.'
          : res.status >= 500
            ? 'Erro interno da API. Tente novamente em instantes.'
            : undefined;
    throw new ApiError(res.status, problem, fallback);
  }
  if (opts.raw) return res as unknown as T;
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  if (!text) return undefined as T;
  try {
    return JSON.parse(text) as T;
  } catch {
    return text as unknown as T;
  }
}

export const api = {
  get: <T>(path: string, query?: QueryParams, signal?: AbortSignal) => request<T>(path, { query, signal }),
  post: <T>(path: string, body?: unknown, query?: QueryParams) => request<T>(path, { method: 'POST', body, query }),
  put: <T>(path: string, body?: unknown, query?: QueryParams) => request<T>(path, { method: 'PUT', body, query }),
  patch: <T>(path: string, body?: unknown) => request<T>(path, { method: 'PATCH', body }),
  delete: <T>(path: string, query?: QueryParams) => request<T>(path, { method: 'DELETE', query }),
  upload: <T>(path: string, form: FormData) => request<T>(path, { method: 'POST', body: form }),
  /** Baixa um arquivo autenticado e dispara o download no navegador. */
  async download(path: string, nomeArquivo: string) {
    const res = await request<Response>(path, { raw: true });
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = nomeArquivo;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  },
};

/** Mensagem legível de qualquer erro. */
export function mensagemErro(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof Error) return err.message;
  return 'Erro inesperado.';
}

/** Política de retentativa para o TanStack Query: só falhas de rede / 5xx. */
export function deveRetentar(failureCount: number, err: unknown): boolean {
  if (err instanceof ApiError) {
    if (err.status === 0 || err.status >= 500) return failureCount < 2;
    return false;
  }
  return failureCount < 1;
}
