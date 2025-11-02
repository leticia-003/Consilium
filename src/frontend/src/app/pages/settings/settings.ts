import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-settings',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Settings"></app-page-title>
      <p>Settings page placeholder.</p>
    </section>
  `,
  styleUrls: ['./settings.css'],
  imports: [PageTitleComponent]
})
export class SettingsComponent {}
