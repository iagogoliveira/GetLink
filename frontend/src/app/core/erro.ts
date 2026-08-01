import { HttpErrorResponse } from '@angular/common/http';

/**
 * O backend responde erro em tres formatos diferentes conforme a origem:
 * string pura (BadRequest(ex.Message)), objeto { message } (Unauthorized do
 * login) e ProblemDetails com { errors } (validacao automatica do [ApiController]).
 * Esta funcao reduz os tres a uma frase exibivel.
 */
export function mensagemDeErro(erro: unknown, padrao = 'Algo deu errado. Tente novamente.'): string {
  if (!(erro instanceof HttpErrorResponse)) {
    return padrao;
  }

  if (erro.status === 0) {
    return 'Nao foi possivel falar com o servidor. Ele esta rodando?';
  }

  if (erro.status === 429) {
    return 'Tentativas demais. Aguarde um minuto e tente de novo.';
  }

  const corpo = erro.error;

  if (typeof corpo === 'string' && corpo.trim()) {
    return corpo;
  }

  if (corpo && typeof corpo === 'object') {
    if (typeof corpo.message === 'string') {
      return corpo.message;
    }

    // ProblemDetails de validacao: { errors: { Campo: ["msg", ...] } }
    if (corpo.errors && typeof corpo.errors === 'object') {
      const primeira = Object.values(corpo.errors as Record<string, string[]>)
        .flat()
        .find((m) => typeof m === 'string' && m.trim());

      if (primeira) {
        return primeira;
      }
    }

    if (typeof corpo.title === 'string') {
      return corpo.title;
    }
  }

  return padrao;
}
