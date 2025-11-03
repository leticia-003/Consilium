import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-create-client',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Create Client"></app-page-title>
      <div>
        <h3>Placeholder</h3>
        <p>This is a placeholder page for creating a new client. The form will be implemented later.</p>
      </div>
    </section>
  `,
  imports: [CommonModule, PageTitleComponent]
})
export class CreateClientComponent {}
