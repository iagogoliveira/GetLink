import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API } from './api.config';
import { CredenciaisLogin, NovoUsuario, RespostaLogin } from './models';

const CHAVE_TOKEN = 'encurtador.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _token = signal<string | null>(localStorage.getItem(CHAVE_TOKEN));

  readonly token = this._token.asReadonly();
  readonly autenticado = computed(() => !tokenExpirado(this._token()));
  readonly usuario = computed(() => nomeDoToken(this._token()));

  entrar(credenciais: CredenciaisLogin): Observable<RespostaLogin> {
    return this.http
      .post<RespostaLogin>(`${API.auth}/Auth/Login`, credenciais)
      .pipe(tap((resposta) => this.guardarToken(resposta.token)));
  }

  cadastrar(usuario: NovoUsuario): Observable<void> {
    return this.http.post<void>(`${API.auth}/Auth/CreateUser`, usuario);
  }

  sair(): void {
    localStorage.removeItem(CHAVE_TOKEN);
    this._token.set(null);
  }

  private guardarToken(token: string): void {
    localStorage.setItem(CHAVE_TOKEN, token);
    this._token.set(token);
  }
}

/** Le o payload do JWT sem validar assinatura: serve so para a UI. */
function payloadDoToken(token: string | null): Record<string, unknown> | null {
  if (!token) {
    return null;
  }

  try {
    const corpo = token.split('.')[1];
    if (!corpo) {
      return null;
    }

    const base64 = corpo.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return null;
  }
}

/**
 * Token vencido conta como deslogado. Sem isto o usuario veria as telas
 * internas e so descobriria o problema quando cada requisicao voltasse 401.
 */
function tokenExpirado(token: string | null): boolean {
  const payload = payloadDoToken(token);

  if (!payload) {
    return true;
  }

  const exp = payload['exp'];

  if (typeof exp !== 'number') {
    return true;
  }

  return exp * 1000 <= Date.now();
}

function nomeDoToken(token: string | null): string | null {
  const payload = payloadDoToken(token);
  const email = payload?.['email'];

  return typeof email === 'string' ? email : null;
}
