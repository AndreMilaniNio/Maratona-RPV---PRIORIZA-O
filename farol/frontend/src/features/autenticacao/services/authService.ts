import { api } from '@/services/api/client';
import type { LoginResponse, UsuarioSessaoDto } from '@/types/api';

export const authService = {
  login: (email: string, senha: string) => api.post<LoginResponse>('/api/auth/login', { email, senha }),
  operadorUnico: () => api.post<LoginResponse>('/api/auth/operador-unico'),
  me: () => api.get<UsuarioSessaoDto>('/api/me'),
  salvarPreferencias: (municipioPreferidoId: number | null) => api.put<void>('/api/me/preferencias', { municipioPreferidoId }),
};
