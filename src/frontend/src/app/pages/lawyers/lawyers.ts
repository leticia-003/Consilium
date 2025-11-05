import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-lawyers',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Lawyers"></app-page-title>
      <p>Lawyers page placeholder.</p>
    </section>
  `,
  styleUrls: ['./lawyers.css'],
  imports: [PageTitleComponent]
})
export class LawyersComponent {}
