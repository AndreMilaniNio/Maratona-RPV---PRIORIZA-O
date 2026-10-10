/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string;
  readonly VITE_MODO_OPERADOR_UNICO?: string;
}
interface ImportMeta {
  readonly env: ImportMetaEnv;
}
