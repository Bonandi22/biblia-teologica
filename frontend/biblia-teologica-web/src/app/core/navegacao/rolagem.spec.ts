import { Component } from '@angular/core';
import { Location, ViewportScroller } from '@angular/common';
import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { NavigationEnd, provideRouter, Router } from '@angular/router';
import { filter, firstValueFrom } from 'rxjs';
import { rolarAoTopoAoTrocarDePagina } from './rolagem';

@Component({ template: '' })
class VazioComponent {}

describe('rolarAoTopoAoTrocarDePagina', () => {
  const montar = async () => {
    TestBed.configureTestingModule({
      providers: [
        provideLocationMocks(),
        provideRouter([
          { path: '', component: VazioComponent },
          { path: 'biblia/:slug', component: VazioComponent },
          { path: 'biblia/:slug/:aba', component: VazioComponent },
          { path: 'estudos', component: VazioComponent },
        ]),
      ],
    });
    const rolar = vi.spyOn(TestBed.inject(ViewportScroller), 'scrollToPosition').mockImplementation(() => undefined);
    TestBed.runInInjectionContext(() => rolarAoTopoAoTrocarDePagina());
    const router = TestBed.inject(Router);
    router.initialNavigation();
    await TestBed.inject(Router).navigateByUrl('/');
    rolar.mockClear();
    return { router, rolar };
  };

  it('rola ao topo quando a página muda', async () => {
    const { router, rolar } = await montar();
    await router.navigateByUrl('/biblia/obadias');
    expect(rolar).toHaveBeenCalledWith([0, 0]);
  });

  it('rola ao topo ao passar para outro livro', async () => {
    const { router, rolar } = await montar();
    await router.navigateByUrl('/biblia/obadias/contexto');
    rolar.mockClear();
    await router.navigateByUrl('/biblia/jonas/contexto');
    expect(rolar).toHaveBeenCalledWith([0, 0]);
  });

  it('mantém a posição ao trocar de aba do mesmo livro', async () => {
    const { router, rolar } = await montar();
    await router.navigateByUrl('/biblia/isaias');
    rolar.mockClear();
    await router.navigateByUrl('/biblia/isaias/contexto');
    await router.navigateByUrl('/biblia/isaias/mundo');
    expect(rolar).not.toHaveBeenCalled();
  });

  it('não interfere no botão voltar do navegador', async () => {
    const { router, rolar } = await montar();
    await router.navigateByUrl('/estudos');
    rolar.mockClear();
    const fim = firstValueFrom(router.events.pipe(filter((e) => e instanceof NavigationEnd)));
    TestBed.inject(Location).back();
    await fim;
    expect(router.url).toBe('/');
    expect(rolar).not.toHaveBeenCalled();
  });
});
