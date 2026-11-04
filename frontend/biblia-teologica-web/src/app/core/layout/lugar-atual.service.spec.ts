import { TestBed } from '@angular/core/testing';
import { LugarAtualService } from './lugar-atual.service';

describe('LugarAtualService', () => {
  it('começa vazio, define e limpa o texto', () => {
    const servico = TestBed.inject(LugarAtualService);
    expect(servico.texto()).toBe('');
    servico.definir('Isaías 1–66');
    expect(servico.texto()).toBe('Isaías 1–66');
    servico.limpar();
    expect(servico.texto()).toBe('');
  });
});
