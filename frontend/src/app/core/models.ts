// Espelham os DTOs do backend. O ASP.NET serializa em camelCase por padrao.

export interface CredenciaisLogin {
  login: string;
  password: string;
}

export interface NovoUsuario {
  name: string;
  login: string;
  password: string;
  email: string;
}

export interface RespostaLogin {
  token: string;
}

export interface UrlResumo {
  id: string;
  originalUrl: string;
  shortUrl: string;
  createdAt: string;
  totalClicks: number;
  lastClickedAt: string | null;
}

export interface ContagemPorChave {
  chave: string;
  total: number;
}

export interface ClicksPorDia {
  /** Serializado pelo DateOnly do .NET como "2026-08-01". */
  dia: string;
  total: number;
}

export interface UrlEstatisticas {
  id: string;
  originalUrl: string;
  shortUrl: string;
  createdAt: string;
  totalClicks: number;
  lastClickedAt: string | null;
  clicksPorDia: ClicksPorDia[];
  porNavegador: ContagemPorChave[];
  porDispositivo: ContagemPorChave[];
  porSistema: ContagemPorChave[];
  porOrigem: ContagemPorChave[];
}
