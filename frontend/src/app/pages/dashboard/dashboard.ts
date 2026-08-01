import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { mensagemDeErro } from '../../core/erro';
import { UrlResumo } from '../../core/models';
import { UrlService } from '../../core/url.service';

@Component({
  selector: 'app-dashboard',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly urls = inject(UrlService);

  protected readonly lista = signal<UrlResumo[]>([]);
  protected readonly carregando = signal(true);
  protected readonly criando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly erroLista = signal<string | null>(null);
  protected readonly copiada = signal<string | null>(null);
  protected readonly excluindo = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    originalUrl: ['', [Validators.required, Validators.maxLength(2048)]],
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erroLista.set(null);

    this.urls.listar().subscribe({
      next: (dados) => {
        this.lista.set(dados);
        this.carregando.set(false);
      },
      error: (e) => {
        this.erroLista.set(mensagemDeErro(e, 'Nao foi possivel carregar suas URLs.'));
        this.carregando.set(false);
      },
    });
  }

  protected criar(): void {
    if (this.form.invalid || this.criando()) {
      this.form.markAllAsTouched();
      return;
    }

    this.criando.set(true);
    this.erro.set(null);

    this.urls.criar(this.form.getRawValue().originalUrl).subscribe({
      next: () => {
        this.form.reset();
        this.criando.set(false);
        this.carregar();
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e, 'Nao foi possivel encurtar essa URL.'));
        this.criando.set(false);
      },
    });
  }

  protected excluir(url: UrlResumo): void {
    if (!confirm(`Excluir ${url.shortUrl}? O historico de cliques vai junto.`)) {
      return;
    }

    this.excluindo.set(url.id);

    this.urls.excluir(url.id).subscribe({
      next: () => {
        this.excluindo.set(null);
        this.carregar();
      },
      error: (e) => {
        this.erroLista.set(mensagemDeErro(e, 'Nao foi possivel excluir.'));
        this.excluindo.set(null);
      },
    });
  }

  protected async copiar(url: UrlResumo): Promise<void> {
    try {
      await navigator.clipboard.writeText(url.shortUrl);
      this.copiada.set(url.id);
      setTimeout(() => this.copiada.set(null), 2000);
    } catch {
      // clipboard exige contexto seguro; em http o navegador pode recusar.
      this.erroLista.set('O navegador bloqueou a copia. Selecione o link manualmente.');
    }
  }
}