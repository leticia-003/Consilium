import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-home',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Home"></app-page-title>
      <p>Welcome to Consilium dashboard (placeholder).</p>
    </section>
  `,
  styleUrls: ['./home.css'],
  imports: [PageTitleComponent]
})
export class HomeComponent {}
