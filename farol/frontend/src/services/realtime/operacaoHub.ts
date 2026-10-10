import { HubConnectionBuilder, HubConnectionState, LogLevel, type HubConnection } from '@microsoft/signalr';
import { API_URL } from '@/app/config/env';
import { tokenStorage } from '@/services/api/client';

export type EstadoTempoReal = 'desconectado' | 'conectando' | 'conectado' | 'reconectando';

export interface FilaAlteradaEvento {
  municipioId?: number | null;
  [k: string]: unknown;
}

/**
 * Conexão com o hub /hubs/operacao. O evento `filaAlterada` só invalida consultas;
 * a atualização periódica (60 s) continua como reconciliação.
 */
export class OperacaoHub {
  private conn: HubConnection;
  private municipioAtual: number | null = null;
  private iniciando: Promise<void> | null = null;

  constructor(
    private readonly onFilaAlterada: (e: FilaAlteradaEvento) => void,
    private readonly onEstado: (e: EstadoTempoReal) => void,
  ) {
    this.conn = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/operacao`, { accessTokenFactory: () => tokenStorage.get() ?? '' })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();
    this.conn.on('filaAlterada', (e: FilaAlteradaEvento) => this.onFilaAlterada(e ?? {}));
    this.conn.onreconnecting(() => this.onEstado('reconectando'));
    this.conn.onreconnected(() => {
      this.onEstado('conectado');
      void this.acompanhar(this.municipioAtual);
    });
    this.conn.onclose(() => this.onEstado('desconectado'));
  }

  async iniciar(): Promise<void> {
    if (this.conn.state !== HubConnectionState.Disconnected) return this.iniciando ?? Promise.resolve();
    this.onEstado('conectando');
    this.iniciando = this.conn
      .start()
      .then(() => {
        this.onEstado('conectado');
        return this.acompanhar(this.municipioAtual);
      })
      .catch(() => {
        this.onEstado('desconectado');
      })
      .finally(() => {
        this.iniciando = null;
      });
    return this.iniciando;
  }

  async acompanhar(municipioId: number | null): Promise<void> {
    this.municipioAtual = municipioId;
    if (this.conn.state !== HubConnectionState.Connected) return;
    try {
      await this.conn.invoke('Acompanhar', municipioId);
    } catch {
      /* o polling de 60 s cobre a falha */
    }
  }

  async parar(): Promise<void> {
    // Parar durante a negociação gera erro no console (StrictMode monta/desmonta em dev):
    // espera a tentativa de conexão terminar antes de encerrar.
    if (this.iniciando) await this.iniciando.catch(() => {});
    try {
      await this.conn.stop();
    } catch {
      /* ignore */
    }
  }
}
