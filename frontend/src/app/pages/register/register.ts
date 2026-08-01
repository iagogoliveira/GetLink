import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { mensagemDeErro } from '../../core/erro';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  // Os limites espelham as DataAnnotations do CreateUserDto no backend, para o
  // erro aparecer antes da requisicao.
  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    login: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
  });

  protected cadastrar(): void {
    if (this.form.invalid || this.enviando()) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    const dados = this.form.getRawValue();

    this.auth.cadastrar(dados).subscribe({
      // Cadastrou: ja entra, para o usuario nao precisar digitar tudo de novo.
      next: () =>
        this.auth.entrar({ login: dados.login, password: dados.password }).subscribe({
          next: () => this.router.navigate(['/painel']),
          error: () => this.router.navigate(['/login']),
        }),
      error: (e) => {
        this.erro.set(mensagemDeErro(e, 'Nao foi possivel criar a conta.'));
        this.enviando.set(false);
      },
    });
  }
}
