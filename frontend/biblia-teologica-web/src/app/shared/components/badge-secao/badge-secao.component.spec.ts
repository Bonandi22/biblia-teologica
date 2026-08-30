import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { BadgeSecaoComponent } from './badge-secao.component';

@Component({
  standalone: true,
  imports: [BadgeSecaoComponent],
  template: `<bt-badge-secao rotulo="PENTATEUCO" />`,
})
class HostComponent {}

it('mostra o rótulo da seção', () => {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  expect(fixture.nativeElement.textContent).toContain('PENTATEUCO');
});
