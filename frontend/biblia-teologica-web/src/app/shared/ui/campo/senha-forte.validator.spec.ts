import { FormControl } from '@angular/forms';
import { senhaForte } from './senha-forte.validator';

describe('senhaForte', () => {
  const erroPara = (v: string) => senhaForte()(new FormControl(v));

  it('senhaForte_aceita_quandoTemOitoCaracteresMaiusculaMinusculaEDigito', () => {
    expect(erroPara('Abcdefg1')).toBeNull();
  });

  it('senhaForte_recusa_quandoTemMenosDeOitoCaracteres', () => {
    expect(erroPara('Abcde1')).toEqual({ senhaForte: true });
  });

  it('senhaForte_recusa_quandoFaltaMaiuscula', () => {
    expect(erroPara('abcdefg1')).toEqual({ senhaForte: true });
  });

  it('senhaForte_recusa_quandoFaltaMinuscula', () => {
    expect(erroPara('ABCDEFG1')).toEqual({ senhaForte: true });
  });

  it('senhaForte_recusa_quandoFaltaDigito', () => {
    expect(erroPara('Abcdefgh')).toEqual({ senhaForte: true });
  });

  it('senhaForte_deixaOCampoVazioParaORequired', () => {
    expect(erroPara('')).toBeNull();
  });
});
