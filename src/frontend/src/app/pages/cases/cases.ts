import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-cases',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Cases"></app-page-title>
      <p>Cases page placeholder.</p>
    </section>
  `,
  styleUrls: ['./cases.css'],
  imports: [PageTitleComponent]
})
export class CasesComponent {}
