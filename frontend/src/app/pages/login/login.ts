import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { mensagemDeErro } from '../../core/erro';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly sessaoExpirou = signal(
    this.rota.snapshot.queryParamMap.get('expirou') === '1',
  );

  protected readonly form = this.fb.nonNullable.group({
    login: ['', Validators.required],
    password: ['', Validators.required],
  });

  protected entrar(): void {
    if (this.form.invalid || this.enviando()) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);
    this.sessaoExpirou.set(false);

    this.auth.entrar(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/painel']),
      error: (e) => {
        this.erro.set(mensagemDeErro(e, 'Nao foi possivel entrar.'));
        this.enviando.set(false);
      },
    });
  }
}
