import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API } from './api.config';
import { UrlEstatisticas, UrlResumo } from './models';

@Injectable({ providedIn: 'root' })
export class UrlService {
  private readonly http = inject(HttpClient);

  listar(): Observable<UrlResumo[]> {
    return this.http.get<UrlResumo[]>(`${API.urls}/api/urls`);
  }

  estatisticas(id: string): Observable<UrlEstatisticas> {
    return this.http.get<UrlEstatisticas>(`${API.urls}/api/urls/${id}/stats`);
  }

  criar(originalUrl: string): Observable<{ newUrl: string }> {
    return this.http.post<{ newUrl: string }>(`${API.urls}/CreateNewUrl`, { originalUrl });
  }

  atualizar(id: string, originalUrl: string, newPath: string | null): Observable<void> {
    return this.http.put<void>(`${API.urls}/UpdateUrl`, { id, originalUrl, newPath });
  }

  // O endpoint espera o id no corpo, e nao na rota.
  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${API.urls}/DeleteUrl`, { body: { id } });
  }
}
