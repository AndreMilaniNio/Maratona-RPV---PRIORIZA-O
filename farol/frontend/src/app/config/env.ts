/** Configuração de ambiente do frontend. */
export const API_URL: string = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080').replace(/\/+$/, '');

/** Fuso horário operacional usado para exibir datas (a API trabalha em UTC). */
export const FUSO_OPERACIONAL = 'America/Sao_Paulo';

/** Intervalo de atualização automática da fila (seção 10). */
export const INTERVALO_ATUALIZACAO_MS = 60_000;

/** Servidor de tiles OpenStreetMap. */
export const TILE_URL = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
export const TILE_ATTRIBUTION =
  '&copy; <a href="https://www.openstreetmap.org/copyright">colaboradores do OpenStreetMap</a>';
