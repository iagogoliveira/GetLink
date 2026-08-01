import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.token();

  const requisicao = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(requisicao).pipe(
    catchError((erro: unknown) => {
      // Token recusado pelo servidor: derruba a sessao local em vez de deixar
      // a tela num estado logado que nao funciona.
      if (erro instanceof HttpErrorResponse && erro.status === 401 && token) {
        auth.sair();
        router.navigate(['/login'], { queryParams: { expirou: '1' } });
      }

      return throwError(() => erro);
    }),
  );
};
