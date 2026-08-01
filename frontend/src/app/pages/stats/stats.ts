import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { mensagemDeErro } from '../../core/erro';
import { UrlEstatisticas } from '../../core/models';
import { UrlService } from '../../core/url.service';

@Component({
  selector: 'app-stats',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './stats.html',
})
export class Stats implements OnInit {
  private readonly rota = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly urls = inject(UrlService);
  private readonly fb = inject(FormBuilder);

  private readonly id = this.rota.snapshot.paramMap.get('id')!;

  protected readonly dados = signal<UrlEstatisticas | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly editando = signal(false);
  protected readonly salvando = signal(false);
  protected readonly erroEdicao = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    originalUrl: ['', [Validators.required, Validators.maxLength(2048)]],
    newPath: ['', Validators.maxLength(100)],
  });

  /**
   * Altura relativa de cada barra. Normalizar pelo maior dia mantem o grafico
   * legivel tanto com 3 cliques quanto com 3 mil.
   */
  protected readonly barras = computed(() => {
    const dias = this.dados()?.clicksPorDia ?? [];
    const maior = Math.max(...dias.map((d) => d.total), 1);

    return dias.map((d) => ({
      dia: d.dia,
      total: d.total,
      altura: Math.round((d.total / maior) * 100),
    }));
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.urls.estatisticas(this.id).subscribe({
      next: (dados) => {
        this.dados.set(dados);
        this.form.patchValue({ originalUrl: dados.originalUrl, newPath: '' });
        this.carregando.set(false);
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e, 'Nao foi possivel carregar as estatisticas.'));
        this.carregando.set(false);
      },
    });
  }

  protected alternarEdicao(): void {
    this.editando.update((v) => !v);
    this.erroEdicao.set(null);
  }

  protected salvar(): void {
    if (this.form.invalid || this.salvando()) {
      this.form.markAllAsTouched();
      return;
    }

    this.salvando.set(true);
    this.erroEdicao.set(null);

    const { originalUrl, newPath } = this.form.getRawValue();

    // Caminho vazio significa "manter o atual"; o backend so troca quando vem preenchido.
    this.urls.atualizar(this.id, originalUrl, newPath.trim() || null).subscribe({
      next: () => {
        this.salvando.set(false);
        this.editando.set(false);
        this.carregar();
      },
      error: (e) => {
        this.erroEdicao.set(mensagemDeErro(e, 'Nao foi possivel salvar.'));
        this.salvando.set(false);
      },
    });
  }

  protected excluir(): void {
    const url = this.dados();

    if (!url || !confirm(`Excluir ${url.shortUrl}? O historico de cliques vai junto.`)) {
      return;
    }

    this.urls.excluir(this.id).subscribe({
      next: () => this.router.navigate(['/painel']),
      error: (e) => this.erro.set(mensagemDeErro(e, 'Nao foi possivel excluir.')),
    });
  }
}
