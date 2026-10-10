import { api } from '@/services/api/client';
import type { UnidadeConsumidoraDto } from '@/types/api';

export const ucService = {
  /** Consulta de UC no cadastro (auditada no backend). 404 = não validada; 400 = formato inválido. */
  consultar: (uc: string) => api.get<UnidadeConsumidoraDto>('/api/unidades-consumidoras', { uc }),
};
